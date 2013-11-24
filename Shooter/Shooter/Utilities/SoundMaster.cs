using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

namespace Shooter.Utility
{
    public class SoundMaster
    {
        public Dictionary<string, SoundEffect> SFX { private set; get; }
        public Game Game { get; set; }

        public SoundMaster(Game game)
        {
            this.Game = game;
            SFX = new Dictionary<string, SoundEffect>();
        }
    }
}
