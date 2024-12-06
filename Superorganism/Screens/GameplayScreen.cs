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

namespace Superorganism.Screens
{
    public class GameplayScreen : GameScreen
    {
        // Core components
        private GameStateManager _gameState;
        private GameUIManager _uiManager;
        private Camera2D _camera;
        private GroundSprite _groundTexture;

        // Constants
        private readonly int _groundY = 400;
        private readonly float _zoom = 1f;
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
            // Initialize core managers
            _gameState = new GameStateManager(
                ScreenManager.Game,
                _content,
                ScreenManager.GraphicsDevice
            );

            _gameState.InitializeAudio(
                OptionsMenuScreen.SoundEffectVolume,
                OptionsMenuScreen.BackgroundMusicVolume
            );

            // Initialize UI
            _uiManager = new GameUIManager(
                _content.Load<SpriteFont>("gamefont"),
                ScreenManager.SpriteBatch
            );

            // Initialize camera
            _camera = new Camera2D(ScreenManager.GraphicsDevice, _zoom);

            // Initialize ground
            _groundTexture = new GroundSprite(ScreenManager.GraphicsDevice, _groundY, 100);
            _groundTexture.LoadContent(_content);

            DecisionMaker.GroundY = _groundY;
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (_gameState.HandlePauseInput(input, ControllingPlayer, out PlayerIndex playerIndex))
            {
                _gameState.PauseAudio();
                ScreenManager.AddScreen(new PauseMenuScreen(), playerIndex);
                return;
            }

            if ((_gameState.IsGameOver || _gameState.IsGameWon) &&
                input.IsNewKeyPress(Keys.R, ControllingPlayer, out playerIndex))
            {
                _gameState.Reset();
                ScreenManager.ResetScreen(this);
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            if (!IsActive) return;

            _gameState.Update(gameTime);
            _camera.Update(_gameState.GetPlayerPosition());
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw game world
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                _camera.TransformMatrix
            );

            _groundTexture.Draw(spriteBatch);
            _gameState.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            // Draw UI elements without camera transform
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone
            );

            // Get health values
            int currentHealth = _gameState.GetPlayerHealth();
            int maxHealth = _gameState.GetPlayerMaxHealth();

            // Draw health bar first
            _uiManager.DrawHealthBar(currentHealth, maxHealth);

            _uiManager.DrawCropsLeft(_gameState.CropsLeft);

            // Add enemy debug info
            _uiManager.DrawEnemyDebugInfo(
                _gameState.GetEnemyPosition(),
                _camera.TransformMatrix,
                _gameState.GetEnemyStrategy(),
                _gameState.GetDistanceToPlayer(),
                _gameState.GetEnemyStrategyHistory()
            );

            if (_gameState.IsGameOver)
                _uiManager.DrawGameOverScreen();
            else if (_gameState.IsGameWon)
                _uiManager.DrawWinScreen();

            spriteBatch.End();

            if (TransitionPosition > 0 || _pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, _pauseAlpha / 2);
                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }

        public override void Unload()
        {
            _uiManager?.Dispose();
            _gameState?.Unload();
            _content?.Unload();
        }
    }
}