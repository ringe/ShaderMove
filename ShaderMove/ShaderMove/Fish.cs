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
            fish.CopyAbsoluteBoneTransformsTo(fishMatrix);
            size = Matrix.CreateScale(sz);
            position = pos;
        }

        public Fish(ContentManager content, float[,] height)
        {
            fish = content.Load<Model>(@"Content\Sheephead0");
            fishMatrix = new Matrix[fish.Bones.Count];
            fish.CopyAbsoluteBoneTransformsTo(fishMatrix);
            size = Matrix.CreateScale(8);
            Random r = new Random();

            //position = new Vector3(r.Next(-133, 133), 0, r.Next(-133, 133));
            //position.Y = r.Next(height[position.X, position.Z], 
        }
    }
}
