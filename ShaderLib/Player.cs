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
        protected IInputHandler input; //Reference to the input component
        private const float moveRate = 30.0f;
        private float cameraYaw = 0.0f; // position of the yaw(sideways tilt)
        private float cameraPitch = 0.0f; // position of the pitch(tilt)
        private const float spinRate = 100.0f;


        public Player(ContentManager content, Vector3 pos, Game game, float[,] height, float water, int terrX, int terrZ)
            : base(content, pos, height, water, terrX, terrZ) {
            //Reference to the input handeler 
            input = (IInputHandler)game.Services.GetService(typeof(IInputHandler));
        }

        public Player(ContentManager content, float[,] height, float water, int terrX, int terrZ)
            : base(content, height, water, terrX, terrZ)
        {
        }

        public void Update(GameTime gameTime)
        {
            //timeDelta = time between two calls on Update
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Alive)
            {
                //Checks for keyboard clicks on left/right and updates the position
                movement = Vector3.Zero;
                if (input.KeyboardState.IsKeyDown(Keys.A))
                    cameraYaw = cameraYaw + (spinRate * timeDelta); //movement.X--;
                if (input.KeyboardState.IsKeyDown(Keys.D))
                    cameraYaw = cameraYaw - (spinRate * timeDelta); // movement.X++;
                if (input.KeyboardState.IsKeyDown(Keys.S))
                {
                    movement.Z++;
                    movement.Y += cameraPitch / 50;
                }
                if (input.KeyboardState.IsKeyDown(Keys.W))
                {
                    movement.Z--;
                    movement.Y -= cameraPitch / 50;
                }

                if (cameraYaw > 360)
                    cameraYaw -= 360;
                else if (cameraYaw < 0)
                    cameraYaw += 360;

                //Checks for keyboard clicks on up/down(pitch) and updates the pitch
                if (input.KeyboardState.IsKeyDown(Keys.Down))
                    cameraPitch = cameraPitch + (spinRate * timeDelta);
                if (input.KeyboardState.IsKeyDown(Keys.Up))
                    cameraPitch = cameraPitch - (spinRate * timeDelta);
                if (cameraPitch > 89)
                    cameraPitch = 89;
                else if (cameraPitch < -89)
                    cameraPitch = -89;

                if (movement.LengthSquared() != 0)
                    movement.Normalize();

                // Rotation matrix for the camera
                Matrix rotationMatrix;

                //Rotation matrix around Y 
                Matrix.CreateRotationY(MathHelper.ToRadians(cameraYaw), out rotationMatrix);

                //Rotation matrix around Z (pitch)
                Rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationZ(MathHelper.ToRadians(cameraPitch))
                    * rotationMatrix
                    * Matrix.CreateRotationY((float)(Math.PI * 6 / 4))
                    );

                //Change camera position 
                movement *= (moveRate * timeDelta);
                if (movement != Vector3.Zero)
                {
                    //Rotates the movement vector
                    Vector3.Transform(ref movement, ref rotationMatrix, out movement);

                    //Updates the camera position with the move vector  
                    base.pos += movement;
                }
            }
        }


        
    }
}
