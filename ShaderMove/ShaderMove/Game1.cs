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
        VertexPositionColor[] xAxis = new VertexPositionColor[2];
        VertexPositionColor[] yAxis = new VertexPositionColor[2];
        VertexPositionColor[] zAxis = new VertexPositionColor[2];

        // Textures
        Texture2D texture1;
        Texture2D texture2;

        // Shaderstuff
        private Effect effect;
        private EffectParameter effectWorld;
        private EffectParameter effectView;
        private EffectParameter effectProjection;

        // WVP-matrisene:
        private Matrix world;
        private Matrix projection;
        private Matrix view;

        // Kameraposisjon:
        //private Vector3 cameraPosition = new Vector3(-5f, 2f, 4f);
        //private Vector3 cameraTarget = Vector3.Zero;
        //private Vector3 cameraUpVector = new Vector3(0.0f, 1.0f, 0.0f);
        private Vector3 cameraPosition;// = new Vector3(60, 80, -80);
        private Vector3 cameraTarget;// = new Vector3(0, 0, 0);
        private Vector3 cameraUpVector;// = new Vector3(0, 1, 0);

        // Boundaries
        private const float BOUNDARY = 80.0f;
        private const float EDGE = BOUNDARY * 2.0f;

        // Rotationfactor
        float orbRotY;

        // Speed in world units per ms.
        private float speed = 0.02f;
        float elapsedTime;

        // Sky dome
        Texture2D cloudMap;
        Model skyDome;

        // FPS calculation
        TimeSpan fpsTime = TimeSpan.Zero;
        int frameRate = 0;
        int frameCounter = 0;
        private int terrainWidth = 4;
        private int terrainHeight = 3;
        private float[,] heightData;
        private int[] terrainIndices;
        private VertexPositionColorNormal[] terrainVertices;

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

            // Load heightmap
            Texture2D heightMap = Content.Load<Texture2D>(@"Content/mama");
            LoadHeightData(heightMap);

            SetUpVertices();
            SetUpIndices();
            CalculateNormals();
            CopyToBuffers();

            // Load skydome
            skyDome = Content.Load<Model>(@"Content/dome");
            cloudMap = Content.Load<Texture2D>(@"Content/cloudMap");
            skyDome.Meshes[0].MeshParts[0].Effect = effect.Clone();
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

            camera.setHeight(ref heightData);
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
            Matrix scale, cubeTrans, orbRotatY;

            Matrix.CreateScale(0.5f, 0.5f, 0.5f, out scale);

            cubeTrans = Matrix.CreateTranslation(2f, 0f, -2f);

            // Make the moon orbit the earth
            orbRotatY = Matrix.CreateRotationY(orbRotY);
            orbRotY += (elapsedTime * speed) / 50f;
            orbRotY = orbRotY % (float)(2 * Math.PI);

            world = matIdentify *orbRotatY;
            //world = matIdentify * scale * cubeTrans * orbRotatY;

            GraphicsDevice.Textures[0] = texture;
            //GraphicsDevice.Textures[1] = texture1;

            //effect.World = world;
            //effectWVP.SetValue(world * view * projection);
            effectWorld.SetValue(world);
            effectView.SetValue(camera.View);
            effectProjection.SetValue(camera.Projection);

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

        public void DrawTerrain()
        {
            Matrix matIdentify = Matrix.Identity;
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
                //GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, terrainVertices, 0, terrainVertices.Length, terrainIndices, 0, terrainIndices.Length / 3, VertexPositionColorNormal.VertexDeclaration);
                GraphicsDevice.Indices = myIndexBuffer;
                GraphicsDevice.SetVertexBuffer(myVertexBuffer);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, terrainVertices.Length, 0, terrainIndices.Length / 3);
            }
        }

        //private void DrawSkyDome(Matrix currentViewMatrix)
        //{
        //    //GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

        //    Matrix[] modelTransforms = new Matrix[skyDome.Bones.Count];
        //    skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);

        //    Matrix wMatrix = Matrix.CreateTranslation(0, -0.3f, 0) * Matrix.CreateScale(100) * Matrix.CreateTranslation(cameraPosition);
        //    foreach (ModelMesh mesh in skyDome.Meshes)
        //    {
        //        foreach (Effect currentEffect in mesh.Effects)
        //        {
        //            Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;
        //            currentEffect.CurrentTechnique = currentEffect.Techniques["SkyDome"];
        //            currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
        //            currentEffect.Parameters["xView"].SetValue(currentViewMatrix);
        //            currentEffect.Parameters["xProjection"].SetValue(camera.Projection);
        //            currentEffect.Parameters["xTexture0"].SetValue(cloudMap);
        //            currentEffect.Parameters["xEnableLighting"].SetValue(false);
        //        }
        //        mesh.Draw();
        //    }
        //    //GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
        //}

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

            DrawTerrain();
            //DrawSkyDome(camera.View);
            //DrawAxis();
            //DrawCube(cubeVertices, texture1);
            //DrawCube(cubeVertices2, texture2);

            // Count frames and show FPS
            frameCounter++;
            DrawOverlayText(string.Format("FPS: {0}", frameRate), 5, 2);

            base.Draw(gameTime);
        }
    }
}
