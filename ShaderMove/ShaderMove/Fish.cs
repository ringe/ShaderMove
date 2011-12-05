using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ShaderMove
{
    class Fish
    {
        private Model fish;
        private Matrix[] fishMatrix;
        private Matrix size;
        private Vector3 position;
        private  Matrix rotation;

        public Matrix scale { get { return size; } }
        public Vector3 pos { get { return position; } }
        public ModelMeshCollection meshes { get { return fish.Meshes; } }
        public Matrix[] matrix { get { return fishMatrix; } }
        public ModelBoneCollection bones { get { return fish.Bones; } }

        public void CopyAbsoluteBoneTransformsTo(ref Matrix[] mtr)
        {
            fish.CopyAbsoluteBoneTransformsTo(mtr);
        }

        public Fish (ContentManager content, float sz, Vector3 pos){
            // Load models
            fish = content.Load<Model>(@"Content\Sheephead0");
            fishMatrix = new Matrix[fish.Bones.Count];
            rotation = Matrix.CreateRotationY((float)(Math.PI * 6 / 4));
            fish.CopyAbsoluteBoneTransformsTo(fishMatrix);
            size = Matrix.CreateScale(sz);
            position = pos;
        }

        public Fish(ContentManager content, float[,] height, float water)
        {
            fish = content.Load<Model>(@"Content\Sheephead0");
            fishMatrix = new Matrix[fish.Bones.Count];
            fish.CopyAbsoluteBoneTransformsTo(fishMatrix);
            size = Matrix.CreateScale(8);
            Random r = new Random();

            int x = r.Next(0, 266);
            int z = r.Next(0, 266);
            float h = height[x, z];
            // TODO: Y should be between h and the water level
            // WHAT IS h? the fish location is different from the terrain and the water

            rotation = Matrix.CreateRotationY((float)(Math.PI * 6 / 4));
            position = new Vector3(x-133, h, z-133);
        }

        public Matrix Rotation { 
            get { return rotation; }
        }
    }
}
