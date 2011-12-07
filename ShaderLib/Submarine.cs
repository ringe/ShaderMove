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
    public struct VertexPositionColorTextureNormal
    {
        private Vector3 position;
        private Color color;
        private Vector2 texcoord;
        private Vector3 normal;

        public VertexPositionColorTextureNormal(Vector3 position, Color color, Vector2 texcoord, Vector3 normal)
        {
            this.position = position;
            this.color = color;
            this.texcoord = texcoord;
            this.normal = normal;
        }

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );

    }

    public class SubMarine
    {
        // Kalkulasjonsfaktorer
        float c = (float)Math.PI / 180.0f; //Opererer med radianer. 
        float phir = 0.0f;
        float phir20 = 0.0f;
        float thetar = 0.0f;
        float x = 0.0f, y = 0.0f, z = 0.0f;
        int i = 0;

        // Vertices
        VertexPositionColorTextureNormal[] vertices = new VertexPositionColorTextureNormal[1406];
        private Texture2D texture;

        // Position
        Vector3 position;

        public Texture2D Texture { get { return texture; } }
        public Vector3 Position { get { return position; } }
        public VertexPositionColorTextureNormal[] Vertices { get { return vertices; } }

        public SubMarine(Texture2D text, Vector3 pos)
        {
            texture = text;
            position = pos;
            position.Y += 5;
            InitVertices();
        }

        private void InitVertices() {
            //Varierer fi: 
            for (float phi = -90.0f; phi <= 90.0f; phi += 10)
            {
                phir = c * phi;   //phi radianer
                phir20 = c * (phi + 10);  //(phi+10) radianer

                float min = 0.001f; float max = 0.999f;
                //Varierer teta: 
                for (float theta = -180.0f; theta <= 180.0f; theta += 10)
                {
                    thetar = c * theta;
                    //Her skal x,y og z beregnes for pkt.1-3-5-7...:
                    x = (float)(Math.Sin(thetar) * Math.Cos(phir));
                    y = (float)(Math.Cos(thetar) * Math.Cos(phir));
                    z = (float)(Math.Sin(phir));
                    vertices[i] = new VertexPositionColorTextureNormal(new Vector3(x, y, z), Color.Red, new Vector2(min, max), new Vector3(0, 0, 0));
                    //if ((i == 2) || (i == 1404))
                    //{
                    //    //verticesSphere[i].Position = new Vector3(0, 0, 0);
                    //    verticesSphere[i].Color = Color.Blue;
                    //}
                    i++;

                    //Her skal x,y og z beregnes for pkt.2-4-6-8  
                    x = (float)(Math.Sin(thetar) * Math.Cos(phir20));
                    y = (float)(Math.Cos(thetar) * Math.Cos(phir20));
                    z = (float)(Math.Sin(phir20));
                    vertices[i] = new VertexPositionColorTextureNormal(new Vector3(x, y, z), Color.Red, new Vector2(min, max), new Vector3(0, 0, 0));
                    //vertices[i].Position = new Vector3(x, y, z);
                    //vertices[i].Color = Color.White;
                    //vertices[i].TextureCoordinate = new Vector2(min, max);
                    i++;
                }
            }
        }

        //private void CalculateNormals()
        //{
        //    for (int i = 0; i < vertices.Length; i++)
        //        vertices[i].Normal = new Vector3(0, 0, 0);

        //    for (int i = 0; i < vertices.Length / 3; i++)
        //    {
        //        int index1 = terrainIndices[i * 3];
        //        int index2 = terrainIndices[i * 3 + 1];
        //        int index3 = terrainIndices[i * 3 + 2];

        //        Vector3 side1 = terrainVertices[index1].Position - terrainVertices[index3].Position;
        //        Vector3 side2 = terrainVertices[index1].Position - terrainVertices[index2].Position;
        //        Vector3 normal = Vector3.Cross(side1, side2);

        //        terrainVertices[index1].Normal += normal;
        //        terrainVertices[index2].Normal += normal;
        //        terrainVertices[index3].Normal += normal;
        //    }

        //    for (int i = 0; i < terrainVertices.Length; i++)
        //        terrainVertices[i].Normal.Normalize();
        //}

    }
}
