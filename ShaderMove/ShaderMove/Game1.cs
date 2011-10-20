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
using System.Runtime.InteropServices;

namespace ShaderMove
{
    struct MittVerteksFormat
    {
        private Vector3 position;
        private Color color;

        public MittVerteksFormat(Vector3 position, Color color)
        {
            this.position = position;
            this.color = color;
        }

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint MessageBox(IntPtr hWnd, String text, String caption, uint type);

        GraphicsDeviceManager graphics;
        private ContentManager content;
        SpriteBatch spriteBatch;

        // Vertices
        VertexPositionColor[] cubeVertices;
        VertexPositionColor[] xAxis = new VertexPositionColor[2];
        VertexPositionColor[] yAxis = new VertexPositionColor[2];
        VertexPositionColor[] zAxis = new VertexPositionColor[2];

        // Shaderstuff
        private Effect effect;
        private EffectParameter effectRed;
        private EffectParameter effectPos;
        private EffectParameter effectWorld;
        private EffectParameter effectView;
        private EffectParameter effectProjection;
        private float mfRed = 0f;
        private bool mbRedIncrease = true;

        //WVP-matrisene:
        private Matrix world;
        private Matrix projection;
        private Matrix view;

        //Kameraposisjon:
        private Vector3 cameraPosition = new Vector3(4f, 3f, 7f);
        private Vector3 cameraTarget = Vector3.Zero;
        private Vector3 cameraUpVector = new Vector3(0.0f, 1.0f, 0.0f);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            content = new ContentManager(this.Services);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            InitDevice();
            InitCamera();
            InitVertices();
        }


        /// <summary>
        /// Initialize the graphics device.
        /// </summary>
        private void InitDevice()
        {

            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 800;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            Window.Title = "Move!";

            //effect = new BasicEffect(GraphicsDevice);

            //effect.VertexColorEnabled = true;

            //Initialize Effect
            try
            {
                effect = Content.Load<Effect>(@"Content/MinEffekt2");
                effectWorld = effect.Parameters["World"];
                effectView = effect.Parameters["View"];
                effectProjection = effect.Parameters["Projection"];

                effectRed = effect.Parameters["fx_Red"];
                effectPos = effect.Parameters["fx_Pos"];
                //effectWVP = effect.Parameters["fx_WVP"];
            }
            catch (ContentLoadException cle)
            {
                MessageBox(new IntPtr(0), cle.Message, "Utilgivelig feil...", 0);
                this.Exit();
            }
        }


        /// <summary>
        /// Position the camera.
        /// </summary>
        private void InitCamera()
        {
            //Projeksjon:
            float aspectRatio = (float)graphics.GraphicsDevice.Viewport.Width / (float)graphics.GraphicsDevice.Viewport.Height;

            //Oppretter view-matrisa:
            Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out view);

            //Oppretter projeksjonsmatrisa:
            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 0.01f, 1000.0f, out projection);

            //Gir matrisene til shader:
            //effect.Projection = projection;
            //effect.View = view;

        }

        /// <summary>
        /// Prepare the object vertices
        /// </summary>
        protected void InitVertices()
        {
            // Set axis lines
            xAxis[0] = new VertexPositionColor(new Vector3(-100.0f, 0f, 0f), Color.Red);
            xAxis[1] = new VertexPositionColor(new Vector3(100.0f, 0f, 0f), Color.Red);
            yAxis[0] = new VertexPositionColor(new Vector3(0f, -100.0f, 0f), Color.White);
            yAxis[1] = new VertexPositionColor(new Vector3(0f, 100.0f, 0f), Color.White);
            zAxis[0] = new VertexPositionColor(new Vector3(0f, 0f, -100.0f), Color.Black);
            zAxis[1] = new VertexPositionColor(new Vector3(0f, 0f, 100.0f), Color.Black);

            // Initialize a Cube
            cubeVertices = new VertexPositionColor[17]
            {
                new VertexPositionColor(new Vector3(-1,  1,  1), Color.Red),
                new VertexPositionColor(new Vector3( 1,  1,  1), Color.Red),
                new VertexPositionColor(new Vector3(-1, -1,  1), Color.Red),
                new VertexPositionColor(new Vector3(1, -1,  1), Color.Red),
                new VertexPositionColor(new Vector3(-1, -1, -1), Color.Green),
                new VertexPositionColor(new Vector3(1, -1, -1), Color.Green),
                new VertexPositionColor(new Vector3(-1,  1, -1), Color.Yellow),
                new VertexPositionColor(new Vector3(1,  1, -1), Color.Yellow),
                new VertexPositionColor(new Vector3(-1,  1,  1), Color.Green),
                new VertexPositionColor(new Vector3(1,  1,  1), Color.Green),
                new VertexPositionColor(new Vector3(1, -1,  1), Color.Blue),
                new VertexPositionColor(new Vector3(1,  1, -1), Color.Blue),
                new VertexPositionColor(new Vector3(1, -1, -1), Color.Orange),
                new VertexPositionColor(new Vector3(-1, -1, -1), Color.Orange),
                new VertexPositionColor(new Vector3(-1,  1, -1), Color.Pink),
                new VertexPositionColor(new Vector3(-1, -1,  1), Color.Pink),
                new VertexPositionColor(new Vector3(-1,  1,  1), Color.Yellow)
            };
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            HandleKeyboardInput();
            SetPos(gameTime);

            base.Update(gameTime);
        }

        void SetPos(GameTime gameTime)
        {
            if (mbRedIncrease)
                mfRed += (float)gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            else
                mfRed -= (float)gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            if (mfRed <= 0.0f)
                mbRedIncrease = true;
            else if (mfRed >= 1.0f)
                mbRedIncrease = false;

            effectPos.SetValue(mfRed);
            effectRed.SetValue(mfRed);
        }

        #region input
        /// <summary>
        /// React to key press
        /// </summary>
        private void HandleKeyboardInput()
        {
            KeyboardState keys = Keyboard.GetState();

            // Determine change in direction
            //if (keys.IsKeyDown(Keys.Left))
            //    SetSpeed(false);
            //else if (keys.IsKeyDown(Keys.Right))
            //    SetSpeed(true);

            //// Determine change in speed
            //if (keys.IsKeyDown(Keys.Up))
            //    movement = movement + movement * 0.02f;
            //else if (keys.IsKeyDown(Keys.Down))
            //    movement = movement - movement * 0.02f;

            //// Determine change in direction up/down
            //if (keys.IsKeyDown(Keys.W))
            //    position.Y += 0.05f;
            //else if (keys.IsKeyDown(Keys.S))
            //    position.Y -= 0.05f;

            // Exit
            if (keys.IsKeyDown(Keys.Escape))
                this.Exit();
        }
        #endregion

        /// <summary>
        /// Draw the axis lines.
        /// </summary>
        private void DrawAxis()
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, xAxis, 0, 1);
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, yAxis, 0, 1);
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, zAxis, 0, 1);
            }
        }

        private void DrawCube()
        {
            Matrix scale, rotatY, move;

            Matrix.CreateScale(0.5f, 0.5f, 0.5f, out scale);

            // Set rotation according to the movement angle
            //rotatY = Matrix.CreateRotationY(speedAngle);

            // Set translation to current position
            //move = Matrix.CreateTranslation(position);

            //plane.Push(scale * rotatY * move);

            //world = Matrix.Identity * plane.Peek();

            //effect.World = world;
            //effectWVP.SetValue(world * view * projection);
            effectWorld.SetValue(world);
            effectView.SetValue(view);
            effectProjection.SetValue(projection);

            //Starter tegning
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, cubeVertices, 0, 15, MittVerteksFormat.VertexDeclaration);
            }

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            RasterizerState rasterizerState1 = new RasterizerState();
            rasterizerState1.CullMode = CullMode.None;
            rasterizerState1.FillMode = FillMode.Solid;
            GraphicsDevice.RasterizerState = rasterizerState1;

            GraphicsDevice.Clear(Color.DeepSkyBlue);

            //Setter world=I:
            world = Matrix.Identity;

            //Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(world));
            //effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);

            // Setter world-matrisa p� effect-objektet (verteks-shaderen):
            //effect.World = world;

            //effectWVP.SetValue(world * view * projection);
            effectWorld.SetValue(world);
            effectView.SetValue(view);
            effectProjection.SetValue(projection);

            //DrawAxis();
            DrawCube();

            base.Draw(gameTime);
        }
    }
}
