using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Superorganism.Core.Camera;
using Superorganism.Core.Managers;
using Superorganism.Common;
using Microsoft.Xna.Framework.Content;
using Superorganism.AI;
using Superorganism.ScreenManagement;
using Superorganism.Core.Background;
using Superorganism.Tiles;
using System.IO;
using System.Reflection.Metadata;

namespace Superorganism.Screens
{
    public class GameplayScreen : GameScreen
    {
        // Core components
        public GameStateManager GameState;
        private GameUiManager _uiManager;
        private Camera2D _camera;
        private GroundSprite _groundTexture;
        private ParallaxBackground _parallaxBackground;
        private Tilemap _tilemap;
        private Map _map;

        // Constants
        private const int GroundY = 400;
        public readonly float Zoom = 1f;
        private float _pauseAlpha;
        private ContentManager _content;

        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public override void Activate()
        {
            DecisionMaker.GameStartTime = DateTime.Now;
            _content ??= new ContentManager(ScreenManager.Game.Services, "Content");
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            _tilemap = new Tilemap("Tileset/map.txt");
            _map = Map.Load(Path.Combine(_content.RootDirectory, "Tileset/TestMapRev1.tmx"), _content);

            // Initialize camera
            _camera = new Camera2D(ScreenManager.GraphicsDevice, Zoom);

            // Initialize core managers
            GameState = new GameStateManager(
                ScreenManager.Game,
                _content,
                ScreenManager.GraphicsDevice,
                _camera,
                ScreenManager.GameAudioManager,
                _tilemap
            );

            // Initialize UI
            _uiManager = new GameUiManager(
                _content.Load<SpriteFont>("gamefont"),
                ScreenManager.SpriteBatch
            );

            // Initialize ground
            _groundTexture = new GroundSprite(ScreenManager.GraphicsDevice, GroundY, 100);
            _groundTexture.LoadContent(_content);

            _parallaxBackground = new ParallaxBackground(ScreenManager.GraphicsDevice);
            _parallaxBackground.LoadContent(_content);

            _camera.Initialize(GameState.GetPlayerPosition());
            DecisionMaker.GroundY = GroundY;
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (GameState.HandlePauseInput(input, ControllingPlayer, out PlayerIndex playerIndex))
            {
                //GameState.PauseMusic();
                ScreenManager.AddScreen(new PauseMenuScreen(), playerIndex);
                return;
            }

            if ((GameState.IsGameOver || GameState.IsGameWon) &&
                input.IsNewKeyPress(Keys.R, ControllingPlayer, out playerIndex))
            {
                GameState.Reset();
                ScreenManager.ResetScreen(this);
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            if (!IsActive) return;

            GameState.Update(gameTime);
            _camera.Update(GameState.GetPlayerPosition(), gameTime);
            UpdatePauseAlpha(gameTime, coveredByOtherScreen); // Pass coveredByOtherScreen
        }

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

            // Draw other game elements
            //_groundTexture.Draw(spriteBatch);
            //_tilemap.Draw(gameTime, spriteBatch);
            _map.Draw(spriteBatch, new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width, 
                ScreenManager.GraphicsDevice.Viewport.Height), _camera.Position);
            GameState.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            // Draw UI elements without camera transform
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone
            );

            // Draw Health bar
            int currentHealth = GameState.GetPlayerHealth();
            int maxHealth = GameState.GetPlayerMaxHealth();
            _uiManager.DrawHealthBar(currentHealth, maxHealth);
            _uiManager.DrawCropsLeft(GameState.CropsLeft);

            // Add enemy debug info
            _uiManager.DrawEnemyDebugInfo(
                GameState.GetEnemyPosition(),
                _camera.TransformMatrix,
                GameState.GetEnemyStrategy(),
                GameState.GetDistanceToPlayer(),
                GameState.GetEnemyStrategyHistory()
            );

            if (GameState.IsGameOver)
                _uiManager.DrawGameOverScreen();
            else if (GameState.IsGameWon)
                _uiManager.DrawWinScreen();

            spriteBatch.End();

            if (!(TransitionPosition > 0) && !(_pauseAlpha > 0)) return;
            float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, _pauseAlpha / 2);
            ScreenManager.FadeBackBufferToBlack(alpha);
        }

        private void UpdatePauseAlpha(GameTime gameTime, bool coveredByOtherScreen)
        {
            _pauseAlpha = coveredByOtherScreen ? 
                Math.Min(_pauseAlpha + 0.05f, 1.0f) : Math.Max(_pauseAlpha - 0.05f, 0f);
        }

        public override void Unload()
        {
            _uiManager?.Dispose();
            _parallaxBackground?.Unload();
            GameState?.Unload();
            _content?.Unload();
        }
    }
}