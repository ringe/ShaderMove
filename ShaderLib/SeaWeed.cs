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

namespace PondLibs
{
    public class SeaWeed
    {
        private Random rnd = new Random();
        private Vector3 position;

        public SeaWeed(int terrainWidth, int[,] height)
        {
            int x = rnd.Next(0, terrainWidth);
            int z = rnd.Next(0, terrainWidth);
            position = new Vector3(x, height[x,z], z);
            InitVertices();
        }

        private void InitVertices()
        {
            //vertices = new Vector3[] {
            //    new Vector3(0, 0, 0),
            //    new Vector3(0, 0, 1),
            //    new Vector3(1, 0, 0),
            //    new Vector3(0, 1, 1),
            //};
        }
    }
}
