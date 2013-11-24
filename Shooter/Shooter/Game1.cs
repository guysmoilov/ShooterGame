using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Shooter.Utility;
using Shooter.Scenes;

namespace Shooter
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // Utilities
        public SoundMaster soundMaster;
        public TextureMaster textureMaster;

        // Scenes
        public Scene[] scenes;
        public int activeScene = 0;

        // Graphics
        GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;

        public bool resultionIndependent = false;
        public Vector2 baseScreenSize = new Vector2(800, 600);
        public int screenWidth;
        public int screenHeight;
        public Matrix GlobalTransformationMatrix { get; protected set; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Window properties
            graphics.PreferredBackBufferWidth = 500;
            graphics.PreferredBackBufferHeight = 500;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = "Shooter 2D";

            if (resultionIndependent)
            {
                screenWidth = (int)baseScreenSize.X;
                screenHeight = (int)baseScreenSize.Y;
            }
            else
            {
                screenWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
                screenHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
            }

            Vector3 screenScalingFactor;
            if (resultionIndependent)
            {
                float horScaling = (float)GraphicsDevice.PresentationParameters.BackBufferWidth / baseScreenSize.X;
                float verScaling = (float)GraphicsDevice.PresentationParameters.BackBufferHeight / baseScreenSize.Y;
                screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            }
            else
            {
                screenScalingFactor = new Vector3(1, 1, 1);
            }
            this.GlobalTransformationMatrix = Matrix.CreateScale(screenScalingFactor);

            // Initialize uitlites
            soundMaster = new SoundMaster(this);
            textureMaster = new TextureMaster(this);

            base.Initialize();
            
            // Init scenes
            scenes = new Scene[] { new BirdScene(this), new RocketScene(this) };
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load textures
            textureMaster.Sprites.Add("background", Content.Load<Texture2D>("background"));
            textureMaster.Sprites.Add("carriage", Content.Load<Texture2D>("carriage"));
            textureMaster.ColorArrays.Add("carriage", TextureHelper.TextureTo2DArray(textureMaster.Sprites["carriage"]));
            textureMaster.Sprites.Add("cannon", Content.Load<Texture2D>("cannon"));
            textureMaster.ColorArrays.Add("cannon", TextureHelper.TextureTo2DArray(textureMaster.Sprites["cannon"]));
            textureMaster.Sprites.Add("rocket", Content.Load<Texture2D>("rocket"));
            textureMaster.ColorArrays.Add("rocket", TextureHelper.TextureTo2DArray(textureMaster.Sprites["rocket"]));
            textureMaster.Sprites.Add("smoke", Content.Load<Texture2D>("smoke"));
            textureMaster.Sprites.Add("ground", Content.Load<Texture2D>("ground"));
            textureMaster.Sprites.Add("explosion", Content.Load<Texture2D>("explosion"));
            textureMaster.ColorArrays.Add("explosion", TextureHelper.TextureTo2DArray(textureMaster.Sprites["explosion"]));
            textureMaster.SpriteFonts.Add("myFont", Content.Load<SpriteFont>("myFont"));

            // Load sounds
            soundMaster.SFX.Add("hitcannon", Content.Load<SoundEffect>("hitcannon"));
            soundMaster.SFX.Add("hitterrain", Content.Load<SoundEffect>("hitterrain"));
            soundMaster.SFX.Add("launch", Content.Load<SoundEffect>("launch"));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Update active scene
            if (activeScene < scenes.Length)
            {
                scenes[activeScene].Update(gameTime);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, GlobalTransformationMatrix);

            // Draw active scene
            if (activeScene < scenes.Length)
            {
                scenes[activeScene].Draw(gameTime);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public virtual void StartParticleRendering()
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, GlobalTransformationMatrix);
        }

        public virtual void EndParticleRendering()
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, GlobalTransformationMatrix);
        }
    }
}
