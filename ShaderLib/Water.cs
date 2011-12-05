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
    public struct Drop
    {
        public float BirthTime;
        public float MaxAge;
        public Vector2 OrginalPosition;
        public Vector2 Accelaration;
        public Vector2 Direction;
        public Vector2 Position;
        public float Scaling;
        public Color ModColor;
    }

    public class Water
    {
        Random randomizer = new Random();
        private List<Drop> dropList;
        private Texture2D dropTexture;

        public Water(Texture2D texture)
        {
            dropTexture = texture;
            dropList = new List<Drop>();
        }

        private void AddDrops(Vector2 pos, int amount, float size, float maxAge, GameTime gameTime)
        {
            for (int i = 0; i < amount; i++)
                AddDrop(pos, size, maxAge, gameTime);
        }

        private void AddDrop(Vector2 pos, float size, float maxAge, GameTime gameTime)
        {
            Drop drop = new Drop();

            drop.OrginalPosition = pos;
            drop.Position = drop.OrginalPosition;

            drop.BirthTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
            drop.MaxAge = maxAge;
            drop.Scaling = 0.25f;
            drop.ModColor = Color.White;

            float particleDistance = (float)randomizer.NextDouble() * size;
            Vector2 displacement = new Vector2(particleDistance, 0);
            float angle = MathHelper.ToRadians(randomizer.Next(360));
            displacement = Vector2.Transform(displacement, Matrix.CreateRotationZ(angle));

            drop.Direction = displacement;
            drop.Accelaration = 3.0f * drop.Direction;

            dropList.Add(drop);
        }

        public void Draw(ref SpriteBatch spriteBatch)
        {
            for (int i = 0; i < dropList.Count; i++)
            {
                 Drop particle = dropList[i];
                 spriteBatch.Draw(dropTexture, particle.Position, null, particle.ModColor, i, new Vector2(256, 256), particle.Scaling, SpriteEffects.None, 1);
            }
        }
    }
}
