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
        private Vector2 texcoord;
        private Vector3 normal;

        public MittVerteksFormat(Vector3 position, Color color, Vector2 texcoord, Vector3 normal)
        {
            this.position = position;
            this.color = color;
            this.texcoord = texcoord;
            this.normal = normal;
        }

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
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
        SpriteFont spriteFont;

        // Vertices
        VertexPositionColorTexture[] cubeVertices;
        VertexPositionColorTexture[] cubeVertices2;
        VertexPositionColor[] xAxis = new VertexPositionColor[2];
        VertexPositionColor[] yAxis = new VertexPositionColor[2];
        VertexPositionColor[] zAxis = new VertexPositionColor[2];

        // Textures
        Texture2D texture1;
        Texture2D texture2;

        // Shaderstuff
        private Effect effect;
        private EffectParameter effectRed;
        private EffectParameter effectPos;
        private EffectParameter effectWorld;
        private EffectParameter effectView;
        private EffectParameter effectProjection;
        private float mfRed = 0f;
        private bool mbRedIncrease = true;

        // WVP-matrisene:
        private Matrix world;
        private Matrix projection;
        private Matrix view;

        // Kameraposisjon:
        private Vector3 cameraPosition = new Vector3(-5f, 2f, 4f);
        private Vector3 cameraTarget = Vector3.Zero;
        private Vector3 cameraUpVector = new Vector3(0.0f, 1.0f, 0.0f);

        // Boundaries
        private const float BOUNDARY = 80.0f;
        private const float EDGE = BOUNDARY * 2.0f;

        // Rotationfactor
        float orbRotY;

        // Speed in world units per ms.
        private float speed = 0.02f;
        float elapsedTime;

        // FPS calculation
        TimeSpan fpsTime = TimeSpan.Zero;
        int frameRate = 0;
        int frameCounter = 0;

        #region initialize
        public Game1()
        {
            this.IsFixedTimeStep = false;
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

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp; 


            Window.Title = "Move!";
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
        #endregion

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
            float min = 0.001f; float max = 0.999f;
            cubeVertices = new VertexPositionColorTexture[8]
            {
                new VertexPositionColorTexture(new Vector3(-1,  1,  1),
                    Color.Blue, new Vector2(min,max)),
                new VertexPositionColorTexture(new Vector3( 1,  1,  1),
                    Color.Blue, new Vector2(min,min)),
                new VertexPositionColorTexture(new Vector3(-1, -1,  1),
                    Color.Red, new Vector2(max,max)),
                new VertexPositionColorTexture(new Vector3(1, -1,  1),
                    Color.Red, new Vector2(max,min)),
                new VertexPositionColorTexture(new Vector3(-1, -1, -1),
                    Color.Green, new Vector2(min,max)),
                new VertexPositionColorTexture(new Vector3(1, -1, -1),
                    Color.Green, new Vector2(min,min)),
                new VertexPositionColorTexture(new Vector3(-1,  1, -1),
                    Color.Yellow, new Vector2(max,max)),
                new VertexPositionColorTexture(new Vector3(1,  1, -1),
                    Color.Yellow, new Vector2(max,min))
            };
            cubeVertices2 = new VertexPositionColorTexture[8]
            {
                new VertexPositionColorTexture(new Vector3(1, -1, -1),
                    Color.Green, new Vector2(min,max)),
                new VertexPositionColorTexture(new Vector3(1, -1,  1),
                    Color.Red, new Vector2(min,min)),
                new VertexPositionColorTexture(new Vector3(1,  1, -1),
                    Color.Yellow, new Vector2(max,max)),
                new VertexPositionColorTexture(new Vector3( 1,  1,  1),
                    Color.Blue, new Vector2(max,min)),
                new VertexPositionColorTexture(new Vector3(-1,  1, -1),
                    Color.Yellow, new Vector2(min,max)),
                new VertexPositionColorTexture(new Vector3(-1,  1,  1),
                    Color.Blue, new Vector2(min,min)),
                new VertexPositionColorTexture(new Vector3(-1, -1, -1),
                    Color.Green, new Vector2(max,max)),
                new VertexPositionColorTexture(new Vector3(-1, -1,  1),
                    Color.Red, new Vector2(max,min))
            };

        }

        #region content
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>(@"Content\Arial");

            //Initialize Effect
            try
            {
                effect = Content.Load<Effect>(@"Content/MinEffekt2");
                effectWorld = effect.Parameters["World"];
                effectView = effect.Parameters["View"];
                effectProjection = effect.Parameters["Projection"];

                effectRed = effect.Parameters["fx_Red"];
                //effectPos = effect.Parameters["fx_Pos"];

                // Load textures
                texture1 = Content.Load<Texture2D>(@"Content/wla240077");
                texture2 = Content.Load<Texture2D>(@"Content/mta240029");
                
            }
            catch (ContentLoadException cle)
            {
                MessageBox(new IntPtr(0), cle.Message, "Utilgivelig feil...", 0);
                this.Exit();
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        #endregion

        #region update
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

            // Time elapsed since the last call to update.
            elapsedTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            fpsCalc(gameTime);

            base.Update(gameTime);
        }

        // FPS calculation
        void fpsCalc(GameTime gameTime)
        {
            fpsTime += gameTime.ElapsedGameTime;

            if (fpsTime > TimeSpan.FromSeconds(1))
            {
                fpsTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
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

            //effectPos.SetValue(mfRed);
            effectRed.SetValue(mfRed);
        }
        #endregion

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

        private void DrawOverlayText(string text, int x, int y)
        {
            spriteBatch.Begin();
            //Skriver teksten to ganger, først med svart bakgrunn og deretter med  hvitt, en piksel ned og til venstre, slik at teksten blir mer lesbar.
            spriteBatch.DrawString(spriteFont, text, new Vector2(x, y), Color.Black);
            spriteBatch.DrawString(spriteFont, text, new Vector2(x - 1, y - 1), Color.White);
            spriteBatch.End();
            //Må sette diverse parametre tilbake siden SpriteBatch justerer flere parametre (se Shawn Hargreaves Blog):
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone; //Avhenger av hva du ønsker
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        } 

        private void DrawCube(VertexPositionColorTexture[] cube, Texture2D texture)
        {
            Matrix matIdentify = Matrix.Identity;
            Matrix scale, cubeTrans, orbRotatY;

            Matrix.CreateScale(0.5f, 0.5f, 0.5f, out scale);

            cubeTrans = Matrix.CreateTranslation(2f, 0f, -2f);

            // Make the moon orbit the earth
            orbRotatY = Matrix.CreateRotationY(orbRotY);
            orbRotY += (elapsedTime * speed) / 50f;
            orbRotY = orbRotY % (float)(2 * Math.PI);

            world = matIdentify * orbRotatY;
            //world = matIdentify * scale * cubeTrans * orbRotatY;

            GraphicsDevice.Textures[0] = texture;
            //GraphicsDevice.Textures[1] = texture1;

            //effect.World = world;
            //effectWVP.SetValue(world * view * projection);
            effectWorld.SetValue(world);
            effectView.SetValue(view);
            effectProjection.SetValue(projection);

            //Starter tegning
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                //GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, cubeVertices, 0, 6, MittVerteksFormat.VertexDeclaration);
                //GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, cubeVertices2, 0, 6, MittVerteksFormat.VertexDeclaration);
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, cube, 0, 6, VertexPositionColorTexture.VertexDeclaration);
                //GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, cubeVertices2, 0, 6, VertexPositionColorTexture.VertexDeclaration);
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

            effectWorld.SetValue(world);
            effectView.SetValue(view);
            effectProjection.SetValue(projection);

            //DrawAxis();
            DrawCube(cubeVertices, texture1);
            DrawCube(cubeVertices2, texture2);

            // Count frames and show FPS
            frameCounter++;
            DrawOverlayText(string.Format("FPS: {0}", frameRate), 5, 2);

            base.Draw(gameTime);
        }
    }
}
