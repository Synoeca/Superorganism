﻿using System;
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
    public class GameplayScreen : GameScreen
    {
        // Core components
        public GameStateManager GameStateManager;
        private GameUiManager _uiManager;
        private Camera2D _camera;
        private ParallaxBackground _parallaxBackground;
        private TiledMap _map;
        private ContentManager _content;

        // Constants
        public readonly float Zoom = 1f;
        private float _pauseAlpha;

        public string SaveFileToLoad { get; set; }

        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public override void Activate()
        {
            _content ??= new ContentManager(ScreenManager.Game.Services, "Content");
            InitializeComponents();
        }

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

            GameStateManager = new GameStateManager(
                ScreenManager.Game,
                _content,
                ScreenManager.GraphicsDevice,
                _camera,
                ScreenManager.GameAudioManager,
                _map,
                loadedState
            );

            GameState.Initialize(GameStateManager);
            GameState.CurrentMapName = GameState.CurrentMap.MapFileName;
            //GameState.CurrentMapName = GameState.CurrentMap



            // Initialize UI and other components
            _uiManager = new GameUiManager(
                _content.Load<SpriteFont>("gamefont"),
                ScreenManager.SpriteBatch
            );

            _parallaxBackground = new ParallaxBackground();
            _parallaxBackground.LoadContent(_content);

            _camera.Initialize(GameStateManager.GetPlayerPosition(), ScreenManager);
            ScreenManager.GameplayScreenCamera2D = _camera;
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (GameStateManager.HandlePauseInput(input, ControllingPlayer, out PlayerIndex playerIndex))
            {
                //GameStateManager.PauseMusic();
                ScreenManager.AddScreen(new PauseMenuScreen(), playerIndex);
                return;
            }

            // Debug visualization toggles
            if (input.IsNewKeyPress(Keys.F1, ControllingPlayer, out _))
                _uiManager.ToggleCollisionBounds();

            if (input.IsNewKeyPress(Keys.F2, ControllingPlayer, out _))
                _uiManager.ToggleEntityInfo();

            if (input.IsNewKeyPress(Keys.F3, ControllingPlayer, out _))
                _uiManager.ToggleMousePosition();

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
            UpdatePauseAlpha(coveredByOtherScreen); // Pass coveredByOtherScreen
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

            _map.Draw(
                spriteBatch,
                new Rectangle(0, 0,
                    ScreenManager.GraphicsDevice.Viewport.Width,
                    ScreenManager.GraphicsDevice.Viewport.Height
                ),
                Vector2.Zero  // Use Vector2.Zero since camera transform is handled by SpriteBatch
            );

            GameStateManager.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            // Draw UI elements without camera transform
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone
            );

            _uiManager.DrawHealthBar(GameStateManager.GetPlayerHealth(), GameStateManager.GetPlayerMaxHealth());
            _uiManager.DrawCropsLeft(GameStateManager.CropsLeft);
            _uiManager.DrawDebugInfo(gameTime, DecisionMaker.Entities, _camera.TransformMatrix, GameStateManager.GetPlayerPosition());

            // Draw win/lose screen if game is over
            if (GameStateManager.IsGameOver)
            {
                _uiManager.DrawGameOverScreen();
            }
            else if (GameStateManager.IsGameWon)
            {
                _uiManager.DrawWinScreen();
            }

            spriteBatch.End();

            if (!(TransitionPosition > 0) && !(_pauseAlpha > 0)) return;
            float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, _pauseAlpha / 2);
            ScreenManager.FadeBackBufferToBlack(alpha);
        }

        private void UpdatePauseAlpha(bool coveredByOtherScreen)
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