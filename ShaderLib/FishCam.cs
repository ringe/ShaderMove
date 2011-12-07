using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;

namespace PondLibs
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class FishCam : Microsoft.Xna.Framework.GameComponent
    {
        private GraphicsDeviceManager graphics;
        private Matrix projection;
        private Matrix view;
        private Vector3 cameraPosition = new Vector3(76, 24, 76);//Position of the fishCamera
        private Vector3 cameraTarget = Vector3.Zero;//Target of the fishCamera
        private Vector3 cameraUpVector = Vector3.Up;//Up vector on the fishCamera
        private Vector3 cameraReference = new Vector3(0, -.2f, -1.0f);
        private IInputHandler input; //Reference to the input component
        private Matrix rotationMatrix;// Rotation matrix for the fishCamera

        //Makes the view-, projection-, rotation- and position matrix available trough properties
        public Matrix View
        {
            get { return view; }
            set { view = value; }
        }

        public Matrix Rotation
        {
            get { return rotationMatrix; }
            set { rotationMatrix = value; }
        }

        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; }
        }

        public Vector3 Position
        {
            get { return cameraPosition; }
            set
            {
                cameraPosition.X = value.X;
                cameraPosition.Y = value.Y;
                cameraPosition.Z = value.Z;
            }
        }

        public FishCam(Game game, Vector3 pos)
            : base(game)
        {
            graphics = (GraphicsDeviceManager)game.Services.GetService(typeof(IGraphicsDeviceManager));
            cameraPosition = pos;

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

        public override void Update(GameTime gameTime)
        {
            //timeDelta = time between two calls on Update
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Creates a vector to point in the direction of the camera view 
            Vector3 transformedReference;

            // Starting position behind the Player
            Vector3 campos = new Vector3(0, 4, 10);

            // Turn to look forward
            rotationMatrix *= Matrix.CreateRotationY((float)(Math.PI * 1 / 4));

            // Adjust to camera reference
            Vector3.Transform(ref cameraReference, ref rotationMatrix, out transformedReference);

            campos = Vector3.Transform(campos, rotationMatrix);
            campos += cameraPosition;

            Vector3.Add(ref campos, ref transformedReference, out cameraTarget);

            Matrix.CreateLookAt(ref campos, ref cameraTarget, ref cameraUpVector, out view);

            base.Update(gameTime);
        }

        
    }
}
