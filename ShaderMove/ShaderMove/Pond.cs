using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Runtime.InteropServices;
using PondLibs;

namespace FishPond
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Pond : Microsoft.Xna.Framework.Game
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
        private FishCam camera; 

        // Vertices
        VertexPositionColorTexture[] cubeVertices;
        VertexPositionColorTexture[] cubeVertices2;
        VertexPositionColorTexture[] surfaceVertices;

        // Textures
        Model skyDome;
        Texture2D cloudTexture;
        Texture2D surfaceTexture;

        // Shaderstuff
        private Effect effect;
        private EffectParameter effectWorld;
        private EffectParameter effectView;
        private EffectParameter effectProjection;
        private EffectParameter effectPos;

        // WVP-matrisene:
        private Matrix world;

        // Boundaries
        private const float BOUNDARY = 80.0f;
        private const float EDGE = BOUNDARY * 2.0f;

        float elapsedTime;

        // FPS calculation
        TimeSpan fpsTime = TimeSpan.Zero;
        int frameRate = 0;
        int frameCounter = 0;

        // Terrain vars
        private int terrainWidth;
        private int terrainHeight;
        private float terrainPeak;
        private float[,] heightData;
        private int[] terrainIndices;
        private VertexPositionColorNormal[] terrainVertices;
        private Effect effect2;

        private EffectParameter effectAlpha;
        private Effect surfaceEffect;
        private float mfRed;
        private EffectParameter effectWaterWorld;
        private EffectParameter effectWaterProjection;
        private EffectParameter effectWaterView;

        // Fish
        private Matrix[] fishMatrix;
        private Player Player;
        private float animFactor;
        private bool animUp;
        private ArrayList opponents;

        // Water
        //private Water water;
        private List<ParticleExplosion> explosions = new List<ParticleExplosion>();
        private ParticleExplosionSettings particleExplosionSettings = new ParticleExplosionSettings();
        private ParticleSettings particleSettings = new ParticleSettings();
        private Texture2D dropTexture;
        private Effect waterEffect;
        private float waterLevel = 0.8f;

        // Start it all here
        private Vector3 startPosition = new Vector3(76, 60, 70);
        private int opponentCount = 20;

        //Randomness
        public Random rnd = new Random();
        private SubMarine sub;
        private bool showHelp;
        private Texture2D wasdTexture;
        private TimeSpan timeSinceLastHelpRequest;

        public Pond()
        {
            this.IsFixedTimeStep = false;
            graphics = new GraphicsDeviceManager(this);
            content = new ContentManager(this.Services);

            //Oppretter og tar i bruk input-handleren: 
            input = new Input(this);
            this.Components.Add(input);

            //Legger til Camera: 
            camera = new FishCam(this, startPosition);
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
            camera.Initialize();
            InitVertices();

            timeSinceLastHelpRequest = new TimeSpan();
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
        /// Prepare the object vertices
        /// </summary>
        protected void InitVertices()
        {
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

            // Surface vertices
            surfaceVertices = new VertexPositionColorTexture[4]
            {
                new VertexPositionColorTexture(new Vector3(-1, waterLevel, -1), Color.Blue, new Vector2(min,max)),
                new VertexPositionColorTexture(new Vector3( 1, waterLevel, -1), Color.Blue, new Vector2(min,min)),
                new VertexPositionColorTexture(new Vector3(-1, waterLevel,  1), Color.Blue, new Vector2(max,max)),
                new VertexPositionColorTexture(new Vector3(1, waterLevel,  1), Color.Blue, new Vector2(max,min))
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
                        terrainVertices[x + y * terrainWidth].Color = Color.DarkGray;
                    else if (heightData[x, y] < minHeight + (maxHeight - minHeight) * 2 / 4)
                        terrainVertices[x + y * terrainWidth].Color = Color.DarkBlue;
                    else if (heightData[x, y] < minHeight + (maxHeight - minHeight) * 3 / 4)
                        terrainVertices[x + y * terrainWidth].Color = Color.Blue;
                    else
                        terrainVertices[x + y * terrainWidth].Color = Color.SandyBrown;
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

            //Help
            wasdTexture = Content.Load<Texture2D>(@"Content\movement");

            // Water
            dropTexture = Content.Load<Texture2D>(@"Content\bubble");
            waterEffect = Content.Load<Effect>(@"Content\Particle2");
            waterEffect.CurrentTechnique =
            waterEffect.Techniques["Technique1"];
            waterEffect.Parameters["theTexture"].SetValue(dropTexture);
            //water = new Water(dropTexture);
            surfaceTexture = content.Load<Texture2D>(@"Content\pond-water-texture");

            // Position mouse at the center of the game window
            //Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);

            effect = Content.Load<Effect>(@"Content/effects");
            effect2 = Content.Load<Effect>(@"Content/MinEffekt2");
            surfaceEffect = Content.Load<Effect>(@"Content/waterEffectt");
            effectWorld = effect2.Parameters["World"];
            effectProjection = effect2.Parameters["Projection"];
            effectView = effect2.Parameters["View"];
            effectAlpha = surfaceEffect.Parameters["fx_Alpha"];
            effectWaterWorld = surfaceEffect.Parameters["World"];
            effectWaterProjection = surfaceEffect.Parameters["Projection"];
            effectWaterView = surfaceEffect.Parameters["View"];
            effectPos = surfaceEffect.Parameters["fx_Pos"];

            // Load heightmap
            Texture2D heightMap = Content.Load<Texture2D>(@"Content/mama");
            LoadHeightData(heightMap);

            SetUpVertices();
            SetUpIndices();
            CalculateNormals();
            CopyToBuffers();

            // Load Player
            Player = new Player(content, startPosition, this, heightData, terrainPeak*waterLevel);
            fishMatrix = new Matrix[Player.bones.Count];

            // Load Opponents
            Texture2D subT = Content.Load<Texture2D>(@"Content\steel");
            sub = new SubMarine(subT, startPosition);
            opponents = new ArrayList();
            for (int i = 0; i < opponentCount; i++)
            {
                opponents.Add(new Fish(content, heightData, terrainPeak*waterLevel));
                System.Threading.Thread.Sleep(50);
            }

            skyDome = Content.Load<Model>(@"Content\dome");
            skyDome.Meshes[0].MeshParts[0].Effect = effect.Clone();
            cloudTexture = content.Load<Texture2D>(@"Content\cloudMap");
        }

        // Load heightdata from Texture2D heightmap
        private void LoadHeightData(Texture2D heightMap)
        {
            terrainWidth = heightMap.Width;
            terrainHeight = heightMap.Height;

            Color[] heightMapColors = new Color[terrainWidth * terrainHeight];
            heightMap.GetData(heightMapColors);

            heightData = new float[terrainWidth, terrainHeight];
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainHeight; y++)
                {
                    float point = heightMapColors[x + y * terrainWidth].R / 2.80f;
                    terrainPeak = (terrainPeak > point) ? terrainPeak : point;
                    heightData[x, y] = point;
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

            // Time elapsed since the last call to update.
            elapsedTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            fpsCalc(gameTime);
            SetPos(gameTime);
            SetAnim(gameTime);

            Player.Update(gameTime);
            camera.Rotation = Matrix.CreateFromQuaternion(Player.Rotation);
            camera.Position = Player.pos;
            camera.Update(gameTime);

            // TODO Update drops 
            UpdateBubbles(gameTime);

            if (input.KeyboardState.IsKeyDown(Keys.H))
                if (timeSinceLastHelpRequest.TotalMilliseconds > 100)
                {
                    timeSinceLastHelpRequest = new TimeSpan();
                    showHelp = !showHelp;
                }

            base.Update(gameTime);
        }

        protected void UpdateBubbles(GameTime gameTime)
        {
            // Loop through and update explosions 
            for (int i = 0; i < explosions.Count; ++i)
            {
                explosions[i].Update(gameTime);
                // If explosion is finished, remove it 
                if (explosions[i].IsDead)
                {
                    explosions.RemoveAt(i);
                    --i;
                }
            }
        }

        private void MakeBubbles(Vector3 pos)
        {
            explosions.Add(new ParticleExplosion(GraphicsDevice,
               pos,
               (rnd.Next(
                   particleExplosionSettings.minLife,
                   particleExplosionSettings.maxLife)),
               (rnd.Next(
                   particleExplosionSettings.minRoundTime,
                   particleExplosionSettings.maxRoundTime)),
               (rnd.Next(
                   particleExplosionSettings.minParticlesPerRound,
                   particleExplosionSettings.maxParticlesPerRound)),
               (rnd.Next(
                   particleExplosionSettings.minParticles,
                   particleExplosionSettings.maxParticles)),
               new Vector2(dropTexture.Width,
                   dropTexture.Height),
               particleSettings));
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

        private void DrawHelpScreen()
        {
            Rectangle retval = new Rectangle(
                GraphicsDevice.Viewport.X + GraphicsDevice.Viewport.Width / 3,
                GraphicsDevice.Viewport.Y + GraphicsDevice.Viewport.Height * 1/ 4,
                wasdTexture.Width,
                wasdTexture.Height);

            String title = "How to Play:";
            String helpheader = " (H) Show/hide this screen.";
            String helptext = "  Movement       Look up/down";

            Vector2 pos = spriteFont.MeasureString(title);
            pos.X = graphics.GraphicsDevice.Viewport.Height / 2 - (pos.X / 4);
            pos.Y = graphics.GraphicsDevice.Viewport.Height / 2 - 200;

            spriteBatch.Begin();
            // Write header.
            spriteBatch.DrawString(spriteFont, helpheader, pos, Color.Black);
            spriteBatch.DrawString(spriteFont, helpheader, new Vector2(pos.X - 1, pos.Y - 1), Color.White);

            // Draw keys, write explanation
            spriteBatch.Draw(wasdTexture, retval, Color.White);
            pos.X = retval.X;
            pos.Y = retval.Y + retval.Height + 10;
            spriteBatch.DrawString(spriteFont, helptext, pos, Color.Black);
            spriteBatch.DrawString(spriteFont, helptext, new Vector2(pos.X - 1, pos.Y - 1), Color.White);
            
            
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
            Matrix matCam = Matrix.CreateTranslation(camera.Position.X, 0.0f, camera.Position.Z);

            world = matIdentify * scale * matCam;

            GraphicsDevice.Textures[0] = cloudTexture;

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

        protected void DrawSurface()
        {
            GraphicsDevice.Textures[0] = surfaceTexture;

            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            BlendState bs = new BlendState();
            bs.ColorSourceBlend = Blend.DestinationColor;
            bs.ColorDestinationBlend = Blend.SourceColor;
            bs.AlphaBlendFunction = BlendFunction.Add;
            GraphicsDevice.BlendState = bs;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // Surface
            for (int i = 0; i <= 2; i++)
            {
                effectPos.SetValue(mfRed-((i-1)*2));

                effectWaterWorld.SetValue(world);
                effectWaterView.SetValue(camera.View);
                effectWaterProjection.SetValue(camera.Projection);
                effectAlpha.SetValue(0.5f);
                Indices = new int[6] { 0, 1, 2, 2, 1, 3 };


                Matrix matIdentify = Matrix.Identity;
                Matrix scale;

                Matrix.CreateScale(terrainWidth, terrainPeak, terrainWidth, out scale);
                Matrix matCam = Matrix.CreateTranslation(camera.Position.X, 0.0f, camera.Position.Z);

                world = matIdentify * scale * matCam;
                //Starter tegning - må bruke effect-objektet:
                foreach (EffectPass pass in surfaceEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    // Angir primitivtype, aktuelle vertekser, en offsetverdi og antall 
                    //  primitiver (her 1 siden verteksene beskriver en tredekant):
                    GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, surfaceVertices, 0, 4, Indices, 0, 2);


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

        private void DrawSub()
        {
            GraphicsDevice.Textures[0] = sub.Texture;

            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            BlendState bs = new BlendState();
            bs.ColorSourceBlend = Blend.DestinationColor;
            bs.ColorDestinationBlend = Blend.SourceColor;
            bs.AlphaBlendFunction = BlendFunction.Add;
            GraphicsDevice.BlendState = bs;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            Matrix trans;

            trans = Matrix.CreateTranslation(sub.Position);
        
            Matrix matIdentify = Matrix.Identity;
            Matrix scale;

            Matrix.CreateScale(5, 5, 15, out scale);

            world = matIdentify * scale * trans;

            //Starter tegning
            foreach (EffectPass pass in effect2.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, sub.Vertices, 0, 1329, VertexPositionColorTextureNormal.VertexDeclaration);
            }

            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        private void DrawFish(Fish fish)
        {
            Matrix world = Matrix.Identity;
            effectWorld.SetValue(world);
            effectView.SetValue(camera.View);
            effectProjection.SetValue(camera.Projection);

            Matrix trans = Matrix.CreateTranslation(fish.pos);
        
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

            world *= fish.scale * Matrix.CreateFromQuaternion(fish.Rotation) * trans;
            fish.World = world;
            foreach (ModelMesh mesh in fish.meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = fishMatrix[mesh.ParentBone.Index] * world;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                    effect.EnableDefaultLighting();
                    effect.LightingEnabled = true;
                    
                }
                mesh.Draw();
            }

            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        }

        public void drawOpponents()
        {
            Object[] p = opponents.ToArray();
            for (int i = 0; i < p.Length; i++) {
                Fish fish = (Fish)p[i];
                if (Player.hits(fish, fish.World) || fish.hits(Player, Player.World))
                {
                    MakeBubbles(fish.pos);
                    opponents.Remove(fish);
                    if (opponents.Count == 0)
                        Player.Alive = false;
                }
                else
                    DrawFish(fish);
            }
        }

        private void DrawSkyDome()
        {
            Matrix[] modelTransforms = new Matrix[skyDome.Bones.Count];
            skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);
            

            Matrix wMatrix = Matrix.CreateTranslation(0, -0.3f, 0) * Matrix.CreateScale(200) * Matrix.CreateTranslation(camera.Position);
            foreach (ModelMesh mesh in skyDome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;
                    currentEffect.CurrentTechnique = currentEffect.Techniques["SkyDome"];
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(camera.View);
                    currentEffect.Parameters["xProjection"].SetValue(camera.Projection);
                    currentEffect.Parameters["xTexture"].SetValue(cloudTexture);
                    currentEffect.Parameters["xEnableLighting"].SetValue(true);
                }
                mesh.Draw();
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Keypress timer
            timeSinceLastHelpRequest += gameTime.ElapsedGameTime;

            // Count frames
            frameCounter++;

            RasterizerState rasterizerState1 = new RasterizerState();
            rasterizerState1.CullMode = CullMode.None;
            rasterizerState1.FillMode = FillMode.Solid;
            GraphicsDevice.RasterizerState = rasterizerState1;

            GraphicsDevice.Clear(Color.DeepSkyBlue);

            //Setter world=I:
            world = Matrix.Identity;

            // Player is alive and swimming
            if (Player.Alive)
            {
                //DrawCube(cubeVertices, cloudTexture);
                //DrawCube(cubeVertices2, texture2);
                //DrawSkyDome();

                // Draw Player and opponents
                DrawFish(Player);
                drawOpponents();

                DrawSub(); // TODO: buggy - sub under terrain and doesn't look like a sub at all
                DrawTerrain();

                // Draw water
                DrawSurface();
                for (int i = 0; i < explosions.Count; i++)
                    explosions[i].Draw(waterEffect, camera);
            }
            else
            {   // Player died
                showHelp = false;
                DrawFish(Player);
                String diemessage;
                if (opponents.Count == 0)
                    diemessage = "Congratulations, fatty!";
                else
                    diemessage = "To Eat or Get Eaten: You Died!";
                Vector2 pos = spriteFont.MeasureString(diemessage);
                pos.X = graphics.GraphicsDevice.Viewport.Height / 2 - (pos.X/4);
                pos.Y = graphics.GraphicsDevice.Viewport.Height / 2;
                DrawOverlayText(diemessage, (int)pos.X, (int)pos.Y);
            }

            if (showHelp)
                DrawHelpScreen();
            else {
                // Show FPS
                DrawOverlayText(string.Format("FPS: {0}", frameRate), 5, 2);

                // Print score
                DrawOverlayText(string.Format("Score: {0}", Player.score), 50, 20);
            }

            base.Draw(gameTime);
        }

        public int[] Indices { get; set; }
    }
}