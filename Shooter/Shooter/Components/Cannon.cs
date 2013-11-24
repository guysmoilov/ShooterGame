using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Shooter.Utility;
using Microsoft.Xna.Framework.Input;

namespace Shooter.Components
{
    public class Cannon : DrawableGameComponent
    {
        // Mechanics
        public Vector2 Position;
        public bool IsAlive;
        public float Angle;
        public float Power;
        public float ShotPowerModifier = 0.1f;
        public bool IsActive = false;
        public Keys LeftKey = Keys.Left;
        public Keys RightKey = Keys.Right;
        public Keys PowerUpKey = Keys.Up;
        public Keys PowerDownKey = Keys.Down;
        public Keys LargePowerUpKey = Keys.PageUp;
        public Keys LargePowerDownKey = Keys.PageDown;
        
        // Graphics
        public Color Color;
        public Texture2D carriageTexture;
        public Color[,] carriageColorArray;
        public Texture2D barrelTexture;
        public Color[,] barrelColorArray;
        public float scaling;
        public Vector2 CANNON_CENTER = new Vector2(11, 50);
        public Vector2 CANNON_OFFSET = new Vector2(20, -10);
        protected float _width;
        public float Width 
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
                if (carriageTexture != null)
                {
                    scaling = value / carriageTexture.Width;
                    //CANNON_OFFSET *= scaling;
                }
            }
        }

        // Game services
        SpriteBatch spriteBatch;
        TextureMaster textureMaster;

        public Cannon(Game game) : base (game)
        {
            spriteBatch = (game as Game1).spriteBatch;
            textureMaster = (game as Game1).textureMaster;

            Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            carriageTexture = textureMaster.Sprites["carriage"];
            carriageColorArray = textureMaster.ColorArrays["carriage"];
            barrelTexture = textureMaster.Sprites["cannon"];
            barrelColorArray = textureMaster.ColorArrays["cannon"];

            // To reset scaling
            Width = Width;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (barrelTexture != null && carriageTexture != null)
            {
                spriteBatch.Draw(
                    barrelTexture,
                    Position + CANNON_OFFSET,
                    null,
                    Color,
                    Angle,
                    CANNON_CENTER,
                    scaling,
                    SpriteEffects.None,
                    1);
                spriteBatch.Draw(
                    carriageTexture,
                    Position,
                    null,
                    Color,
                    0,
                    new Vector2(0, carriageTexture.Height),
                    scaling,
                    SpriteEffects.None,
                    0);  
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsActive)
            {
                KeyboardState keybState = Keyboard.GetState();
                if (keybState.IsKeyDown(LeftKey))
                    Angle -= 0.01f;
                if (keybState.IsKeyDown(RightKey))
                    Angle += 0.01f;

                if (Angle > MathHelper.PiOver2)
                    Angle = -MathHelper.PiOver2;
                if (Angle < -MathHelper.PiOver2)
                    Angle = MathHelper.PiOver2;

                if (keybState.IsKeyDown(PowerDownKey))
                    Power -= 1;
                if (keybState.IsKeyDown(PowerUpKey))
                    Power += 1;
                if (keybState.IsKeyDown(LargePowerDownKey))
                    Power -= 20;
                if (keybState.IsKeyDown(LargePowerUpKey))
                    Power += 20;

                if (Power > 1000)
                    Power = 1000;
                if (Power < 0)
                    Power = 0;
            }
        }
    }
}
