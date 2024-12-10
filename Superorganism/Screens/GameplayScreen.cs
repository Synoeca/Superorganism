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
            //_tilemap = new Tilemap("Tileset/map.txt");
            //_map = Map.Load(Path.Combine(_content.RootDirectory, "Tileset/TestMapRev1.tmx"), _content);

            _tilemap = new Tilemap(ContentPaths.GetMapPath("map.txt"));
            _map = Map.Load(Path.Combine(_content.RootDirectory, ContentPaths.GetMapPath("TestMapRev1.tmx")), _content);

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

            // In GameplayScreen.Draw
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

            // Draw Health bar
            int currentHealth = GameStateManager.GetPlayerHealth();
            int maxHealth = GameStateManager.GetPlayerMaxHealth();
            _uiManager.DrawHealthBar(currentHealth, maxHealth);
            _uiManager.DrawCropsLeft(GameStateManager.CropsLeft);


            // Draw entity collision boundaries
            foreach (Entity entity in DecisionMaker.Entities)
            {
                if (entity.CollisionBounding != null)
                {
                    switch (entity)
                    {
                        case Crop crop:
                            if (!crop.Collected)
                            {
                                _uiManager.DrawCollisionBounds(crop, crop.CollisionBounding, _camera.TransformMatrix);
                            }

                            break;
                        case Fly fly:
                            if (!fly.Destroyed)
                            {
                                _uiManager.DrawCollisionBounds(fly, fly.CollisionBounding, _camera.TransformMatrix);
                            }
                            break;
                        case Ant ant:
                            _uiManager.DrawCollisionBounds(ant, ant.CollisionBounding, _camera.TransformMatrix);
                            break;
                        case AntEnemy antEnemy:
                            _uiManager.DrawCollisionBounds(antEnemy, antEnemy.CollisionBounding, _camera.TransformMatrix);
                            break;
                        default:
                            _uiManager.DrawCollisionBounds(entity, entity.CollisionBounding, _camera.TransformMatrix);
                            break;
                    }
                }
            }

            foreach (Entity entity in DecisionMaker.Entities)
            {
                switch (entity)
                {
                    case Crop crop:
                        if (!crop.Collected)
                        {
                            _uiManager.DrawDebugInfo(
                                crop.Position,
                                _camera.TransformMatrix,
                                GameStateManager.GetDistanceToPlayer(crop),
                                crop.CollisionBounding
                            );
                        }

                        break;
                    case Fly { Destroyed: true }:
                        continue;
                    case Fly fly:
                        _uiManager.DrawDebugInfo(
                            fly.Position,
                            _camera.TransformMatrix,
                            fly.Strategy,
                            GameStateManager.GetDistanceToPlayer(fly),
                            fly.StrategyHistory,
                            fly.CollisionBounding
                        );
                        break;
                    case AntEnemy antEnemy:
                        _uiManager.DrawDebugInfo(
                            antEnemy.Position,
                            _camera.TransformMatrix,
                            antEnemy.Strategy,
                            GameStateManager.GetDistanceToPlayer(antEnemy),
                            antEnemy.StrategyHistory,
                            antEnemy.CollisionBounding
                        );
                        break;
                    case Ant ant:
                        _uiManager.DrawDebugInfo(
                            ant.Position,
                            _camera.TransformMatrix,
                            GameStateManager.GetDistanceToPlayer(ant),
                            ant.CollisionBounding
                        );
                        break;
                }
            }

            _uiManager.DrawMousePositionDebug(gameTime, _camera.TransformMatrix);

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