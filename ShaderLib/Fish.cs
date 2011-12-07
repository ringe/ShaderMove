using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace PondLibs
{
    public class Fish
    {
        private const float moveRate = 20.0f;
        protected Vector3 movement = Vector3.Zero;
        private float[,] heightMap;
        private int terrainX;
        private int terrainZ;
        private float waterLevel;

        private Model fish;
        private Matrix[] fishMatrix;
        private Matrix size;
        private Vector3 position;
        private Vector3 startPos;
        private Quaternion rotation = Quaternion.Identity;
        private BoundingSphere boundingSphere;
        Vector3 newPos;

        // Store world between draws for collision detection
        private Matrix world;

        public Matrix scale { get { return size; } }

        // Set position TODO: doesn't work with the heightmap (scale sync issue)
        public Vector3 pos {
            get { return position; }
            set {
                newPos = value;
                int x = (int)newPos.X;
                int z = (int)newPos.Z;
                if (newPos.Y > waterLevel)
                    newPos.Y = waterLevel;
                try
                {
                    if (newPos.Y < heightMap[x + terrainX / 2, -z + terrainZ / 2])
                        newPos.Y = heightMap[x + terrainX / 2, -z + terrainZ / 2];
                }
                catch { // no problem, we'll kill you outside the terrain later
                }
                position = newPos;
            }
        }
        public ModelMeshCollection meshes { get { return fish.Meshes; } }
        public Matrix[] matrix { get { return fishMatrix; } }
        public ModelBoneCollection bones { get { return fish.Bones; } }
        public Matrix World
        {
            get { return world; }
            set { world = value; }
        }
        public BoundingSphere Sphere { get { return boundingSphere; } }

        // Score for Player
        private int points;
        private bool dead;
        
        public int score { 
            get { return points; }
            set {
                points += value;
                size *= Matrix.CreateScale(1.0f + (float)points/1000.0f);
            }
        }
        public Boolean Alive
        {
            get { return !dead; }
            set { dead = false; }
        }

        public void CopyAbsoluteBoneTransformsTo(ref Matrix[] mtr)
        {
            fish.CopyAbsoluteBoneTransformsTo(mtr);
        }

        // Constructor for player fish
        public Fish(ContentManager content, Vector3 pos, float[,] height, float waterL, int tszX, int tszZ)
        {
            heightMap = height;
            waterLevel = waterL;
            terrainX = tszX;
            terrainZ = tszZ;

            // Load models
            fish = content.Load<Model>(@"Content\Sheephead0");
            fishMatrix = new Matrix[fish.Bones.Count];
            fish.CopyAbsoluteBoneTransformsTo(fishMatrix);

            // Set starting size
            points = 30;
            size = Matrix.CreateScale(points/10);

            startPos = position = pos;
            createBoundingSphere();
        }

        // Constructor for oppenent fish
        public Fish(ContentManager content, float[,] height, float waterL, int tszX, int tszZ)
        {
            heightMap = height;
            waterLevel = waterL;
            terrainX = tszX;
            terrainZ = tszZ;
            Random r = new Random();

            fish = content.Load<Model>(@"Content\Sheephead0");
            fishMatrix = new Matrix[fish.Bones.Count];
            fish.CopyAbsoluteBoneTransformsTo(fishMatrix);

            // Set the size of the opponent
            points = r.Next(5, 50);
            size = Matrix.CreateScale(points/10);

            // Get the terrain factors
            int teX = terrainX / 2;
            int teZ = terrainZ / 2;

            int x = 0, z = 0; float y = 0.0f;
            //while (y > waterLevel)
            while (y == 0.0f)
            {
                x = r.Next(-teX, teX);
                z = r.Next(-teZ, teZ);

                float h = height[x+teX-1, -z+teZ-1];

                if (h < waterLevel)
                    y = (waterLevel - h) * (float)r.NextDouble() + h;
            }

            startPos = position = new Vector3(x, y, z);
            createBoundingSphere();
        }

        public Quaternion Rotation
        { 
            get { return rotation; }
            set { rotation = value; }
        }

        private void createBoundingSphere()
        {
            BoundingSphere sphere = new BoundingSphere(Vector3.Zero, 0);

            // Merge all the model's built in bounding spheres
            foreach (ModelMesh mesh in fish.Meshes)
            {
                BoundingSphere transformed = mesh.BoundingSphere.Transform(
                    fishMatrix[mesh.ParentBone.Index]);

                sphere = BoundingSphere.CreateMerged(sphere, transformed);
            }

            this.boundingSphere = sphere;
        }

        // Did this fish just hit a Fish?
        public bool hits(Fish fishToHit, Matrix hitWorld)
        {
            BoundingSphere hitSphere = fishToHit.Sphere;
            BoundingSphere sphere1 = hitSphere.Transform(hitWorld);

            BoundingSphere hitSphere2 = Sphere;
            BoundingSphere sphere2 = hitSphere.Transform(World);

            bool collision = sphere1.Intersects(sphere2);

            if (collision)
                if (score > fishToHit.score)
                    score = fishToHit.score;
                else
                {
                    score = -fishToHit.score;
                    position = startPos;
                    dead = true;
                }

            return collision;
        }
    }
}
