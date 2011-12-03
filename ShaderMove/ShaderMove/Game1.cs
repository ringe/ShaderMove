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
using ShaderLib;

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

        public struct VertexPositionColorNormal
        {
            public Vector3 Position;
            public Color Color;
            public Vector3 Normal;

            public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
            );
        }

        GraphicsDeviceManager graphics;
        private ContentManager content;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        VertexBuffer myVertexBuffer;
        IndexBuffer myIndexBuffer;

        // ShaderLib input and camera
        private Input input;
        //private Camera camera;
        private FirstPersonCamera camera; 

        // Vertices
        VertexPositionColorTexture[] cubeVertices;
        VertexPositionColorTexture[] cubeVertices2;
        VertexPositionColorTexture[] waterVertices;
        VertexPositionColor[] xAxis = new VertexPositionColor[2];
        VertexPositionColor[] yAxis = new VertexPositionColor[2];
        VertexPositionColor[] zAxis = new VertexPositionColor[2];

        // Textures
        Texture2D texture1;
        Texture2D texture2;
        Texture2D texture3;

        // Shaderstuff
        private Effect effect;
        private EffectParameter effectWorld;
        private EffectParameter effectView;
        private EffectParameter effectProjection;
        private EffectParameter effectPos;

        // WVP-matrisene:
        private Matrix world;
        private Matrix projection;
        private Matrix view;

        // Kameraposisjon:

        private Vector3 cameraPosition;// = new Vector3(60, 80, -80);
        private Vector3 cameraTarget;// = new Vector3(0, 0, 0);
        private Vector3 cameraUpVector;// = new Vector3(0, 1, 0);

        // Boundaries
        private const float BOUNDARY = 80.0f;
        private const float EDGE = BOUNDARY * 2.0f;

        float elapsedTime;

        // FPS calculation
        TimeSpan fpsTime = TimeSpan.Zero;
        int frameRate = 0;
        int frameCounter = 0;
        private int terrainWidth = 4;
        private int terrainHeight = 3;
        private float[,] heightData;
        private int[] terrainIndices;
        private VertexPositionColorNormal[] terrainVertices;
        private Effect effect2;

        private EffectParameter effectAlpha;
        private Effect waterEffect;
        private float mfRed;
        private EffectParameter effectWaterWorld;
        private EffectParameter effectWaterProjection;
        private EffectParameter effectWaterView;

        // Fish
        private Matrix[] fishMatrix;
        private Fish Player;
        private float animFactor;
        private bool animUp;

        #region initialize
        public Game1()
        {
            this.IsFixedTimeStep = false;
            graphics = new GraphicsDeviceManager(this);
            content = new ContentManager(this.Services);

            //Oppretter og tar i bruk input-handleren: 
            input = new Input(this);
            this.Components.Add(input);

            //Legger til Camera: 
            camera = new FirstPersonCamera(this);
            this.Components.Add(camera);
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

            //To trekanter

            waterVertices = new VertexPositionColorTexture[4]
            {
            new VertexPositionColorTexture(new Vector3(-1,  1.5f,  -1),
                    Color.Blue, new Vector2(min,max)),
                new VertexPositionColorTexture(new Vector3( 1,  1.5f,  -1),
                    Color.Blue, new Vector2(min,min)),
                new VertexPositionColorTexture(new Vector3(-1, 1.5f,  1),
                    Color.Red, new Vector2(max,max)),
                new VertexPositionColorTexture(new Vector3(1, 1.5f,  1),
                    Color.Red, new Vector2(max,min))
            };
           
        }

        private void SetUpVertices()
        {

            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainHeight; y++)
                {
                    if (heightData[x, y] < minHeight)
                        minHeight = heightData[x, y];
                    if (heightData[x, y] > maxHeight)
                        maxHeight = heightData[x, y];
                }
            }

            terrainVertices = new VertexPositionColorNormal[terrainWidth * terrainHeight];
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainHeight; y++)
                {
                    terrainVertices[x + y * terrainWidth].Position = new Vector3(x, heightData[x, y], -y);

                    if (heightData[x, y] < minHeight + (maxHeight - minHeight) / 4)
                        terrainVertices[x + y * terrainWidth].Color = Color.Blue;
                    else if (heightData[x, y] < minHeight + (maxHeight - minHeight) * 2 / 4)
                        terrainVertices[x + y * terrainWidth].Color = Color.Green;
                    else if (heightData[x, y] < minHeight + (maxHeight - minHeight) * 3 / 4)
                        terrainVertices[x + y * terrainWidth].Color = Color.Brown;
                    else
                        terrainVertices[x + y * terrainWidth].Color = Color.White;
                }
            }
        }

        private void SetUpIndices()
        {
            terrainIndices = new int[(terrainWidth - 1) * (terrainHeight - 1) * 6];
            int counter = 0;
            for (int y = 0; y < terrainHeight - 1; y++)
            {
                for (int x = 0; x < terrainWidth - 1; x++)
                {
                    int lowerLeft = x + y * terrainWidth;
                    int lowerRight = (x + 1) + y * terrainWidth;
                    int topLeft = x + (y + 1) * terrainWidth;
                    int topRight = (x + 1) + (y + 1) * terrainWidth;

                    terrainIndices[counter++] = topLeft;
                    terrainIndices[counter++] = lowerRight;
                    terrainIndices[counter++] = lowerLeft;

                    terrainIndices[counter++] = topLeft;
                    terrainIndices[counter++] = topRight;
                    terrainIndices[counter++] = lowerRight;
                }
            }
        }

        private void CopyToBuffers()
        {
            myVertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionColorNormal.VertexDeclaration, terrainVertices.Length, BufferUsage.WriteOnly);
            myVertexBuffer.SetData(terrainVertices);

            myIndexBuffer = new IndexBuffer(GraphicsDevice, typeof(int), terrainIndices.Length, BufferUsage.WriteOnly);
            myIndexBuffer.SetData(terrainIndices);
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

            // Position mouse at the center of the game window
            Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);

            effect = Content.Load<Effect>(@"Content/effects");
            effect2 = Content.Load<Effect>(@"Content/MinEffekt2");
            waterEffect = Content.Load<Effect>(@"Content/waterEffectt");
            effectWorld = effect2.Parameters["World"];
            effectProjection = effect2.Parameters["Projection"];
            effectView = effect2.Parameters["View"];
            effectAlpha = waterEffect.Parameters["fx_Alpha"];
            effectWaterWorld = waterEffect.Parameters["World"];
            effectWaterProjection = waterEffect.Parameters["Projection"];
            effectWaterView = waterEffect.Parameters["View"];
            effectPos = waterEffect.Parameters["fx_Pos"];

            // Load heightmap
            Texture2D heightMap = Content.Load<Texture2D>(@"Content/mama");
            LoadHeightData(heightMap);

            SetUpVertices();
            SetUpIndices();
            CalculateNormals();
            CopyToBuffers();

            Player = new Fish(content, 5, new Vector3(76, 20 , 70 ));
            fishMatrix = new Matrix[Player.bones.Count];

            texture1 = content.Load<Texture2D>(@"Content\cloudMap");
            texture3 = content.Load<Texture2D>(@"Content\water_normal");
        }

        private void LoadHeightData(Texture2D heightMap)
        {
            terrainWidth = heightMap.Width;
            terrainHeight = heightMap.Height;

            Color[] heightMapColors = new Color[terrainWidth * terrainHeight];
            heightMap.GetData(heightMapColors);

            heightData = new float[terrainWidth, terrainHeight];
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainHeight; y++)
                    heightData[x, y] = heightMapColors[x + y * terrainWidth].R / 5.0f;

            //camera.setHeight(ref heightData);
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

            // Time elapsed since the last call to update.
            elapsedTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            fpsCalc(gameTime);
            SetPos(gameTime);
            SetAnim(gameTime);

            base.Update(gameTime);
        }

        void SetPos(GameTime gameTime)
        {
            if (mfRed > 2)
                mfRed = 0;

            mfRed += (float)gameTime.ElapsedGameTime.Milliseconds / 10000.0f;

       }
        //Set direction on animation
        void SetAnim(GameTime gameTime)
        {
            //right boundary
            float max = (float)Math.PI/20;
            //left boundary
            float min = -max;
            float factor = (float)gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            //if less than min, go towards max
            if (animFactor < min)
                animUp = true;
            //if more than mmax, go towards min
            if (animFactor > max)
                animUp = false;

            if (animUp)
                animFactor += factor;
            else
                animFactor -= factor;
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

        private void CalculateNormals()
        {
            for (int i = 0; i < terrainVertices.Length; i++)
                terrainVertices[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < terrainIndices.Length / 3; i++)
            {
                int index1 = terrainIndices[i * 3];
                int index2 = terrainIndices[i * 3 + 1];
                int index3 = terrainIndices[i * 3 + 2];

                Vector3 side1 = terrainVertices[index1].Position - terrainVertices[index3].Position;
                Vector3 side2 = terrainVertices[index1].Position - terrainVertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                terrainVertices[index1].Normal += normal;
                terrainVertices[index2].Normal += normal;
                terrainVertices[index3].Normal += normal;
            }

            for (int i = 0; i < terrainVertices.Length; i++)
                terrainVertices[i].Normal.Normalize();
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
            Matrix scale;

            Matrix.CreateScale(terrainWidth, terrainHeight, terrainWidth, out scale);
            Matrix matCam = Matrix.CreateTranslation(camera.CameraPosition.X, 0.0f, camera.CameraPosition.Z);

            world = matIdentify * scale * matCam;

            GraphicsDevice.Textures[0] = texture1;

            effectWorld.SetValue(world);
            effectView.SetValue(camera.View);
            effectProjection.SetValue(camera.Projection);
            effectAlpha.SetValue(1f);
            //Starter tegning
            foreach (EffectPass pass in effect2.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, cube, 0, 6, VertexPositionColorTexture.VertexDeclaration);
            }

        }

        protected void DrawWater(VertexPositionColorTexture[] water, Texture2D texture)
        {
            for (int i = 0; i <= 2; i++)
            {
                effectPos.SetValue(mfRed-((i-1)*2));

                GraphicsDevice.Textures[0] = texture3;

                GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

                BlendState bs = new BlendState();
                bs.ColorSourceBlend = Blend.DestinationColor;
                bs.ColorDestinationBlend = Blend.SourceColor;
                bs.AlphaBlendFunction = BlendFunction.Add;
                GraphicsDevice.BlendState = bs;
                GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

                effectWaterWorld.SetValue(world);
                effectWaterView.SetValue(camera.View);
                effectWaterProjection.SetValue(camera.Projection);
                effectAlpha.SetValue(0.5f);
                Indices = new int[6] { 0, 1, 2, 2, 1, 3 };


                Matrix matIdentify = Matrix.Identity;
                Matrix scale;

                Matrix.CreateScale(terrainWidth, terrainHeight / 10, terrainWidth, out scale);
                Matrix matCam = Matrix.CreateTranslation(camera.CameraPosition.X, 0.0f, camera.CameraPosition.Z);

                world = matIdentify * scale * matCam;
                //Starter tegning - må bruke effect-objektet:
                foreach (EffectPass pass in waterEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    // Angir primitivtype, aktuelle vertekser, en offsetverdi og antall 
                    //  primitiver (her 1 siden verteksene beskriver en tredekant):
                    GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, water, 0, 4, Indices, 0, 2);


                }
            }
            
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        public void DrawTerrain()
        {
            Matrix matIdentify = Matrix.Identity;
            // Sentrer terreng i origo
            Matrix translate = Matrix.CreateTranslation(-terrainWidth / 2.0f, 0, terrainHeight / 2.0f);


            world = matIdentify * translate;

            effect.CurrentTechnique = effect.Techniques["Colored"];
            effect.Parameters["xView"].SetValue(camera.View);
            effect.Parameters["xProjection"].SetValue(camera.Projection);
            effect.Parameters["xWorld"].SetValue(world);
            Vector3 lightDirection = new Vector3(1.0f, -1.0f, -1.0f);
            lightDirection.Normalize();
            effect.Parameters["xLightDirection"].SetValue(lightDirection);
            effect.Parameters["xAmbient"].SetValue(0.1f);
            effect.Parameters["xEnableLighting"].SetValue(true);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.Indices = myIndexBuffer;
                GraphicsDevice.SetVertexBuffer(myVertexBuffer);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, terrainVertices.Length, 0, terrainIndices.Length / 3);
            }
        }

        private void DrawFish(Fish fish)
        {
            Matrix world = Matrix.Identity;
            effectWorld.SetValue(world);
            effectView.SetValue(camera.View);
            effectProjection.SetValue(camera.Projection);

            Matrix trans, rotation;

            float m = (float)(Math.PI*4/4);
            Matrix.CreateRotationY(m, out rotation);

            trans = Matrix.CreateTranslation(fish.pos);
            
            //under back fin
            fish.bones[21].Transform = Matrix.CreateRotationY(animFactor/5);
            //back fin
            fish.bones[24].Transform = Matrix.CreateRotationY(-animFactor);
            //side fins
            fish.bones[12].Transform = Matrix.CreateRotationY(animFactor/5);
            fish.bones[9].Transform = Matrix.CreateRotationY(-animFactor/5);
            //under fins
            fish.bones[15].Transform = Matrix.CreateRotationY(-animFactor);
            fish.bones[18].Transform = Matrix.CreateRotationY(animFactor);
            //whole fish
            fish.bones[1].Transform = Matrix.CreateRotationY(animFactor/10);

            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            fish.CopyAbsoluteBoneTransformsTo(ref fishMatrix);

            foreach (ModelMesh mesh in fish.meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = fishMatrix[mesh.ParentBone.Index] * fish.scale * rotation * trans;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                    effect.EnableDefaultLighting();
                    effect.LightingEnabled = true;
                    
                }
                mesh.Draw();
            }

            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
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
            
            
            DrawCube(cubeVertices, texture1);
            DrawCube(cubeVertices2, texture2);

            DrawFish(Player);

            // Count frames and show FPS
            frameCounter++;
            DrawOverlayText(string.Format("FPS: {0}", frameRate), 5, 2);

            DrawTerrain();
            DrawWater(waterVertices, texture2);



            base.Draw(gameTime);
        }

        public int[] Indices { get; set; }
    }
}
