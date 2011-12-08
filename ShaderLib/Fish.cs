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
    /// <summary>
    /// The Fish is the representative of the players in this game. Eat or be eaten.
    /// </summary>
    public class Fish
    {
        private const float moveRate = 20.0f;
        protected Vector3 movement = Vector3.Zero;
        private float[,] heightMap;
        private int terrainX;
        private int terrainZ;
        private float waterLevel;

        private Model fish;
        public Matrix[] fishMatrix;
        private Matrix size;
        private float lastSize;
        private Vector3 position;
        private Vector3 startPos;
        private Quaternion rotation = Quaternion.Identity;
        private BoundingSphere boundingSphere;
        private Vector3 newPos;

        // Store world between draws for collision detection
        private Matrix world;

        // Return Scale Matrix
        public Matrix Scale { get { return size; } }

        // Update position, restrict movement to water.
        public Vector3 Position {
            get { return position; }
            set {
                newPos = value;
                int x = (int)newPos.X;
                int z = (int)newPos.Z;
                if (newPos.Y > waterLevel)
                    newPos.Y = waterLevel;

                // Attempt to get height of current position
                try {
                    if (newPos.Y < heightMap[x + terrainX / 2, -z + terrainZ / 2])
                        newPos.Y = heightMap[x + terrainX / 2, -z + terrainZ / 2];
                }
                catch { }// no problem, we'll kill you outside the terrain later

                // Don't allow the fish to leave water.
                if (newPos.Y > waterLevel) return;

                // Don't leave outside the terrain - TODO if issue
                //if (terrainX/2 < newPos.X || newPos.X > -terrainX/2) return;
                //if (newPos.Z > terrainZ/2 || newPos.Z/2 < -terrainZ) return;

                // Update position.
                position = newPos;
            }
        }
        public ModelMeshCollection meshes { get { return fish.Meshes; } }
        public Matrix[] matrix { get { return fishMatrix; } }
        public ModelBoneCollection bones { get { return fish.Bones; } }

        // return world matrix
        public Matrix World
        {
            get { return world; }
            set { world = value; }
        }

        // Retrun Bounding Sphere
        public BoundingSphere Sphere { get { return boundingSphere; } }

        // Score for Player
        private float points;
        private bool dead;
        private Boolean winner;
        
        // Show public score and change size when given more points
        public int score { 
            get { return (int)points; }
            set
            {
                points += (value / 10.0f);
                float newSize = points / 20.0f;
                if (newSize > lastSize) // Change size?
                    if (newSize > 6f) newSize = 6f; // Not any bigger than this
                        size = Matrix.CreateScale(newSize); // New size.
            }
        }

        // Is this Fish alive?
        public Boolean Alive
        {
            get { return !dead; }
            set { dead = value; }
        }

        // Is this Fish a Winner?
        public Boolean Won
        {
            get { return winner; }
            set { winner = value; }
        }

        // Public access to the rotation variable
        public Quaternion Rotation
        {
            get { return rotation; }
            set { rotation = value; }
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
            points = 42;
            lastSize = points / 20.0f;
            size = Matrix.CreateScale(lastSize);

            // Set starting position and create bounding sphere
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
            points = r.Next(20, 80);
            lastSize = points / 20.0f;
            size = Matrix.CreateScale(lastSize);

            // Calculate the terrain factors
            int teX = terrainX / 2;
            int teZ = terrainZ / 2;

            // Generate a random position under the surface, over the sea floor
            int x = 0, z = 0; float y = 0.0f;
            while (y == 0.0f)
            {
                // random x and y
                x = r.Next(-teX, teX);
                z = r.Next(-teZ, teZ);

                // find height, catch out of range to re-run position
                float h = 0.0f;
                try
                {
                    h = height[x + teX - 1, -z + teZ - 1];
                } catch { }

                // if the height found is below the water level, we're in the water
                // otherwise, re-run position
                if (h < waterLevel && h != 0.0f)
                    y = (waterLevel - h) * (float)r.NextDouble() + h;
            }

            // Set starting position and create bounding sphere
            startPos = position = new Vector3(x, y, z);
            createBoundingSphere();
        }

        // Copy bonetransforms to matrix
        public void CopyAbsoluteBoneTransformsTo(ref Matrix[] mtr)
        {
            fish.CopyAbsoluteBoneTransformsTo(mtr);
        }

        // Create a bounding sphere for collision detection
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
