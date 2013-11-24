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
    class Rocket : DrawableGameComponent
    {
        // Consts
        public float CANNON_TO_ROCKET_SCALING_FACTOR = 0.2f;

        // Mechanics
        public Vector2 Position;
        public bool IsNew = true;
        public Vector2 Direction;
        public float Angle;
        public float Gravity = 10f;
        public Vector2 GravityVector = Vector2.UnitY;
        public List<Particle> Smoke = new List<Particle>();
        public Vector2 Wind = Vector2.Zero;
        public int SmokeDrift = 5;
        public float SmokeScaling = 5f;
        public bool IsFlying = true;
        public TimeSpan SmokeParticleLifespan = TimeSpan.FromSeconds(3);
        Random randomizer = new Random();
        public Cannon shooter;

        // Graphics
        public Texture2D RocketTexture;
        public Texture2D SmokeTexture;
        public Color[,] RocketColorArray;
        public Vector2 RocketCenter = new Vector2(42, 240);
        public float Scaling = 0.1f;
        public Color RocketColor = Color.Black;
        public Color SmokeColor = Color.White;

        // Sounds
        public SoundEffect LaunchSound;

        // Game services
        SpriteBatch spriteBatch;
        TextureMaster textureMaster;
        SoundMaster soundMaster;

        public Rocket(Game game) : base (game)
        {
            spriteBatch = (game as Game1).spriteBatch;
            textureMaster = (game as Game1).textureMaster;
            soundMaster = (game as Game1).soundMaster;

            Initialize();
        }

        public Rocket(Game game, Cannon shooter)
            : base(game)
        {
            spriteBatch = (game as Game1).spriteBatch;
            textureMaster = (game as Game1).textureMaster;
            soundMaster = (game as Game1).soundMaster;

            // Setup parameters
            this.Position = shooter.Position + shooter.CANNON_OFFSET;
            this.Angle = shooter.Angle;
            var RotationMatrix = Matrix.CreateRotationZ(this.Angle);
            this.Direction = Vector2.Transform(-Vector2.UnitY, RotationMatrix);
            this.Direction *= shooter.Power * shooter.ShotPowerModifier;
            this.RocketColor = shooter.Color;
            this.Scaling = shooter.scaling * CANNON_TO_ROCKET_SCALING_FACTOR;

            this.shooter = shooter;

            Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            LaunchSound = soundMaster.SFX["launch"];
            SmokeTexture = textureMaster.Sprites["smoke"];
            RocketTexture = textureMaster.Sprites["rocket"];
            RocketColorArray = textureMaster.ColorArrays["rocket"];
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Launch
            if (IsNew)
            {
                LaunchSound.Play();
                IsNew = false;
            }

            // Only move and add smoke if still flying
            if (IsFlying)
            {
                // Move
                Direction += GravityVector / 10.0f;
                Position += Direction;
                Angle = (float)Math.Atan2(Direction.X, -Direction.Y);

                // Add smoke
                var smk = new Particle(Game);
                smk.Texture = SmokeTexture;
                smk.Position = Position + new Vector2(randomizer.Next(SmokeDrift * 2) - SmokeDrift, randomizer.Next(SmokeDrift * 2) - SmokeDrift);
                smk.OrginalPosition = smk.Position;
                smk.Direction = Vector2.Zero;
                smk.Accelaration = Wind;
                //smk.BirthTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
                smk.MaxAge = (float)SmokeParticleLifespan.TotalMilliseconds;
                smk.ParticleColor = SmokeColor;
                smk.Scaling = this.Scaling * this.SmokeScaling;
                Smoke.Add(smk); 
            }

            // Update smoke
            for (int i = 0; i < Smoke.Count; i++)
            {
                if (Smoke[i].Enabled)
                {
                    Smoke[i].Update(gameTime);
                }

                if (!Smoke[i].Enabled)
                {
                    Smoke.RemoveAt(i);
                    i--;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (IsFlying)
            {
                spriteBatch.Draw(RocketTexture, Position, null, RocketColor, Angle, RocketCenter, Scaling, SpriteEffects.None, 1); 
            }

            // Draw smoke
            (Game as Game1).StartParticleRendering();

            foreach (var smk in Smoke)
            {
                if (smk.Visible)
                {
                    smk.Draw(gameTime);
                }
            }

            (Game as Game1).EndParticleRendering();
        }
    }
}
