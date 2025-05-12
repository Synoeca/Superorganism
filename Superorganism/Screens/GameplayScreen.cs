using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Superorganism.Core.Camera;
using Superorganism.Core.Managers;
using Microsoft.Xna.Framework.Content;
using Superorganism.AI;
using Superorganism.ScreenManagement;
using Superorganism.Core.Background;
using Superorganism.Tiles;
using System.IO;
using Superorganism.Core.SaveLoadSystem;
using Superorganism.Core.Timing;

#pragma warning disable CA1416

namespace Superorganism.Screens
{
    /// <summary>
    /// Main gameplay screen that handles the primary game experience.
    /// Manages game state, rendering, input handling, and provides the core
    /// game loop for player interaction with the game world.
    /// </summary>
    public class GameplayScreen : GameScreen
    {
        // Core components
        /// <summary>
        /// Central organizer that manages all game state logic including entities, collisions, and gameplay rules.
        /// </summary>
        public GameStateOrganizer GameStateOrganizer;

        /// <summary>
        /// Handles rendering of UI elements such as health bars, stamina, debug info, and game over screens.
        /// </summary>
        private GameUiRenderer _uiRenderer;

        /// <summary>
        /// 2D camera that follows the player and controls viewport transformations.
        /// </summary>
        private Camera2D _camera;

        /// <summary>
        /// Multi-layered scrolling background that creates depth effect in the game world.
        /// </summary>
        private ParallaxBackground _parallaxBackground;

        /// <summary>
        /// Tiled map loaded from TMX file that defines the game level layout, tiles, and collision data.
        /// </summary>
        private TiledMap _map;

        /// <summary>
        /// Content manager for loading game assets specific to this screen.
        /// </summary>
        private ContentManager _content;

        // Constants
        /// <summary>
        /// Default zoom level for the camera. Value of 1f represents no zoom.
        /// </summary>
        public readonly float Zoom = 1f;

        /// <summary>
        /// Alpha value for screen fade effect when paused. Ranges from 0 (transparent) to 1 (opaque).
        /// </summary>
        private float _pauseAlpha;

        /// <summary>
        /// Determines if a screen is one of the screen types that should pause the game.
        /// Checks for specific screen classes that are known to require game pausing.
        /// </summary>
        /// <param name="screen">The screen to check</param>
        /// <returns>True if the screen is a type that should pause the game, false otherwise</returns>
        private bool ShouldPause(GameScreen screen)
        {
            return screen != this && screen is PauseMenuScreen or SaveFileMenuScreen or OptionsMenuScreen;
        }

        /// <summary>
        /// Checks if a screen has the ShouldPauseGame property and if that property is true.
        /// Used to determine if a screen should pause the game when active.
        /// </summary>
        /// <param name="screen">The screen to check</param>
        /// <returns>True if the screen has ShouldPauseGame=true, false otherwise</returns>
        private bool HasPauseProperty(GameScreen screen)
        {
            return screen != this &&
                   screen.GetType().GetProperty("ShouldPauseGame")?.GetValue(screen) is true;
        }

        /// <summary>
        /// A flag to track if we're transitioning from pause back to gameplay
        /// </summary>
        private bool _returningFromPause = false;

        /// <summary>
        /// Gets or sets the path to a save file that should be loaded when the screen starts.
        /// If null, a new game will be started instead.
        /// </summary>
        public string SaveFileToLoad { get; set; }

        /// <summary>
        /// Initializes a new instance of the GameplayScreen class.
        /// Sets up transition times for screen animations.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Activates the screen by initializing the content manager and all game components.
        /// Called when the screen becomes active in the screen manager.
        /// </summary>
        public override void Activate()
        {
            _content ??= new ContentManager(ScreenManager.Game.Services, "Content");
            InitializeComponents();
        }

        /// <summary>
        /// Initializes all gameplay components including map, camera, and game state.
        /// Loads a save file if specified, otherwise starts a new game with default values.
        /// </summary>
        private void InitializeComponents()
        {
            string mapFileName = "TestMapRev5"; // Default map
            GameStateInfo loadedState = new();

            _map = new TiledMap();
            _map = _map.Load(Path.Combine(_content.RootDirectory, ContentPaths.GetMapPath($"{mapFileName}.tmx")), _content);
            _map.MapFileName = mapFileName;
            _camera = new Camera2D(ScreenManager.GraphicsDevice, Zoom);

            if (SaveFileToLoad != null)
            {
                try
                {
                    string savePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Superorganism",
                        "Saves",
                        SaveFileToLoad);

                    if (File.Exists(savePath))
                    {
                        (loadedState, mapFileName) = GameStateLoader.LoadGameState(SaveFileToLoad, _content, _map);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load save file: {ex.Message}");
                }
            }

            TilePhysicsInspector.TileSize = _map.TileWidth;
            TilePhysicsInspector.MapWidth = _map.Width;
            TilePhysicsInspector.MapHeight = _map.Height;

            GameStateOrganizer = new GameStateOrganizer(
                ScreenManager.Game,
                _content,
                ScreenManager.GraphicsDevice,
                _camera,
                ScreenManager.GameAudioManager,
                _map,
                loadedState
            );

            GameState.Initialize(GameStateOrganizer);
            GameState.CurrentMapName = GameState.CurrentMap.MapFileName;

            // Initialize UI and other components
            _uiRenderer = new GameUiRenderer(
                _content.Load<SpriteFont>("gamefont"),
                ScreenManager.SpriteBatch
            );

            _parallaxBackground = new ParallaxBackground();
            _parallaxBackground.LoadContent(_content);

            _camera.Initialize(GameStateOrganizer.GetPlayerPosition(), ScreenManager);
            ScreenManager.GameplayScreenCamera2D = _camera;
        }

        /// <summary>
        /// Handles user input including pause, debug toggles, and game reset commands.
        /// </summary>
        /// <param name="gameTime">Timing information for the current frame.</param>
        /// <param name="input">The input state containing keyboard, mouse, and gamepad data.</param>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            // Handle inventory toggle first
            if (input.IsNewKeyPress(Keys.I, ControllingPlayer, out PlayerIndex playerIndex))
            {
                ScreenManager.AddScreen(new InventoryScreen(), playerIndex);
                return;
            }

            // Then handle pause menu
            if (GameStateOrganizer.HandlePauseInput(input, ControllingPlayer, out playerIndex))
            {
                // Pause the gameplay timer when entering pause menu
                GameTimer.Pause();
                ScreenManager.AddScreen(new PauseMenuScreen(), playerIndex);
                return;
            }

            // Debug visualization toggles
            if (input.IsNewKeyPress(Keys.F1, ControllingPlayer, out _))
                _uiRenderer.ToggleCollisionBounds();

            if (input.IsNewKeyPress(Keys.F2, ControllingPlayer, out _))
                _uiRenderer.ToggleEntityInfo();

            if (input.IsNewKeyPress(Keys.F3, ControllingPlayer, out _))
                _uiRenderer.ToggleMousePosition();

            if ((GameStateOrganizer.IsGameOver || GameStateOrganizer.IsGameWon) &&
                input.IsNewKeyPress(Keys.R, ControllingPlayer, out playerIndex))
            {
                GameStateOrganizer.Reset();
                // Reset the timer when restarting the game
                GameTimer.Reset();
                ScreenManager.ResetScreen(this);
            }
        }

        /// <summary>
        /// Updates the gameplay screen each frame. Handles game state updates, camera movement,
        /// and pause screen effects.
        /// </summary>
        /// <param name="gameTime">Timing information for the current frame.</param>
        /// <param name="otherScreenHasFocus">Whether another screen currently has input focus.</param>
        /// <param name="coveredByOtherScreen">Whether this screen is covered by another screen.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // Call the base update method
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Check if we're covered by a screen that should pause the game
            bool shouldPause = false;
            bool wasPreviouslyPaused = _pauseAlpha > 0.1f; // Check if we were paused before

            if (coveredByOtherScreen || otherScreenHasFocus)
            {
                GameScreen[] screens = ScreenManager.GetScreens();

                foreach (GameScreen screen in screens)
                {
                    // Skip inactive screens
                    if (screen.ScreenState == ScreenState.Hidden)
                        continue;

                    // Check if the screen is a type that should pause the game
                    if (ShouldPause(screen))
                    {
                        shouldPause = true;
                        break;
                    }

                    // Check if screen has the ShouldPauseGame property
                    if (HasPauseProperty(screen))
                    {
                        shouldPause = true;
                        break;
                    }
                }
            }

            // Check if we were paused but aren't anymore - this means we're transitioning back to gameplay
            if (wasPreviouslyPaused && !shouldPause)
            {
                _returningFromPause = true;
            }

            // If we should pause, pause the timer and return
            if (shouldPause)
            {
                GameTimer.Pause();
                _returningFromPause = false; // Reset the transition flag
            }
            else
            {
                // Continue with normal gameplay updates
                GameTimer.Resume();
                GameTimer.Update(gameTime);
                GameStateOrganizer.Update(gameTime);
                _camera.Update(GameStateOrganizer.GetPlayerPosition(), gameTime);
            }

            // Always update the pause alpha, whether we're pausing or unpausing
            UpdatePauseAlpha(shouldPause);

            // If we've fully transitioned back, reset the flag
            if (_returningFromPause && _pauseAlpha <= 0)
            {
                _returningFromPause = false;
            }
        }


        /// <summary>
        /// Renders the game world, UI elements, and transition effects to the screen.
        /// Draws in layers: background, map, entities, UI, and overlay effects.
        /// </summary>
        /// <param name="gameTime">Timing information for the current frame.</param>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw game world
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin(
                SpriteSortMode.BackToFront,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                _camera.TransformMatrix
            );

            // Draw parallax background first
            _parallaxBackground.Draw(spriteBatch, _camera.Position);

            _map.Draw(
                spriteBatch,
                new Rectangle(0, 0,
                    ScreenManager.GraphicsDevice.Viewport.Width,
                    ScreenManager.GraphicsDevice.Viewport.Height
                ),
                Vector2.Zero  // Use Vector2.Zero since camera transform is handled by SpriteBatch
            );

            GameStateOrganizer.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            // Draw UI elements without camera transform
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone
            );

            _uiRenderer.DrawPlayerStatus(GameStateOrganizer.GetPlayerHealth(), GameStateOrganizer.GetPlayerMaxHealth(),
                GameStateOrganizer.GetPlayerStamina(), GameStateOrganizer.GetPlayerMaxStamina(),
                GameStateOrganizer.GetPlayerHunger(), GameStateOrganizer.GetPlayerMaxHunger());
            _uiRenderer.DrawCropsLeft(GameStateOrganizer.CropsLeft);
            _uiRenderer.DrawDebugInfo(gameTime, DecisionMaker.Entities, _camera.TransformMatrix, GameStateOrganizer.GetPlayerPosition());

            // Draw win/lose screen if game is over
            if (GameStateOrganizer.IsGameOver)
            {
                _uiRenderer.DrawGameOverScreen();
            }
            else if (GameStateOrganizer.IsGameWon)
            {
                _uiRenderer.DrawWinScreen();
            }

            spriteBatch.End();

            if (TransitionPosition > 0 || _pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, _pauseAlpha / 2);
                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }

        /// <summary>
        /// Updates the pause fade effect alpha value based on the game's pause state.
        /// Creates smooth transitions between gameplay and paused states with different
        /// fade speeds for a more natural visual effect.
        /// </summary>
        /// <param name="isPaused">Whether the game is currently paused or transitioning to paused</param>
        private void UpdatePauseAlpha(bool isPaused)
        {
            // Adjust the speed for fading in and out
            float fadeInSpeed = 0.05f;  // Speed when going from gameplay to pause
            float fadeOutSpeed = 0.05f;  // Slower speed when returning from pause to gameplay

            if (isPaused)
            {
                // Fade to dark when pausing
                _pauseAlpha = Math.Min(_pauseAlpha + fadeInSpeed, 1.0f);
            }
            else
            {
                // Fade to transparent when unpausing - use slower speed if we're returning from pause
                float speed = _returningFromPause ? fadeOutSpeed : fadeInSpeed;
                _pauseAlpha = Math.Max(_pauseAlpha - speed, 0f);
            }
        }

        /// <summary>
        /// Unloads all resources when the screen is no longer needed.
        /// Called when the screen is removed from the screen manager.
        /// </summary>
        public override void Unload()
        {
            // Reset the timer when the screen is unloaded
            GameTimer.Reset();

            _uiRenderer?.Dispose();
            _parallaxBackground?.Unload();
            GameStateOrganizer?.Unload();
            _content?.Unload();
        }
    }
}