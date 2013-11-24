using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Shooter.Utility;

namespace Shooter.Components
{
    public class Terrain : DrawableGameComponent
    {
        public int[] Contour;
        public int Offset;
        public int Flatness;
        public int PeakHeight;

        public Texture2D ForegroundTexture;
        public Texture2D GroundTexture;
        public Color[,] ForegroundColorArray;

        // Game services
        SpriteBatch spriteBatch;

        public Terrain(Game game) : base(game)
        {
            spriteBatch = (game as Game1).spriteBatch;
        }

        public int[] GenerateContour(int width)
        {
            Contour = new int[width];

            Random randomizer = new Random();
            double rand1 = randomizer.NextDouble() + 1;
            double rand2 = randomizer.NextDouble() + 2;
            double rand3 = randomizer.NextDouble() + 3;

            for (int x = 0; x < width; x++)
            {
                double height = PeakHeight / rand1 * Math.Sin((float)x / Flatness * rand1 + rand1);
                height += PeakHeight / rand2 * Math.Sin((float)x / Flatness * rand2 + rand2);
                height += PeakHeight / rand3 * Math.Sin((float)x / Flatness * rand3 + rand3);
                height += Offset;
                Contour[x] = (int)height;
            }

            return Contour;
        }

        public void FlattenTerrain(int[] locations, int width)
        {
            foreach (var loc in locations)
            {
                for (int x = 0; x < width; x++)
                    Contour[loc + x] = Contour[loc];
            }
        }

        public void CreateForeground(int height)
        {
            // Calculate the colors
            Color[,] groundColors = TextureHelper.TextureTo2DArray(GroundTexture);
            Color[] foregroundColors = new Color[height * Contour.Length];

            for (int x = 0; x < Contour.Length; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (y > Contour[x])
                    {
                        foregroundColors[x + y * Contour.Length] = groundColors[x % GroundTexture.Width, y % GroundTexture.Height];

                        // Add snow
                        int contMax = Contour.Max();
                        int contMin = Contour.Min();
                        foregroundColors[x + y * Contour.Length] =
                            Color.Lerp(foregroundColors[x + y * Contour.Length], Color.White, (float)Math.Pow((float)(height - y) / (height - contMin), 1.5));
                    }
                    else
                        foregroundColors[x + y * Contour.Length] = Color.Transparent;
                }
            }

            // Set the foreground texture and color array
            ForegroundTexture.SetData(foregroundColors);
            ForegroundColorArray = TextureHelper.TextureTo2DArray(ForegroundTexture);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            spriteBatch.Draw(ForegroundTexture, new Rectangle(0, 0, Contour.Length, ForegroundColorArray.GetLength(1)), Color.White);
        }
    }
}
