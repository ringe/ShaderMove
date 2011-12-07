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
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Camera : Microsoft.Xna.Framework.GameComponent
    {

        protected IInputHandler input; //Reference to the input component
        private const float moveRate = 20.0f;      // for FirstPersonCamra. 
        protected Vector3 movement = Vector3.Zero; // for FirstPersonCamera.
        protected float[,] heightData; // for FirstPersonCamera

        private GraphicsDeviceManager graphics;
        private Matrix projection;
        private Matrix view;
        private Vector3 cameraPosition = new Vector3(76, 24, 76); //Position of the camera
        private Vector3 cameraTarget = Vector3.Zero; //Target of the camera
        private Vector3 cameraUpVector = Vector3.Up; //Up vector on the camera
        private Vector3 cameraReference = new Vector3(0, -.3f, -1.0f);
        private float cameraYaw = 0.0f; // position of the yaw(sideways tilt)
        private float cameraPitch = 0.0f; // position of the pitch(tilt)
        private const float spinRate = 40.0f;

        //Makes the view-, projection- and position matrix available trough properties
        public Matrix View
        {
            get { return view; }
            set { view = value; }
        }

        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; }
        }

        public Vector3 Position
        {
            get { return cameraPosition; }
        }

        public Camera(Game game)
            : base(game)
        {
            graphics = (GraphicsDeviceManager)game.Services.GetService(typeof(IGraphicsDeviceManager));
            //Reference to the input handeler 
            input = (IInputHandler)game.Services.GetService(typeof(IInputHandler));
        }

        public override void Initialize()
        {
            base.Initialize();
            this.InitializeCamera();
        }

        private void InitializeCamera()
        {
            float aspectRatio = (float)graphics.GraphicsDevice.Viewport.Width /
                (float)graphics.GraphicsDevice.Viewport.Height;

            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1.0f, 1000.0f, out projection);
            Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out view);
        }

        public void setTarget(Vector3 targ)
        {
            cameraTarget = targ;
        }


        public override void Update(GameTime gameTime)
        {
            //timeDelta = time between two calls on Update
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Checks for keyboard clicks on left/right and updates the position
            if (input.KeyboardState.IsKeyDown(Keys.Left))
                cameraYaw = cameraYaw + (spinRate * timeDelta);
            if (input.KeyboardState.IsKeyDown(Keys.Right))
                cameraYaw = cameraYaw - (spinRate * timeDelta);

            if (cameraYaw > 360)
                cameraYaw -= 360;
            else if (cameraYaw < 0)
                cameraYaw += 360;

            //Checks for keyboard clicks on up/down(pitch) and updates the pitch
            if (input.KeyboardState.IsKeyDown(Keys.Down))
                cameraPitch = cameraPitch - (spinRate * timeDelta);
            if (input.KeyboardState.IsKeyDown(Keys.Up))
                cameraPitch = cameraPitch + (spinRate * timeDelta);
            if (cameraPitch > 89)
                cameraPitch = 89;
            else if (cameraPitch < -89)
                cameraPitch = -89;

            // Rotation matrix for the camera
            Matrix rotationMatrix;

            //Rotation matrix around Y
            Matrix.CreateRotationY(MathHelper.ToRadians(cameraYaw), out rotationMatrix);

            //Rotation matrix around X (pitch)
            rotationMatrix = Matrix.CreateRotationX(MathHelper.ToRadians(cameraPitch)) * rotationMatrix;

            //Change camera position 
            movement *= (moveRate * timeDelta);
            if (movement != Vector3.Zero)
            {
                //Rotates the movement vector
                Vector3.Transform(ref movement, ref rotationMatrix, out movement);
                //Updates the camera position with the move vector
                cameraPosition += movement;
            }

            //Sets position at the floor level
            if (heightData != null)
                cameraPosition.Y = heightData[(int)cameraPosition.X, (int)cameraPosition.Z];

            //Creates a vector to point in the direction of the camera view
            Vector3 transformedReference;
 
            //Rotates the cameraReference vector
            Vector3.Transform(ref cameraReference, ref rotationMatrix, out transformedReference);

            //Calculates what the camera is watching on using the position vector and the direction vector
            Vector3.Add(ref cameraPosition, ref transformedReference, out cameraTarget);

            //Oppdaterer view-matrisa vha. posisjons, kameramål og opp-vektorene: 
            //Updates the view matrix using position vector, cameraTarget and up vector.
            Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out view);

            base.Update(gameTime);
        }
    }
}
