﻿using System;
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
using Superorganism.Entities;

namespace Superorganism.Screens
{
    public class GameplayScreen : GameScreen
    {
        // Core components
        public GameStateManager GameStateManager;
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
            GameStateManager = new GameStateManager(
                ScreenManager.Game,
                _content,
                ScreenManager.GraphicsDevice,
                _camera,
                ScreenManager.GameAudioManager,
                _tilemap,
                _map
            );

            GameState.Initialize(GameStateManager);


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

            _camera.Initialize(GameStateManager.GetPlayerPosition());
            DecisionMaker.GroundY = GroundY;
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (GameStateManager.HandlePauseInput(input, ControllingPlayer, out PlayerIndex playerIndex))
            {
                //GameStateManager.PauseMusic();
                ScreenManager.AddScreen(new PauseMenuScreen(), playerIndex);
                return;
            }

            if ((GameStateManager.IsGameOver || GameStateManager.IsGameWon) &&
                input.IsNewKeyPress(Keys.R, ControllingPlayer, out playerIndex))
            {
                GameStateManager.Reset();
                ScreenManager.ResetScreen(this);
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            if (!IsActive) return;

            GameStateManager.Update(gameTime);
            _camera.Update(GameStateManager.GetPlayerPosition(), gameTime);
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

            // In GameplayScreen.Draw
            _map.Draw(
                spriteBatch,
                new Rectangle(
                    0,
                    0,
                    ScreenManager.GraphicsDevice.Viewport.Width,
                    ScreenManager.GraphicsDevice.Viewport.Height
                ),
                Vector2.Zero  // Use Vector2.Zero since camera transform is handled by SpriteBatch
            );

            GameStateManager.Draw(gameTime, spriteBatch);

            // Draw entity collision boundaries
            foreach (Entity entity in DecisionMaker.Entities)
            {
                if (entity.CollisionBounding != null)
                {
                    _uiManager.DrawCollisionBounds(entity.CollisionBounding, _camera.TransformMatrix);
                }
            }

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
            int currentHealth = GameStateManager.GetPlayerHealth();
            int maxHealth = GameStateManager.GetPlayerMaxHealth();
            _uiManager.DrawHealthBar(currentHealth, maxHealth);
            _uiManager.DrawCropsLeft(GameStateManager.CropsLeft);

            // Add enemy debug info
            _uiManager.DrawEnemyDebugInfo(
                GameStateManager.GetEnemyPosition(),
                _camera.TransformMatrix,
                GameStateManager.GetEnemyStrategy(),
                GameStateManager.GetDistanceToPlayer(),
                GameStateManager.GetEnemyStrategyHistory()
            );

            if (GameStateManager.IsGameOver)
                _uiManager.DrawGameOverScreen();
            else if (GameStateManager.IsGameWon)
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
            GameStateManager?.Unload();
            _content?.Unload();
        }
    }
}