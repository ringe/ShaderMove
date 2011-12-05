using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace PondLibs
{
    public class FirstPersonCamera : Camera
    {
        public FirstPersonCamera(Game game)
            : base(game)
        {
        }

        public override void Update(GameTime gameTime)
        {
            movement = Vector3.Zero;
            if (input.KeyboardState.IsKeyDown(Keys.A))
                movement.X--;
            if (input.KeyboardState.IsKeyDown(Keys.D))
                movement.X++;
            if (input.KeyboardState.IsKeyDown(Keys.S))
                movement.Z++;
            if (input.KeyboardState.IsKeyDown(Keys.W))
                movement.Z--;

            //Sikrer oss at farta ikke øker dersom vi holder nede  
            //både D og S samtidig (diagonal bevegelse): 
            if (movement.LengthSquared() != 0)
                movement.Normalize();

            base.Update(gameTime);
        }
    }
}