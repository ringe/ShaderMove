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
    //Ny vertekstype:
    public struct ParticleVertex
    {
        public Vector3 position;
        public Vector2 textureCoordinate;

        public ParticleVertex(Vector3 position, Vector2 textureCoordinate)
        {
            this.position = position;
            this.textureCoordinate = textureCoordinate;
        }

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
            );
    }

    public struct Particle
    {
        public float size;
        public Vector3 direction;
        public ParticleVertex[] pvs; //ParticleVertexeS (PVS)

        public Particle(Vector3 _position, Vector2 _textureCoordinates, Vector3 _direction, float _size)
        {
            Vector3 p1, p2, p3;
            this.direction = _direction;
            this.size = _size;
            pvs = new ParticleVertex[3];
            p1 = new Vector3(_position.X, _position.Y, _position.Z);
            p2 = new Vector3(_position.X + size, _position.Y, _position.Z);
            p3 = new Vector3(_position.X, _position.Y + size, _position.Z);
            Vector2 tc1, tc2, tc3;
            tc1 = new Vector2(_textureCoordinates.X, _textureCoordinates.Y);
            tc2 = new Vector2(_textureCoordinates.X + size, _textureCoordinates.Y);
            tc3 = new Vector2(_textureCoordinates.X, _textureCoordinates.Y + size);
            //Oppretter 3 vertekser for denne partikkelen:
            pvs[0].position = p1;
            pvs[0].textureCoordinate = tc1;
            pvs[1].position = p2;
            pvs[1].textureCoordinate = tc2;

            pvs[2].position = p3;
            pvs[2].textureCoordinate = tc3;
        }

        public void UpdatePosition()
        {
            pvs[0].position += this.direction;
            pvs[1].position += this.direction;
            pvs[2].position += this.direction;
        }

        public void UpdateTextureCoordinate(Vector2 newCoordinates)
        {
            Vector2 tc1, tc2, tc3;
            tc1 = new Vector2(newCoordinates.X, newCoordinates.Y);
            tc2 = new Vector2(newCoordinates.X + size, newCoordinates.Y);
            tc3 = new Vector2(newCoordinates.X, newCoordinates.Y + size);
            pvs[0].textureCoordinate = tc1;
            pvs[1].textureCoordinate = tc2;
            pvs[2].textureCoordinate = tc3;
        }
    }

    public class ParticleSettings
    {
        // Size of particle 
        public int maxSize = 2;
    }

    public class ParticleExplosionSettings
    {
        // Life of particles 
        public int minLife = 1000;
        public int maxLife = 2000;
        // Particles per round 
        public int minParticlesPerRound = 100;
        public int maxParticlesPerRound = 600;
        // Round time 
        public int minRoundTime = 16;
        public int maxRoundTime = 50;
        // Number of particles 
        public int minParticles = 2000;
        public int maxParticles = 3000;
    }

    public class ParticleExplosion
    {
        // Array med Particle-objekter, opprettes i konstr. 
        Particle[] particles;
        // Kollisjonens posisjon. 
        Vector3 position;
        // Hvor mye “liv” som gjenstår før antall partikler begynner å avta 
        int lifeLeft;
        // Hvor mange nye partikler som skal slippes for hver “runde” 
        int numParticlesPerRound;
        // Maks antall partikler - totalt.
        int maxParticles;
        // Angir tiden mellom hver runde.
        int roundTime;
        // En teller som angir hvor mye tid som er gått siden starten  
        // på pågående runde.
        int timeSinceLastRound = 0;
        // Vertex & graphics-info.
        GraphicsDevice graphicsDevice;
        // Texture 
        Vector2 textureSize;
        // Settinger for hver enkelt partikkel – kun størrelse. 
        ParticleSettings particleSettings;
        // Følgende to variabler er indekser inn i partikkeltabellen: 
        // Markerer slutten på aktive partikler (se fig. Under) 
        int endOfLiveParticlesIndex = 0;
        // Markerer starten på aktive partikler (og slutten på ”døde”)
        int endOfDeadParticlesIndex = 0;
        // Bruker et enkelt random-objekt:
        static Random rnd = new Random();
        private ParticleVertex[] vertexes;

        // Konstruktør: 
        public ParticleExplosion(GraphicsDevice graphicsDevice, Vector3 position, int lifeLeft, int roundTime,
            int numParticlesPerRound, int maxParticles, Vector2 textureSize, ParticleSettings particleSettings)
        {
            this.position = position;
            this.lifeLeft = lifeLeft;
            this.numParticlesPerRound = numParticlesPerRound;
            this.maxParticles = maxParticles;
            this.roundTime = roundTime;
            this.graphicsDevice = graphicsDevice;
            this.textureSize = textureSize;
            this.particleSettings = particleSettings;
            particles = new Particle[maxParticles];
            InitializeParticles();
        }
        public bool IsDead
        {
            get { return endOfDeadParticlesIndex == maxParticles; }
        }
        private void InitializeParticles()
        {
            // Loop until max particles
            for (int i = 0; i < maxParticles; ++i)
            {
                Vector2 txtCoord = new Vector2(rnd.Next(0, (int)textureSize.X) /
                    textureSize.X, rnd.Next(0, (int)textureSize.Y) / textureSize.Y);
                Vector3 direction = new Vector3((float)rnd.NextDouble() * 2 - 1,
                    (float)rnd.NextDouble() * 2 - 1, (float)rnd.NextDouble() * 2 - 1);
                direction.Normalize();
                // Multiply by NextDouble to make sure that
                // all particles move at random speeds
                direction *= (float)rnd.NextDouble();
                //Particlesize:
                float size = ((float)rnd.NextDouble() * particleSettings.maxSize) / 10;
                particles[i] = new Particle(position, txtCoord, direction, size);
            }

            //Kopierer til vertekstabell:
            vertexes = new ParticleVertex[particles.Length * 3];
            for (int i = 0; i < particles.Length; i++)
            {
                vertexes[i * 3] = particles[i].pvs[0];
                vertexes[i * 3 + 1] = particles[i].pvs[1];
                vertexes[i * 3 + 2] = particles[i].pvs[2];
            }
        }

        public void Update(GameTime gameTime)
        {
            // Decrement life left until it's gone
            if (lifeLeft > 0)
                lifeLeft -= gameTime.ElapsedGameTime.Milliseconds;
            // Time for new round?
            timeSinceLastRound += gameTime.ElapsedGameTime.Milliseconds;
            if (timeSinceLastRound > roundTime)
            {
                // New round ‐ add and remove particles
                timeSinceLastRound -= roundTime;
                // Increment end of live particles index each
                // round until end of list is reached
                if (endOfLiveParticlesIndex < maxParticles)
                {
                    endOfLiveParticlesIndex += numParticlesPerRound;
                    if (endOfLiveParticlesIndex > maxParticles)
                        endOfLiveParticlesIndex = maxParticles;
                }
                if (lifeLeft <= 0)
                {
                    // Increment end of dead particles index each
                    // round until end of list is reached
                    if (endOfDeadParticlesIndex < maxParticles)
                    {
                        endOfDeadParticlesIndex += numParticlesPerRound;
                        if (endOfDeadParticlesIndex > maxParticles)
                            endOfDeadParticlesIndex = maxParticles;
                    }
                }
            }
            // Update positions of all live particles
            for (int i = endOfDeadParticlesIndex; i < endOfLiveParticlesIndex; ++i)
            {
                //particles[i].position += particles[i].direction;
                particles[i].UpdatePosition();

                // Assign a random texture coordinate for color
                // to create a flashing effect for each particle
                Vector2 newCoords = new Vector2(
                    rnd.Next(0, (int)textureSize.X) / textureSize.X,
                    rnd.Next(0, (int)textureSize.Y) / textureSize.Y);
                particles[i].UpdateTextureCoordinate(newCoords);
            }
            //Oppdater verteksene:
            for (int j = 0; j < particles.Length; j++)
            {
                vertexes[j * 3] = particles[j].pvs[0];
                vertexes[j * 3 + 1] = particles[j].pvs[1];
                vertexes[j * 3 + 2] = particles[j].pvs[2];
            }
        }

        public void Draw(Effect effect, FishCam camera)
        {
            // Only draw if there are live particles
            if (endOfLiveParticlesIndex - endOfDeadParticlesIndex > 0)
            {
                // Set HLSL parameters
                effect.Parameters["WorldViewProjection"].SetValue(camera.View * camera.Projection);
                // Draw particles
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList,
                                    vertexes, endOfDeadParticlesIndex * 3,
                                    endOfLiveParticlesIndex - endOfDeadParticlesIndex,
                                    ParticleVertex.VertexDeclaration);
                }
            }
        }
    }


    public class Bubbles
    {
        public Bubbles()
        {

        }
    }
}
    //    Random randomizer = new Random();
    //    private List<Drop> dropList;
    //    private Texture2D dropTexture;

    //    public List<Drop> Drops { get { return dropList; }}

    //    public Water(Texture2D texture)
    //    {
    //        dropTexture = texture;
    //        dropList = new List<Drop>();
    //    }

    //    public void AddDrops(Vector2 pos, int amount, float size, float maxAge, GameTime gameTime)
    //    {
    //        for (int i = 0; i < amount; i++)
    //            AddDrop(pos, size, maxAge, gameTime);
    //    }

    //    private void AddDrop(Vector2 pos, float size, float maxAge, GameTime gameTime)
    //    {
    //        Drop drop = new Drop();

    //        drop.OrginalPosition = pos;
    //        drop.Position = drop.OrginalPosition;

    //        drop.BirthTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
    //        drop.MaxAge = maxAge;
    //        drop.Scaling = 0.25f;
    //        drop.ModColor = Color.White;

    //        float particleDistance = (float)randomizer.NextDouble() * size;
    //        Vector2 displacement = new Vector2(particleDistance, 0);
    //        float angle = MathHelper.ToRadians(randomizer.Next(360));
    //        displacement = Vector2.Transform(displacement, Matrix.CreateRotationZ(angle));

    //        drop.Direction = displacement;
    //        drop.Accelaration = 3.0f * drop.Direction;

    //        dropList.Add(drop);
    //    }

    //    public void UpdateDrops(GameTime gameTime)
    //    {
    //        if (dropList.Count > 0)
    //        {
    //            float now = (float)gameTime.TotalGameTime.TotalMilliseconds;
    //            for (int i = dropList.Count - 1; i >= 0; i--)
    //            {
    //                Drop drop = dropList[i];
    //                float timeAlive = now - drop.BirthTime;

    //                if (timeAlive > drop.MaxAge)
    //                {
    //                    dropList.RemoveAt(i);
    //                }
    //                else
    //                {
    //                    float relAge = timeAlive / drop.MaxAge;
    //                    drop.Position = 0.5f * drop.Accelaration * relAge * relAge + drop.Direction * relAge + drop.OrginalPosition;

    //                    float invAge = 1.0f - relAge;
    //                    drop.ModColor = new Color(new Vector4(invAge, invAge, invAge, invAge));

    //                    Vector2 positionFromCenter = drop.Position - drop.OrginalPosition;
    //                    float distance = positionFromCenter.Length();
    //                    drop.Scaling = (50.0f + distance) / 200.0f;

    //                    dropList[i] = drop;
    //                }
    //            }
    //        }
    //    }

    //    public void Draw(ref SpriteBatch spriteBatch)
    //    {
    //        spriteBatch.Begin();
    //        for (int i = 0; i < dropList.Count; i++)
    //        {
    //             Drop particle = dropList[i];
    //             spriteBatch.Draw(dropTexture, particle.Position, null, particle.ModColor, i, new Vector2(256, 256), particle.Scaling, SpriteEffects.None, 1);
    //        }
    //        spriteBatch.End();
    //    }
    //}