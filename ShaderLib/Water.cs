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

namespace ShaderLib
{
    // Water vertex:
    public struct DropVertex
    {
        public Vector3 position;
        public Vector2 textureCoordinate;
         
        public DropVertex(Vector3 position, Vector2 textureCoordinate)
        {
            this.position = position;
            this.textureCoordinate = textureCoordinate;
        }
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
                (
                    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
                );
    }

    // Water particles
    struct Drop
    {
        public float size;
        public Vector3 direction;
        public DropVertex [] pvs; //ParticleVertexeS (PVS)

        public Drop(Vector3 _position, Vector2 _textureCoordinates, Vector3 _direction, float _size) {
            Vector3 p1,p2,p3;
            this.direction = _direction;
            this.size = _size;
            pvs = new DropVertex[3];
            p1 = new Vector3(_position.X, _position.Y, _position.Z);
            p2 = new Vector3(_position.X + size, _position.Y, _position.Z);
            p3 = new Vector3(_position.X, _position.Y + size, _position.Z);
            Vector2 tc1, tc2, tc3;
            tc1 = new Vector2(_textureCoordinates.X, _textureCoordinates.Y);
            tc2 = new Vector2(_textureCoordinates.X + size, _textureCoordinates.Y);
            tc3 = new Vector2(_textureCoordinates.X, _textureCoordinates.Y + size);

            //Oppretter 3 vertekser for denne partikkelen:
            pvs[0].position = p1;
            pvs[0].textureCoordinate = tc1;
            pvs[1].position = p2;
            pvs[1].textureCoordinate = tc2;
             
            pvs[2].position = p3;
            pvs[2].textureCoordinate = tc3;
        }

        public void UpdatePosition() {
            pvs[0].position += this.direction;
            pvs[1].position += this.direction;
            pvs[2].position += this.direction;
        }
        public void UpdateTextureCoordinate(Vector2 newCoordinates) {
            Vector2 tc1, tc2, tc3;
            tc1 = new Vector2(newCoordinates.X, newCoordinates.Y);
            tc2 = new Vector2(newCoordinates.X + size, newCoordinates.Y);
            tc3 = new Vector2(newCoordinates.X, newCoordinates.Y + size);
            pvs[0].textureCoordinate = tc1;
            pvs[1].textureCoordinate = tc2;
            pvs[2].textureCoordinate = tc3;
        }
    }
}
