using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace ShaderLib
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class FishCam : Microsoft.Xna.Framework.GameComponent
    {
        private const float moveRate = 20.0f;      // for FirstPersonCamra. 
        protected Vector3 movement = Vector3.Zero; // for FirstPersonCamera.
        protected float[,] heightData; // for FirstPersonCamera

        private GraphicsDeviceManager graphics;
        private Matrix projection;
        private Matrix view;
        private Vector3 cameraPosition = new Vector3(76, 24, 76);
        private Vector3 cameraTarget = Vector3.Zero;
        private Vector3 cameraUpVector = Vector3.Up;
        private Vector3 cameraReference = new Vector3(0, -.3f, -1.0f);
        private float cameraYaw = 0.0f;
        private float cameraPitch = 0.0f;
        private const float spinRate = 40.0f;

        //view og projection-matrisene er tilgjengelig via properties: 
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

        public FishCam(Game game)
            : base(game)
        {
            graphics = (GraphicsDeviceManager)game.Services.GetService(typeof(IGraphicsDeviceManager));
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
            //timeDelta = tiden mellom to kall på Update 
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Posisjoner kamera: 
            Matrix rotationMatrix;

            //Rotasjonsmatrise om Y-aksen: 
            Matrix.CreateRotationY(MathHelper.ToRadians(cameraYaw), out rotationMatrix);

            //Legger til pitch dvs. rotasjon om X‐aksen:
            rotationMatrix = Matrix.CreateRotationX(MathHelper.ToRadians(cameraPitch)) * rotationMatrix;

            //FirstPersonCamera, endrer kameraets posisjon: 
            movement *= (moveRate * timeDelta);
            if (movement != Vector3.Zero)
            {
                //Roterer movement-vektoren: 
                Vector3.Transform(ref movement, ref rotationMatrix, out movement);
                //Oppdaterer kameraposisjonen med move-vektoren:  
                cameraPosition += movement;
            }

            // Setter posisjonen til bakkenivå
            if (heightData != null)
                cameraPosition.Y = heightData[(int)cameraPosition.X, (int)cameraPosition.Z];

            // Oppretter en vektor som peker i retninga kameraet 'ser': 
            Vector3 transformedReference;

            // Roterer cameraReference-vektoren: 
            Vector3.Transform(ref cameraReference, ref rotationMatrix, out transformedReference);

            // Beregner hva kameraet ser på (cameraTarget) vha.  
            // nåværende posisjonsvektor og retningsvektoren: 
            Vector3.Add(ref cameraPosition, ref transformedReference, out cameraTarget);

            //Oppdaterer view-matrisa vha. posisjons, kameramål og opp-vektorene: 
            Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out view);

            base.Update(gameTime);
        }
    }
}
