using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shooter.Components;
using Microsoft.Xna.Framework;
using Shooter.Utility;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace Shooter.Scenes
{
    class ShooterScene : Scene
    {
        // Consts
        public float CANNON_WIDTH = 40.0f;
        public int TERRAIN_OFFSET;
        public int TERRAIN_FLATNESS = 100;
        public int TERRAIN_PEAK_HEIGHT = 120;
        public TimeSpan ROCKET_FIRE_RATE = TimeSpan.FromSeconds(2f);
        public int EXPLOSION_PARTICLE_NUM = 10;
        public float EXPLOSION_SCALING = 0.15f;
        public float TERRAIN_EXPLOSION_SIZE = 40f;
        public float CANNON_EXPLOSION_SIZE = 80f;

        // Components
        public List<Cannon> cannons = new List<Cannon>();
        public int numOfCannons = 2;
        public int ActivePlayer = 0;
        public Color[] cannonColors = new Color[]
        { 
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.Purple,
            Color.Yellow,
            Color.Orange,
            Color.Indigo,
            Color.SaddleBrown,
            Color.Turquoise,
            Color.White,
            Color.Black
        };

        public Terrain terrain;

        public List<Rocket> Rockets = new List<Rocket>();
        public DateTime lastRocketFireTime = DateTime.Now;
        public Vector2 Wind;

        public List<Explosion> Explosions = new List<Explosion>();
        public SoundEffect ExplodingCannonSound;

        // Game services
        protected TextureMaster textureMaster;
        protected SoundMaster soundMaster;
        protected SpriteBatch spriteBatch;
        protected int screenHeight;
        protected int screenWidth;

        public ShooterScene(Game1 game) : base(game)
        {
            textureMaster = game.textureMaster;
            soundMaster = game.soundMaster;
            spriteBatch = game.spriteBatch;
            screenHeight = game.GraphicsDevice.Viewport.Height;
            screenWidth = game.GraphicsDevice.Viewport.Width;

            this.Initialize();
        }

        public override void Initialize()
        {
            CreateTerrainContour();
            SetupCannons();
            FlattenTerrainBelowPlayers();
            CreateForeground();

            base.Initialize();

            SceneComponents.InsertRange(0, from cn in cannons select (cn as GameComponent));
            SceneComponents.Add(terrain as GameComponent);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            ExplodingCannonSound = soundMaster.SFX["hitcannon"];
        }

        public virtual void CreateTerrainContour()
        {
            terrain = new Terrain(Game);
            terrain.Flatness = TERRAIN_FLATNESS;
            TERRAIN_OFFSET = terrain.Offset = screenHeight / 2;
            terrain.PeakHeight = TERRAIN_PEAK_HEIGHT;
            terrain.GenerateContour(screenWidth);
        }

        public virtual void SetupCannons()
        {
            for (int i = 0; i < numOfCannons; i++)
            {
                cannons.Add(new Cannon(Game));

                cannons[i].IsAlive = true;
                cannons[i].Color = cannonColors[i];
                cannons[i].Angle = MathHelper.ToRadians(90);
                cannons[i].Power = 100;
                cannons[i].Position = new Vector2();
                cannons[i].Width = CANNON_WIDTH;
            }

            SetCannonPositions();

            cannons[ActivePlayer].IsActive = true;
        }

        public virtual void SetCannonPositions()
        {
            for (int i = 0; i < cannons.Count; i++)
            {
                cannons[i].Position.X = (screenWidth / (numOfCannons + 1) * (i + 1));
                cannons[i].Position.Y = terrain.Contour[(int)cannons[i].Position.X];
            }
        }

        public virtual void FlattenTerrainBelowPlayers()
        {
            var locs = (from cn in cannons where cn.IsAlive select (int)cn.Position.X).ToArray();
            terrain.FlattenTerrain(locs, (int)CANNON_WIDTH);
        }

        public virtual void CreateForeground()
        {
            terrain.GroundTexture = textureMaster.Sprites["ground"];
            terrain.ForegroundTexture = new Texture2D(Game.GraphicsDevice, screenWidth, screenHeight, false, SurfaceFormat.Color);
            terrain.CreateForeground(screenHeight);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Check whether to shoot rocket
            KeyboardState keybState = Keyboard.GetState();
            if (keybState.IsKeyDown(Keys.Enter) || keybState.IsKeyDown(Keys.Space))
            {
                if (DateTime.Now - lastRocketFireTime > ROCKET_FIRE_RATE)
                {
                    var rocket = ShootRocket(cannons[ActivePlayer]);
                    Rockets.Add(rocket);
                    SceneComponents.Add(rocket);
                    lastRocketFireTime = DateTime.Now;
                }
            }

            CheckOutOfBoundsRockets();
            CheckExpiredExplosions();
            CheckRocketTerrainHit();
            CheckRocketCannonHit();
        }

        public virtual void CyclePlayer() 
        {
            // Condition to check for infinite recursion
            if ((from cn in cannons where (cn.IsAlive && cn.IsActive) select cn).Count() > 0)
            {
                ActivePlayer++;
                ActivePlayer %= cannons.Count;
                if (!cannons[ActivePlayer].IsAlive || !cannons[ActivePlayer].IsActive)
                {
                    CyclePlayer();
                } 
            }
        }

        public virtual Rocket ShootRocket(Cannon shooter)
        {
            var rckt = new Rocket(Game, shooter);
            rckt.Wind = this.Wind;
            return rckt;
        }

        public virtual void CheckOutOfBoundsRockets()
        {
            for (int i = 0; i < Rockets.Count; i++)
            {
                // Check for out of bounds rockets
                if (Rockets[i].IsFlying &&
                    (Rockets[i].Position.X > Game.GraphicsDevice.Viewport.Width ||
                    Rockets[i].Position.X < 0 ||
                    Rockets[i].Position.Y > Game.GraphicsDevice.Viewport.Height))
                {
                    Rockets[i].IsFlying = false;
                }

                // Check for dead rockets
                if (Rockets[i].IsFlying == false && Rockets[i].Smoke.Count == 0)
                {
                    SceneComponents.Remove(Rockets[i]);
                    //Rockets[i] = null;
                    Rockets.RemoveAt(i);
                    i--;
                }
            }
        }

        public virtual void CheckRocketTerrainHit()
        {
            for (int i = 0; i < Rockets.Count; i++)
            {
                if (Rockets[i].IsFlying)
                {
                    var collision = RocketTerrainCollisionTest(Rockets[i], terrain);

                    if (collision != new Vector2(-1))
                    {
                        Rockets[i].IsFlying = false;

                        // Create explosion
                        var exp = new Explosion(Game);
                        exp.ParticleNum = EXPLOSION_PARTICLE_NUM;
                        exp.Position = Rockets[i].Position;
                        exp.Scaling = EXPLOSION_SCALING;
                        exp.ExplosionSize = TERRAIN_EXPLOSION_SIZE;
                        exp.Wind = Wind;
                        exp.Initialize();

                        Explosions.Add(exp);
                        SceneComponents.Add(exp);

                        // Add crater
                        AddCrater(terrain, exp.Position, exp.ExplosionSize);
                    }
                }
            }
        }

        public virtual Vector2 RocketTerrainCollisionTest(Rocket rocket, Terrain terrain)
        {
            Matrix rocketMatrix =
                        Matrix.CreateTranslation(new Vector3(-rocket.RocketCenter.X, -rocket.Position.Y, 0f)) *
                        Matrix.CreateRotationZ(rocket.Angle) *
                        Matrix.CreateScale(rocket.Scaling) *
                        Matrix.CreateTranslation(new Vector3(rocket.Position.X, rocket.Position.Y, 0f));

            Matrix terrainMat = Matrix.Identity;

            return TextureHelper.TexturesCollide(rocket.RocketColorArray, rocketMatrix, terrain.ForegroundColorArray, terrainMat);
        }

        public virtual void AddCrater(Terrain terrain, Vector2 craterCenter, float craterRadius)
        {
            for (int i = (int)(craterCenter.X - craterRadius); i < craterCenter.X + craterRadius; i++)
            {
                for (int j = (int)(craterCenter.Y - craterRadius); j < craterCenter.Y + craterRadius; j++)
                {
                    // Check bounds
                    if (i >= 0 && i < terrain.Contour.Length && j >= 0 && j < screenHeight)
                    {
                        var currentPos = new Vector2(i, j);
                        var difvec = craterCenter - currentPos;

                        // Check if in crater radius
                        if (difvec.Length() <= craterRadius)
                        {
                            // Check if the contour is higher
                            if (terrain.Contour[i] < currentPos.Y)
                            {
                                terrain.Contour[i] = (int)currentPos.Y;
                            }
                        }
                    }
                }
            }
            // Re-position cannons
            SetCannonPositions();

            // Flatten ground below cannons
            FlattenTerrainBelowPlayers();

            // Update terrain
            terrain.CreateForeground(screenHeight);
        }

        public virtual void CheckRocketCannonHit()
        {
            for (int i = 0; i < Rockets.Count; i++)
            {
                if (Rockets[i].IsFlying)
                {
                    for (int j = 0; j < cannons.Count; j++)
			        {
                        if (cannons[j].Enabled && cannons[j] != Rockets[i].shooter)
                        {
                            var collision = RocketCannonCollisionTest(Rockets[i], cannons[j]);

                            if (collision != new Vector2(-1))
                            {
                                Rockets[i].IsFlying = false;

                                // Create explosion
                                var exp = new Explosion(Game);
                                exp.ParticleNum = EXPLOSION_PARTICLE_NUM;
                                exp.Position = Rockets[i].Position;
                                exp.Scaling = EXPLOSION_SCALING;
                                exp.ExplosionSize = CANNON_EXPLOSION_SIZE;
                                exp.Wind = Wind;
                                exp.Initialize();
                                exp.ExplosionSound = ExplodingCannonSound;

                                Explosions.Add(exp);
                                SceneComponents.Add(exp);

                                // Destroy cannon
                                DestroyCannon(j);
                            } 
                        }
			        }
                }
            }
        }

        public virtual void DestroyCannon(int cannonNum)
        {
            // Check if need to switch
            if (cannonNum == ActivePlayer)
            {
                CyclePlayer();
            }

            this.SceneComponents.Remove(cannons[cannonNum]);
            this.cannons.RemoveAt(cannonNum);
        }

        public virtual Vector2 RocketCannonCollisionTest(Rocket rocket, Cannon cannon)
        {
            Matrix rocketMatrix =
                Matrix.CreateTranslation(new Vector3(-rocket.RocketCenter.X, -rocket.Position.Y, 0f)) *
                Matrix.CreateRotationZ(rocket.Angle) *
                Matrix.CreateScale(rocket.Scaling) *
                Matrix.CreateTranslation(new Vector3(rocket.Position.X, rocket.Position.Y, 0f));

            Matrix carriageMatrix =
                Matrix.CreateTranslation(new Vector3(0, -cannon.carriageTexture.Height, 0)) *
                Matrix.CreateScale(cannon.scaling) *
                Matrix.CreateTranslation(new Vector3(cannon.Position, 0));

            return TextureHelper.TexturesCollide(rocket.RocketColorArray, rocketMatrix, cannon.carriageColorArray, carriageMatrix);
        }

        public virtual void CheckExpiredExplosions()
        {
            for (int i = 0; i < Explosions.Count; i++)
            {
                if (Explosions[i].IsDone)
                {
                    SceneComponents.Remove(Explosions[i]);
                    Explosions.RemoveAt(i);
                    i--;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            DrawText();
        }

        public virtual void DrawText()
        {
            var player = cannons[ActivePlayer];
            spriteBatch.DrawString(textureMaster.SpriteFonts["myFont"], "Cannon angle: " + ((int)(player.Angle * 180 / Math.PI)).ToString(), new Vector2(20, 20), player.Color);
            spriteBatch.DrawString(textureMaster.SpriteFonts["myFont"], "Cannon power: " + player.Power.ToString(), new Vector2(20, 45), player.Color);
        }
    }
}
