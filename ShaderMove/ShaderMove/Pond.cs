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
        VertexPositionColorTexture[] leftV;
        VertexPositionColorTexture[] rightV;
        VertexPositionColorTexture[] backV;
        VertexPositionColorTexture[] frontV;
        VertexPositionColorTexture[] surfaceVertices;

        // Textures
        Model skyDome;
        Texture2D cloudTexture;
        Texture2D surfaceTexture;

        // Shaderstuff
        private Effect terrainEffect;
        private EffectParameter effectWorld;
        private EffectParameter effectView;
        private EffectParameter effectProjection;
        private EffectParameter effectSurfacePosition;

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
        private int terrainSizeX;
        private int terrainSizeZ;
        private float terrainPeak;
        private float[,] heightData;
        private int[] terrainIndices;
        private VertexPositionColorNormal[] terrainVertices;
        private Effect horizonEffect;

        // Surface vars
        private EffectParameter effectSurfaceAlpha;
        private Effect surfaceEffect;
        private float surfaceMovement;
        private EffectParameter effectSurfaceWorld;
        private EffectParameter effectSurfaceProjection;
        private EffectParameter effectSurfaceView;

        // Fish
        private Player Player;
        private float animFactor;
        private bool animUp;
        private SubMarine subMarine;
        private int opponentCount = 20;
        private ArrayList opponents;

        // Water
        //private Water water;
        private List<ParticleExplosion> bubbles = new List<ParticleExplosion>();
        private ParticleExplosionSettings particleExplosionSettings = new ParticleExplosionSettings();
        private ParticleSettings particleSettings = new ParticleSettings();
        private Texture2D dropTexture;
        private Effect bubbleEffect;
        private float waterLevel = 0.8f;

        // Start it all here
        private Vector3 startPosition = new Vector3(76, 60, 70);

        //Randomness
        public Random rnd = new Random();

        // Helpstuff
        private Video video;
        private VideoPlayer player;
        private bool showHelp = true;
        private Texture2D wasdTexture;
        private TimeSpan timeSinceLastHelpRequest;

        /// <summary>
        /// Create an instance of this game! :)
        /// </summary>
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
            base.Initialize();
            InitDevice();
            camera.Initialize();
            InitHorizonVertices();
        }


        /// <summary>
        /// Initialize the graphics device.
        /// </summary>
        private void InitDevice()
        {
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp; 

            Window.Title = "FishPond";
        }

        // Prepare the Horizon / game edge
        protected void InitHorizonVertices()
        {
            float min = 0.001f; float max = 0.999f;

            // SeaSides
            frontV = new VertexPositionColorTexture[4] {
                
                new VertexPositionColorTexture(new Vector3(-1, -1, 1),
                    Color.Yellow, new Vector2(min,max)),
                new VertexPositionColorTexture(new Vector3(-1, 1, 1),
                    Color.Yellow, new Vector2(min,min)),
                new VertexPositionColorTexture(new Vector3(1, -1, 1),
                    Color.Yellow, new Vector2(max,max)),
                new VertexPositionColorTexture(new Vector3(1, 1, 1),
                    Color.Yellow, new Vector2(max,min))
            };

            leftV = new VertexPositionColorTexture[4] {
                new VertexPositionColorTexture(new Vector3(-1, -1, -1),
                    Color.Green, new Vector2(min,max)),
                new VertexPositionColorTexture(new Vector3(-1, 1, -1),
                    Color.Green, new Vector2(min,min)),
                new VertexPositionColorTexture(new Vector3(-1, -1,  1),
                    Color.Green, new Vector2(max,max)),
                new VertexPositionColorTexture(new Vector3(-1, 1,  1),
                    Color.Green, new Vector2(max,min))
            };

            backV = new VertexPositionColorTexture[4] {
                
                new VertexPositionColorTexture(new Vector3(1, -1, -1),
                    Color.Yellow, new Vector2(max,max)),
                new VertexPositionColorTexture(new Vector3(1, 1, -1),
                    Color.Yellow, new Vector2(max,min)),
                new VertexPositionColorTexture(new Vector3(-1, -1, -1),
                    Color.Yellow, new Vector2(min,max)),
                new VertexPositionColorTexture(new Vector3(-1, 1, -1),
                    Color.Yellow, new Vector2(min,min))
            };

            rightV = new VertexPositionColorTexture[4] {
                
                new VertexPositionColorTexture(new Vector3(1, -1, -1),
                    Color.Green, new Vector2(min,max)),
                new VertexPositionColorTexture(new Vector3(1, 1, -1),
                    Color.Green, new Vector2(min,min)),
                new VertexPositionColorTexture(new Vector3(1, -1,  1),
                    Color.Green, new Vector2(max,max)),
                new VertexPositionColorTexture(new Vector3(1, 1,  1),
                    Color.Green, new Vector2(max,min))
            };
        }

        private void SetTerrainVertices()
        {

            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            for (int x = 0; x < terrainSizeX; x++)
            {
                for (int y = 0; y < terrainSizeZ; y++)
                {
                    if (heightData[x, y] < minHeight)
                        minHeight = heightData[x, y];
                    if (heightData[x, y] > maxHeight)
                        maxHeight = heightData[x, y];
                }
            }

            terrainVertices = new VertexPositionColorNormal[terrainSizeX * terrainSizeZ];
            for (int x = 0; x < terrainSizeX; x++)
            {
                for (int y = 0; y < terrainSizeZ; y++)
                {
                    terrainVertices[x + y * terrainSizeX].Position = new Vector3(x, heightData[x, y], -y);

                    if (heightData[x, y] < minHeight + (maxHeight - minHeight) / 4)
                        terrainVertices[x + y * terrainSizeX].Color = Color.DarkGray;
                    else if (heightData[x, y] < minHeight + (maxHeight - minHeight) * 2 / 4)
                        terrainVertices[x + y * terrainSizeX].Color = Color.DarkBlue;
                    else if (heightData[x, y] < minHeight + (maxHeight - minHeight) * 3 / 4)
                        terrainVertices[x + y * terrainSizeX].Color = Color.Blue;
                    else
                        terrainVertices[x + y * terrainSizeX].Color = Color.SandyBrown;
                }
            }
        }

        private void SetTerrainIndices()
        {
            terrainIndices = new int[(terrainSizeX - 1) * (terrainSizeZ - 1) * 6];
            int counter = 0;
            for (int y = 0; y < terrainSizeZ - 1; y++)
            {
                for (int x = 0; x < terrainSizeX - 1; x++)
                {
                    int lowerLeft = x + y * terrainSizeX;
                    int lowerRight = (x + 1) + y * terrainSizeX;
                    int topLeft = x + (y + 1) * terrainSizeX;
                    int topRight = (x + 1) + (y + 1) * terrainSizeX;

                    terrainIndices[counter++] = topLeft;
                    terrainIndices[counter++] = lowerRight;
                    terrainIndices[counter++] = lowerLeft;

                    terrainIndices[counter++] = topLeft;
                    terrainIndices[counter++] = topRight;
                    terrainIndices[counter++] = lowerRight;
                }
            }
        }

        private void CopyTerrainToBuffers()
        {
            myVertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionColorNormal.VertexDeclaration, terrainVertices.Length, BufferUsage.WriteOnly);
            myVertexBuffer.SetData(terrainVertices);

            myIndexBuffer = new IndexBuffer(GraphicsDevice, typeof(int), terrainIndices.Length, BufferUsage.WriteOnly);
            myIndexBuffer.SetData(terrainIndices);
        }

        #region content
        /// <summary>
        /// LoadContent will be called once per game and is the place to load all content.
        /// </summary>
        protected override void LoadContent()
        {   
            // Load Help/Welcome screen
            LoadWelcome();

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>(@"Content\Arial");

            // Load effects for Submarine and horizoncube
            LoadHorizon();

            // Load heightmap, terrain and surface
            LoadTerrainData();
            SetTerrainVertices();
            SetTerrainIndices();
            CalculateTerrainNormals();
            CopyTerrainToBuffers();

            // Add Water elements
            LoadWaterElements();

            // Start new game
            CreateNewGame();

            // Load sky and gameboard edge
            LoadScenery();
        }

        // Create a new Game
        public void CreateNewGame()
        {
            // Load Player
            LoadPlayer();

            // Load Opponents
            LoadOpponents(opponentCount);
        }

        // Load Player
        private void LoadPlayer()
        {
            startPosition = new Vector3(0, waterLevel - 3, 0);
            Player = new Player(content, startPosition, this, heightData, waterLevel, terrainSizeX, terrainSizeZ);
        }

        // Load a video, and initialize a player, and the help screen sprite texture
        private void LoadWelcome()
        {
            wasdTexture = Content.Load<Texture2D>(@"Content\movement");
            video = Content.Load<Video>(@"Content/welcome");
            player = new VideoPlayer();
            player.IsLooped = true;
        }

        // Load effect for the HorizonCube
        private void LoadHorizon()
        {
            horizonEffect = Content.Load<Effect>(@"Content/MinEffekt2");
            effectWorld = horizonEffect.Parameters["World"];
            effectProjection = horizonEffect.Parameters["Projection"];
            effectView = horizonEffect.Parameters["View"];
        }

        // Load effects and textures for bubbles and surface - must be run after LoadTerrainData
        private void LoadWaterElements()
        {
            // Surface vertices, waterlevel before scale
            float min = 0.001f; float max = 0.999f;
            surfaceVertices = new VertexPositionColorTexture[4]
            {
                new VertexPositionColorTexture(new Vector3(-1, waterLevel, -1), Color.Blue, new Vector2(min,max)),
                new VertexPositionColorTexture(new Vector3( 1, waterLevel, -1), Color.Blue, new Vector2(min,min)),
                new VertexPositionColorTexture(new Vector3(-1, waterLevel,  1), Color.Blue, new Vector2(max,max)),
                new VertexPositionColorTexture(new Vector3(1, waterLevel,  1), Color.Blue, new Vector2(max,min))
            };

            // Recalculating waterlevel for real scale
            waterLevel = terrainPeak * waterLevel;

            dropTexture = Content.Load<Texture2D>(@"Content\bubble");
            bubbleEffect = Content.Load<Effect>(@"Content\Particle2");
            bubbleEffect.CurrentTechnique = bubbleEffect.Techniques["Technique1"];
            bubbleEffect.Parameters["theTexture"].SetValue(dropTexture);
            surfaceEffect = Content.Load<Effect>(@"Content/waterEffectt");
            surfaceTexture = content.Load<Texture2D>(@"Content\pond-water-texture");
            effectSurfaceWorld = surfaceEffect.Parameters["World"];
            effectSurfaceProjection = surfaceEffect.Parameters["Projection"];
            effectSurfaceView = surfaceEffect.Parameters["View"];
            effectSurfacePosition = surfaceEffect.Parameters["fx_Pos"];
            effectSurfaceAlpha = surfaceEffect.Parameters["fx_Alpha"];
        }

        // Load Scenery elements, SkyDome and HorizonCube
        private void LoadScenery()
        {
            skyDome = Content.Load<Model>(@"Content\dome");
            skyDome.Meshes[0].MeshParts[0].Effect = terrainEffect.Clone();
            cloudTexture = content.Load<Texture2D>(@"Content\cloudMap");
        }

        // Load Opponents
        private void LoadOpponents(int count)
        {
            //Texture2D subT = Content.Load<Texture2D>(@"Content\steel");
            subMarine = new SubMarine(wasdTexture, startPosition);
            
            // Prepare a list of opponents, fill with the given number
            opponents = new ArrayList();
            for (int i = 0; i < count; i++)
            {
                opponents.Add(new Fish(content, heightData, waterLevel, terrainSizeX, terrainSizeZ));
                System.Threading.Thread.Sleep(15);
            }
        }

        // Load heightdata from Texture2D heightmap
        private void LoadTerrainData()
        {
            terrainEffect = Content.Load<Effect>(@"Content/effects");
            Texture2D heightMap = Content.Load<Texture2D>(@"Content/oceanmap");

            terrainSizeX = heightMap.Width;
            terrainSizeZ = heightMap.Height;

            Color[] heightMapColors = new Color[terrainSizeX * terrainSizeZ];
            heightMap.GetData(heightMapColors);

            heightData = new float[terrainSizeX, terrainSizeZ];
            for (int x = 0; x < terrainSizeX; x++)
                for (int y = 0; y < terrainSizeZ; y++)
                {
                    float point = heightMapColors[x + y * terrainSizeX].R / 2.80f;
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
            // Play the video if it isn't already.
            if (player.State != MediaState.Playing)
                player.Play(video);

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Time elapsed since the last call to update.
            elapsedTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            fpsCalc(gameTime);
            UpdateSurface(gameTime);
            UpdateAnimation(gameTime);
            UpdateBubbles(gameTime);

            // Update Player, and make FishCam follow
            Player.Update(gameTime);
            camera.Rotation = Matrix.CreateFromQuaternion(Player.Rotation);
            camera.Position = Player.Position;
            camera.Update(gameTime);

            // Display help menu on request
            if (input.KeyboardState.IsKeyDown(Keys.H))
                if (timeSinceLastHelpRequest.TotalMilliseconds > 100)
                {
                    timeSinceLastHelpRequest = new TimeSpan();
                    showHelp = !showHelp;
                }

            if (input.KeyboardState.IsKeyDown(Keys.N))
                CreateNewGame();

            base.Update(gameTime);
        }

        // Update bubbles
        protected void UpdateBubbles(GameTime gameTime)
        {
            // Loop through and update explosions 
            for (int i = 0; i < bubbles.Count; ++i)
            {
                bubbles[i].Update(gameTime);
                // If explosion is finished, remove it 
                if (bubbles[i].IsDead)
                {
                    bubbles.RemoveAt(i);
                    --i;
                }
            }
        }

        // Prepare bubbles at the given position
        private void MakeBubbles(Vector3 pos)
        {
            bubbles.Add(new ParticleExplosion(GraphicsDevice,
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

        // Update the surface movement
        void UpdateSurface(GameTime gameTime)
        {
            if (surfaceMovement > 2)
                surfaceMovement = 0;

            surfaceMovement += (float)gameTime.ElapsedGameTime.Milliseconds / 10000.0f;
       }

        //Set direction on animation
        void UpdateAnimation(GameTime gameTime)
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

        // Calculate terrain normals
        private void CalculateTerrainNormals()
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

        // Draw text over the game rendering
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

        // Debug position vs heightmap
        private void DrawHeightVsPos()
        {
            int x = (int)Player.Position.X;
            int z = (int)Player.Position.Z;
            float y = Player.Position.Y;
            float h = 0;
            try
            {
                h = heightData[x+133, -z+133];
            }
            catch { }
            DrawOverlayText(string.Format("X: {0} Y: {1} Z: {2}, H: {3}", x, y, z, h), 50, 60);
        }

        // Draw the help screen
        private void DrawHelpScreen()
        {
            // Nice welcome video
            Texture2D videoFrame;
            if (player.State == MediaState.Playing)
                videoFrame = player.GetTexture();
            else
                videoFrame = null;

            // Prepare video display
            Rectangle videoScreen = new Rectangle(
                GraphicsDevice.Viewport.X + 50,
                GraphicsDevice.Viewport.Y + 50,
                GraphicsDevice.Viewport.Width - 100,
                (GraphicsDevice.Viewport.Height / 2));

            // Prepage keys 2D sprite
            Rectangle keyScreen = new Rectangle(
                GraphicsDevice.Viewport.X + GraphicsDevice.Viewport.Width / 3,
                GraphicsDevice.Viewport.Y + GraphicsDevice.Viewport.Height * 3/ 4,
                wasdTexture.Width,
                wasdTexture.Height);

            String helpheader = "The point of this game is to eat all other\n" +
                            "fish to become the biggest of them all." +
                            "\n\nHow to Play:\n  (H) Show/hide this screen.\n  (Esc) Exit the game\n  (N) New game\n\n"+
                            "See keys below.";
            String helptext = "  Movement       Look up/down";

            Vector2 pos = spriteFont.MeasureString(helpheader);
            pos.X = graphics.GraphicsDevice.Viewport.Height / 2 - (pos.X / 4);
            pos.Y = graphics.GraphicsDevice.Viewport.Height / 2 - 200;

            spriteBatch.Begin();
            // Write header.
            if (videoFrame != null)
                spriteBatch.Draw(videoFrame, videoScreen, Color.White);
            spriteBatch.DrawString(spriteFont, helpheader, pos, Color.Black);
            spriteBatch.DrawString(spriteFont, helpheader, new Vector2(pos.X - 1, pos.Y - 1), Color.White);

            // Draw keys, write explanation
            spriteBatch.Draw(wasdTexture, keyScreen, Color.White);
            pos.X = keyScreen.X;
            pos.Y = keyScreen.Y - 40;
            spriteBatch.DrawString(spriteFont, helptext, pos, Color.Black);
            spriteBatch.DrawString(spriteFont, helptext, new Vector2(pos.X - 1, pos.Y - 1), Color.White);
            
            spriteBatch.End();
            //Må sette diverse parametre tilbake siden SpriteBatch justerer flere parametre (se Shawn Hargreaves Blog):
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone; //Avhenger av hva du ønsker
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        }

        // Draw the surface
        protected void DrawSurface()
        {
            GraphicsDevice.Textures[0] = surfaceTexture;

            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            // Custom blendstate to achieve transparency
            BlendState bs = new BlendState();
            bs.ColorSourceBlend = Blend.DestinationColor;
            bs.ColorDestinationBlend = Blend.SourceColor;
            bs.AlphaBlendFunction = BlendFunction.Add;
            GraphicsDevice.BlendState = bs;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // Surface
            for (int i = 0; i <= 2; i++)
            {
                effectSurfacePosition.SetValue(surfaceMovement-((i-1)*2));

                effectSurfaceWorld.SetValue(world);
                effectSurfaceView.SetValue(camera.View);
                effectSurfaceProjection.SetValue(camera.Projection);
                effectSurfaceAlpha.SetValue(0.5f);
                Indices = new int[6] { 0, 1, 2, 2, 1, 3 };

                Matrix matIdentify = Matrix.Identity;

                // Scale surface to fit terrain
                Matrix scale = Matrix.CreateScale(terrainSizeX, terrainPeak, terrainSizeX);

                world = matIdentify * scale;
                // Draw
                foreach (EffectPass pass in surfaceEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, surfaceVertices, 0, 4, Indices, 0, 2);
                }
            }
            
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        // Draw the terrain
        public void DrawTerrain()
        {
            Matrix matIdentify = Matrix.Identity;
            // Sentrer terreng i origo
            Matrix translate = Matrix.CreateTranslation(-terrainSizeX / 2.0f, 0, terrainSizeZ / 2.0f);

            world = matIdentify * translate;

            terrainEffect.CurrentTechnique = terrainEffect.Techniques["Colored"];
            terrainEffect.Parameters["xView"].SetValue(camera.View);
            terrainEffect.Parameters["xProjection"].SetValue(camera.Projection);
            terrainEffect.Parameters["xWorld"].SetValue(world);
            Vector3 lightDirection = new Vector3(1.0f, -1.0f, -1.0f);
            lightDirection.Normalize();
            terrainEffect.Parameters["xLightDirection"].SetValue(lightDirection);
            terrainEffect.Parameters["xAmbient"].SetValue(0.1f);
            terrainEffect.Parameters["xEnableLighting"].SetValue(true);

            // Draw
            foreach (EffectPass pass in terrainEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.Indices = myIndexBuffer;
                GraphicsDevice.SetVertexBuffer(myVertexBuffer);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, terrainVertices.Length, 0, terrainIndices.Length / 3);
            }
        }

        // Draw the SubMarine. TODO: Doesn't work
        private void DrawSub()
        {
            GraphicsDevice.Textures[0] = subMarine.Texture;

            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            BlendState bs = new BlendState();
            bs.ColorSourceBlend = Blend.DestinationColor;
            bs.ColorDestinationBlend = Blend.SourceColor;
            bs.AlphaBlendFunction = BlendFunction.Add;
            GraphicsDevice.BlendState = bs;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            Matrix trans;

            trans = Matrix.CreateTranslation(subMarine.Position);
        
            Matrix matIdentify = Matrix.Identity;
            Matrix scale;

            Matrix.CreateScale(5, 5, 15, out scale);

            world = matIdentify * scale * trans;

            //Starter tegning
            foreach (EffectPass pass in horizonEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, subMarine.Vertices, 0, 1329, VertexPositionColorTextureNormal.VertexDeclaration);
            }

            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        // Draw a given Fish
        private void DrawFish(Fish fish)
        {
            Matrix world = Matrix.Identity;
            effectWorld.SetValue(world);
            effectView.SetValue(camera.View);
            effectProjection.SetValue(camera.Projection);

            Matrix trans = Matrix.CreateTranslation(fish.Position);
        
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

            world *= fish.Scale * Matrix.CreateFromQuaternion(fish.Rotation) * trans;
            fish.World = world;
            foreach (ModelMesh mesh in fish.meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = fish.matrix[mesh.ParentBone.Index] * world;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                    effect.EnableDefaultLighting();
                    effect.LightingEnabled = true;
                    
                }
                mesh.Draw();
            }

            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        }

        // Draw opponent players
        public void DrawOpponents()
        {
            Object[] p = opponents.ToArray();
            for (int i = 0; i < p.Length; i++) {
                Fish fish = (Fish)p[i];
                if (Player.hits(fish, fish.World) || fish.hits(Player, Player.World))
                {
                    MakeBubbles(fish.Position);
                    opponents.Remove(fish);
                    if (opponents.Count == 0)
                        Player.Alive = false;
                }
                else
                    DrawFish(fish);
            }
        }

        // Draw the sky
        private void DrawSkyDome()
        {
            Matrix[] modelTransforms = new Matrix[skyDome.Bones.Count];
            skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);
            

            Matrix wMatrix = Matrix.CreateTranslation(0, -0.3f, 0) * Matrix.CreateScale(300) * Matrix.CreateTranslation(camera.Position);
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

        // Draw Horizon side
        private void DrawSeaSide(VertexPositionColorTexture[] vertices, Texture2D texture)
        {
            Matrix matIdentify = Matrix.Identity;
            Matrix scale = Matrix.CreateScale(terrainSizeX/2, waterLevel, terrainSizeZ/2);

            world = matIdentify * scale;

            GraphicsDevice.Textures[0] = texture;

            effectWorld.SetValue(world);
            effectView.SetValue(camera.View);
            effectProjection.SetValue(camera.Projection);

            //Starter tegning av cuben
            foreach (EffectPass pass in horizonEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
            }

        }

        // Draw the seabox
        private void DrawSeaBox()
        {
            Texture2D videoFrame;
            if (player.State == MediaState.Playing)
                videoFrame = player.GetTexture();
            else
                videoFrame = null;

            DrawSeaSide(backV, videoFrame);
            DrawSeaSide(rightV, videoFrame);
            DrawSeaSide(frontV, videoFrame);
            DrawSeaSide(leftV, videoFrame);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Keypress timer for help screen
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
                // Draw background
                DrawSkyDome();
                DrawSeaBox();

                // Draw Player and opponents
                DrawFish(Player);
                DrawOpponents();

                DrawSub(); // TODO: buggy - sub under terrain and doesn't look like a sub at all
                DrawTerrain();

                // Draw water
                DrawSurface();
                for (int i = 0; i < bubbles.Count; i++)
                    bubbles[i].Draw(bubbleEffect, camera);
            }
            else
            {   // Player died
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
            {
                DrawHelpScreen();
                //DrawHeightVsPos();
            }
            else
            {
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