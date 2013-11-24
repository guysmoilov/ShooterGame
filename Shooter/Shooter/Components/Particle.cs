using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Shooter.Components
{
    public class Particle : DrawableGameComponent
    {
        public float BirthTime;
        public float MaxAge;
        public Vector2 OrginalPosition;
        public Vector2 Accelaration;
        public Vector2 Direction;
        public Vector2 Position;
        public float Scaling;
        public float InflationRate = 0.01f;
        public Color ParticleColor;
        public Texture2D Texture;
        public Vector2 ScalingOrigin = new Vector2(float.PositiveInfinity);
        public bool IsNew = true;
        public Color ModColor { get; set; }

        // Game services
        public SpriteBatch spriteBatch;

        public Particle(Game game) : base(game)
        {
            spriteBatch = (game as Game1).spriteBatch;

            Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsNew)
            {
                IsNew = false;
                BirthTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
            }

            float now = (float)gameTime.TotalGameTime.TotalMilliseconds;
            float timeAlive = now - BirthTime;

            if (timeAlive > MaxAge)
            {
                this.Enabled = false;
                this.Visible = false;
            }
            else
            {
                float relAge = timeAlive / MaxAge;
                Position = 0.5f * Accelaration * relAge * relAge + Direction * relAge + OrginalPosition;

                float invAge = 1.0f - relAge;
                ModColor = new Color(ParticleColor.ToVector4() * (new Vector4(invAge, invAge, invAge, invAge)));

                Vector2 positionFromCenter = Position - OrginalPosition;
                float distance = positionFromCenter.Length();

                if (Accelaration != Vector2.Zero)
                {
                    Scaling = Scaling + distance * relAge * InflationRate / Accelaration.Length();
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (Texture != null)
            {
                if (ScalingOrigin == new Vector2(float.PositiveInfinity))
                {
                    ScalingOrigin = new Vector2(Texture.Width / 2, Texture.Height / 2);
                }

                spriteBatch.Draw(Texture, Position, null, ModColor, 0f, ScalingOrigin, Scaling, SpriteEffects.None, 0);
            }
        }
    }
}
