using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shooter.Utility;
using Microsoft.Xna.Framework.Audio;

namespace Shooter.Components
{
    class Explosion : DrawableGameComponent
    {
        // Components
        public Vector2 Wind;
        public Vector2 Position;
        public float Scaling;
        public float ExplosionSize;
        public List<Particle> particles = new List<Particle>();
        public int ParticleNum;
        public bool IsDone = false;
        public bool IsNew = true;
        public float Duration = 1000f;

        // Graphics
        public Texture2D ParticleTexture;
        public SoundEffect ExplosionSound;

        // Game services
        SpriteBatch spriteBatch;
        TextureMaster textureMaster;
        SoundMaster soundMaster;
        Random randomizer = new Random();

        public Explosion(Game game) : base(game)
        {
            spriteBatch = (Game as Game1).spriteBatch;
            textureMaster = (Game as Game1).textureMaster;
            soundMaster = (Game as Game1).soundMaster;
        }

        public override void Initialize()
        {
            base.Initialize();

            for (int i = 0; i < ParticleNum; i++)
            {
                Particle particle = new Particle(Game);

                particle.OrginalPosition = Position;
                particle.Position = particle.OrginalPosition;

                particle.Texture = ParticleTexture;
                
                particle.Scaling = Scaling;
                particle.ParticleColor = Color.White;

                float particleDistance = (float)randomizer.NextDouble() * ExplosionSize;
                Vector2 displacement = new Vector2(particleDistance, 0);
                float angle = MathHelper.ToRadians(randomizer.Next(360));
                displacement = Vector2.Transform(displacement, Matrix.CreateRotationZ(angle));

                particle.Direction = displacement * 2.0f;
                particle.Accelaration = -particle.Direction;

                particles.Add(particle);
            }
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            ParticleTexture = textureMaster.Sprites["explosion"];
            ExplosionSound = soundMaster.SFX["hitterrain"];
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsNew)
            {
                ExplosionSound.Play();
                IsNew = false;

                // Create particle lifetimes
                foreach (var particle in particles)
                {
                    particle.BirthTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
                    particle.MaxAge = Duration; 
                }
            }

            // Update particles
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].Enabled)
                {
                    particles[i].Update(gameTime);
                }

                if (!particles[i].Enabled)
                {
                    particles.RemoveAt(i);
                    i--;
                }
            }

            // Turn off explosion
            IsDone = particles.Count == 0;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // Draw particles
            (Game as Game1).StartParticleRendering();

            foreach (var part in particles)
            {
                if (part.Visible)
                {
                    part.Draw(gameTime);
                }
            }

            (Game as Game1).EndParticleRendering();
        }
    }
}
