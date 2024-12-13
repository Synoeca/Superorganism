using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Core.Camera;
using Superorganism.Core.Managers;
using Superorganism.Screens;

namespace Superorganism.ScreenManagement
{
    /// <summary>
    /// The ScreenManager is a component which manages one or more GameScreen instance.
    /// It maintains a stack of screens, calls their Update and Draw methods when 
    /// appropriate, and automatically routes input to the topmost screen.
    /// </summary>
    public class ScreenManager : DrawableGameComponent
    {
        private readonly List<GameScreen> _screens = [];
        private readonly List<GameScreen> _tmpScreensList = [];

        private readonly ContentManager _content;
        private readonly InputState _input = new();

        private bool _isInitialized;

        /// <summary>
        /// A SpriteBatch shared by all GameScreens
        /// </summary>
        public SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// A SpriteFont shared by all GameScreens
        /// </summary>
        public SpriteFont Font { get; private set; }

        /// <summary>
        /// A blank texture that can be used by the screens.
        /// </summary>
        public Texture2D BlankTexture { get; private set; }

        /// <summary>
        /// GraphicsDeviceManager for this game
        /// </summary>
        public GraphicsDeviceManager GraphicsDeviceManager { get; set; }

        /// <summary>
        /// DisplayMode for this game
        /// </summary>
        public DisplayMode DisplayMode { get; set; }

        /// <summary>
        /// GameAudioManager for this game
        /// </summary>
        public GameAudioManager GameAudioManager { get; set; }

        /// <summary>
        /// 2D Camera for this game
        /// </summary>
        public Camera2D GameplayScreenCamera2D { get; set; }

        /// <summary>
        /// Constructs a new ScreenManager
        /// </summary>
        /// <param name="game">The game this ScreenManager belongs to</param>
        public ScreenManager(Game game) : base(game)
        {
            _content = new ContentManager(game.Services, "Content");
        }

        public void SetDefaultGraphicsSettings()
        {
            GraphicsDeviceManager.IsFullScreen = true;
            GraphicsDeviceManager.HardwareModeSwitch = false;
            GraphicsDeviceManager.PreferredBackBufferWidth = DisplayMode.Width;
            GraphicsDeviceManager.PreferredBackBufferHeight = DisplayMode.Height;
        }

        /// <summary>
        /// Initializes the ScreenManager
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _isInitialized = true;
        }

        /// <summary>
        /// Loads content for the ScreenManager and its screens
        /// </summary>
        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Font = _content.Load<SpriteFont>("menufont");
            BlankTexture = _content.Load<Texture2D>("blank");

            // Tell each of the screens to load thier content 
            foreach (GameScreen screen in _screens)
            {
                screen.Activate();
            }
        }

        /// <summary>
        /// Unloads content for the ScreenManager's screens
        /// </summary>
        protected override void UnloadContent()
        {
            foreach (GameScreen screen in _screens)
            {
                screen.Unload();
            }
        }

        /// <summary>
        /// Updates all screens managed by the ScreenManager
        /// </summary>
        /// <param name="gameTime">An object representing time in the game</param>
        public override void Update(GameTime gameTime)
        {
            // Read in the keyboard and gamepad
            _input.Update();

            // Make a copy of the screen list, to avoid confusion if 
            // the process of updating a screen adds or removes others
            _tmpScreensList.Clear();
            _tmpScreensList.AddRange(_screens);

            bool otherScreenHasFocus = !Game.IsActive;
            bool coveredByOtherScreen = false;

            while (_tmpScreensList.Count > 0)
            {
                // Pop the topmost screen 
                GameScreen screen = _tmpScreensList[^1];
                _tmpScreensList.RemoveAt(_tmpScreensList.Count - 1);

                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == ScreenState.TransitionOn || screen.ScreenState == ScreenState.Active)
                {
                    // if this is the first active screen, let it handle input 
                    if (!otherScreenHasFocus)
                    {
                        screen.HandleInput(gameTime, _input);
                        otherScreenHasFocus = true;
                    }

                    // if this is an active non-popup, all subsequent 
                    // screens are covered 
                    if (!screen.IsPopup) coveredByOtherScreen = true;
                }
            }
        }

        /// <summary>
        /// Draws the appropriate screens managed by the SceneManager
        /// </summary>
        /// <param name="gameTime">An object representing time in the game</param>
        public override void Draw(GameTime gameTime)
        {
            foreach (GameScreen screen in _screens)
            {
                if (screen.ScreenState == ScreenState.Hidden) continue;

                screen.Draw(gameTime);
            }
        }

        /// <summary>
        /// Adds a screen to the ScreenManager
        /// </summary>
        /// <param name="screen">The screen to add</param>
        /// <param name="controllingPlayer"></param>
        public void AddScreen(GameScreen screen, PlayerIndex? controllingPlayer)
        {
            screen.ControllingPlayer = controllingPlayer;
            screen.ScreenManager = this;
            screen.IsExiting = false;

            if (screen is OptionsMenuScreen oms)
            {
                oms.ScreenManager = this;
            }

            // If we have a graphics device, tell the screen to load content
            if (_isInitialized) screen.Activate();

            _screens.Add(screen);
        }

        public void ResetScreen(GameScreen screen)
        {
            if (_screens.Contains(screen))
            {
                // Unload the current screen
                if (_isInitialized) screen.Unload();

                // Remove the screen
                _screens.Remove(screen);
                _tmpScreensList.Remove(screen);

                // Reset the screen's state
                screen.IsExiting = false;

                // Reactivate the screen
                if (_isInitialized) screen.Activate();

                // Add the screen back
                _screens.Add(screen);
            }
        }

        public void RemoveScreen(GameScreen screen)
        {
            // If we have a graphics device, tell the screen to unload its content 
            if (_isInitialized) screen.Unload();

            _screens.Remove(screen);
            _tmpScreensList.Remove(screen);
            switch (screen)
            {
                case PauseMenuScreen:
                {
                    foreach (GameScreen gs in _screens)
                    {
                        if (gs is GameplayScreen gps)
                        {
                            gps.GameStateManager.ResumeMusic();
                        }
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Exposes an array holding all the screens managed by the ScreenManager
        /// </summary>
        /// <returns>An array containing references to all current screens</returns>
        public GameScreen[] GetScreens()
        {
            return _screens.ToArray();
        }

        // Helper draws a translucent black fullscreen sprite, used for fading
        // screens in and out, and for darkening the background behind popups.
        public void FadeBackBufferToBlack(float alpha)
        {
            SpriteBatch.Begin();
            SpriteBatch.Draw(BlankTexture, GraphicsDevice.Viewport.Bounds, Color.Black * alpha);
            SpriteBatch.End();
        }

        public void Deactivate()
        {
        }

        public bool Activate()
        {
            return false;
        }
    }
}
