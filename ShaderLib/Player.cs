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
    public class Player : Fish
    {
        //En referanse til input-komponenten: 
        protected IInputHandler input;
        private const float moveRate = 20.0f;
        protected Vector3 movement = Vector3.Zero;
        private float cameraYaw = 0.0f;
        private float cameraPitch = 0.0f;
        private const float spinRate = 40.0f;

        public Player (ContentManager content, float sz, Vector3 pos, Game game)
            : base(content, sz, pos) {
            //Henter ut en referanse til input-handleren: 
            input = (IInputHandler)game.Services.GetService(typeof(IInputHandler));
        }

        public Player(ContentManager content, float[,] height, float water)
            : base(content, height, water)
        {
        }

        public void Update(GameTime gameTime)
        {
            //timeDelta = tiden mellom to kall på Update 
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // update position based on keys
            movement = Vector3.Zero;
            if (input.KeyboardState.IsKeyDown(Keys.A))
                movement.X--;
            if (input.KeyboardState.IsKeyDown(Keys.D))
                movement.X++;
            if (input.KeyboardState.IsKeyDown(Keys.S))
                movement.Z++;
            if (input.KeyboardState.IsKeyDown(Keys.W))
                movement.Z--;
             
            if (input.KeyboardState.IsKeyDown(Keys.Left))
                cameraYaw = cameraYaw + (spinRate * timeDelta);
            if (input.KeyboardState.IsKeyDown(Keys.Right))
                cameraYaw = cameraYaw - (spinRate * timeDelta);

            if (cameraYaw > 360)
                cameraYaw -= 360;
            else if (cameraYaw < 0)
                cameraYaw += 360;

            //OPP/NED (PITCH): 
            if (input.KeyboardState.IsKeyDown(Keys.Down))
                cameraPitch = cameraPitch - (spinRate * timeDelta);
            if (input.KeyboardState.IsKeyDown(Keys.Up))
                cameraPitch = cameraPitch + (spinRate * timeDelta);
            if (cameraPitch > 89)
                cameraPitch = 89;
            else if (cameraPitch < -89)
                cameraPitch = -89;


            if (movement.LengthSquared() != 0)
                movement.Normalize();
            // Posisjoner kamera: 
            Matrix rotationMatrix;

            //Rotasjonsmatrise om Y-aksen: 
            Matrix.CreateRotationY(MathHelper.ToRadians(cameraYaw), out rotationMatrix);

            //Legger til pitch dvs. rotasjon om X‐aksen:
            Rotation = Matrix.CreateRotationX(MathHelper.ToRadians(cameraPitch)) * rotationMatrix;


            //FirstPersonCamera, endrer kameraets posisjon: 
            movement *= (moveRate * timeDelta);
            if (movement != Vector3.Zero)
            {
                //Roterer movement-vektoren: 
                Vector3.Transform(ref movement, ref rotationMatrix, out movement);

                //Oppdaterer kameraposisjonen med move-vektoren:  
                base.pos += movement;
            }
        }
    }
}
