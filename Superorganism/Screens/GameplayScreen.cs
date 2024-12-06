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
        public GameStateManager GameState;
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
            // Initialize camera
            _camera = new Camera2D(ScreenManager.GraphicsDevice, _zoom);

            // Initialize core managers
            GameState = new GameStateManager(
                ScreenManager.Game,
                _content,
                ScreenManager.GraphicsDevice,
                _camera
            );

            GameState.InitializeAudio(
                OptionsMenuScreen.SoundEffectVolume,
                OptionsMenuScreen.BackgroundMusicVolume
            );

            // Initialize UI
            _uiManager = new GameUIManager(
                _content.Load<SpriteFont>("gamefont"),
                ScreenManager.SpriteBatch
            );

            // Initialize ground
            _groundTexture = new GroundSprite(ScreenManager.GraphicsDevice, _groundY, 100);
            _groundTexture.LoadContent(_content);

            _camera.Initialize(GameState.GetPlayerPosition());
            DecisionMaker.GroundY = _groundY;
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (GameState.HandlePauseInput(input, ControllingPlayer, out PlayerIndex playerIndex))
            {
                GameState.PauseAudio();
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

            _groundTexture.Draw(spriteBatch);
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
            GameState?.Unload();
            _content?.Unload();
        }
    }
}