using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Shooter.Scenes
{
    public class Scene : DrawableGameComponent
    {
        public SpriteBatch spriteBatch;
        public List<GameComponent> SceneComponents { get; set; }
        public bool IsOver { get; set; }

        public Scene(Game game)
            : base(game)
        {
            SceneComponents = new List<GameComponent>();
            this.spriteBatch = (SpriteBatch)game.Services.GetService(typeof(SpriteBatch));

            Visible = false;
            Enabled = false;
        }

        public void Show()
        {
            Enabled = true;
            Visible = true;
        }

        public void Hide()
        {
            Enabled = false;
            Visible = false;
        }

        public void Pause()
        {
            Enabled = false;
            Visible = true;
        }

        public override void Update(GameTime gameTime)
        {
            foreach (GameComponent Component in SceneComponents)
            {
                if (Component != null && Component.Enabled) Component.Update(gameTime);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // Draw scene components
            foreach (GameComponent Component in SceneComponents)
            {
                var comp = Component as DrawableGameComponent;
                if (comp != null)
                {
                    if (comp.Visible)
                    {
                        comp.Draw(gameTime);
                    }
                }
            }

            base.Draw(gameTime);
        }
    }
}
