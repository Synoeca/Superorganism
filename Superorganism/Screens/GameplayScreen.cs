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
                        (loadedState, mapFileName) = GameStateLoader.LoadGameState(SaveFileToLoad);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load save file: {ex.Message}");
                }
            }

            _map = new TiledMap();
            _map = _map.Load(Path.Combine(_content.RootDirectory, ContentPaths.GetMapPath($"{mapFileName}.tmx")), _content);
            _map.MapFileName = mapFileName;
            _camera = new Camera2D(ScreenManager.GraphicsDevice, Zoom);

            MapHelper.TileSize = _map.TileWidth;
            MapHelper.MapWidth = _map.Width;
            MapHelper.MapHeight = _map.Height;

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
            if (GameStateOrganizer.HandlePauseInput(input, ControllingPlayer, out PlayerIndex playerIndex))
            {
                //GameStateOrganizer.PauseMusic();
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
            base.Update(gameTime, otherScreenHasFocus, false);

            if (!IsActive) return;

            GameStateOrganizer.Update(gameTime);
            _camera.Update(GameStateOrganizer.GetPlayerPosition(), gameTime);
            UpdatePauseAlpha(coveredByOtherScreen); // Pass coveredByOtherScreen
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

            if (!(TransitionPosition > 0) && !(_pauseAlpha > 0)) return;
            float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, _pauseAlpha / 2);
            ScreenManager.FadeBackBufferToBlack(alpha);
        }

        /// <summary>
        /// Updates the pause fade effect based on whether the screen is covered by another screen.
        /// Creates a smooth transition when the pause menu appears or disappears.
        /// </summary>
        /// <param name="coveredByOtherScreen">Whether this screen is currently covered by another screen (e.g., pause menu).</param>
        private void UpdatePauseAlpha(bool coveredByOtherScreen)
        {
            _pauseAlpha = coveredByOtherScreen ?
                Math.Min(_pauseAlpha + 0.05f, 1.0f) : Math.Max(_pauseAlpha - 0.05f, 0f);
        }

        /// <summary>
        /// Unloads all resources when the screen is no longer needed.
        /// Called when the screen is removed from the screen manager.
        /// </summary>
        public override void Unload()
        {
            _uiRenderer?.Dispose();
            _parallaxBackground?.Unload();
            GameStateOrganizer?.Unload();
            _content?.Unload();
        }
    }
}