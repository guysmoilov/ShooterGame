using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Shooter.Utility
{
    public class TextureMaster
    {
        public Dictionary<string, Texture2D> Sprites { private set; get; }
        public Dictionary<string, SpriteFont> SpriteFonts { private set; get; }
        public Dictionary<string, Color[,]> ColorArrays { private set; get; }
        public Game Game { get; set; }

        public TextureMaster(Game game)
        {
            this.Game = game;
            Sprites = new Dictionary<string, Texture2D>();
            SpriteFonts = new Dictionary<string, SpriteFont>();
            ColorArrays = new Dictionary<string, Color[,]>();
        }
    }
}
