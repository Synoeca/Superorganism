using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Superorganism.Collisions;
using Superorganism.Entities;
using Superorganism.Enums;
using Superorganism.Particle;
using Superorganism.StateManagement;

namespace Superorganism.Screens;

public class GameplayScreen : GameScreen
{
    private readonly int _cropY = 383;

    private readonly int _groundY = 400;

    private readonly InputAction _pauseAction;

    private Ant _ant;
    private AntEnemy _antEnemy;
    private Song _backgroundMusic;

    private Matrix _cameraMatrix;
    private Vector2 _cameraPosition;
    private ContentManager _content;

    private SoundEffect _cropPickup;

    private double _enemyCollisionTimer = 0; // Add this with other timer variables
    private const double EnemyCollisionInterval = 0.2; // 0.2 seconds interval
    private double _damageTimer;
    private double _elapsedTime;
    private ExplosionParticleSystem _explosions;
    private SoundEffect _fliesDestroy;
    private SpriteFont _gameFont;
    private GroundSprite _groundTexture;

    private bool _isGameOver;
    private bool _isGameWon;

    private Crop[] _crops;
    private Fly[] _flies;
    private int _cropsLeft;

    private float _pauseAlpha;
    private readonly float _zoom = 1f;

    public GameplayScreen()
    {
        TransitionOnTime = TimeSpan.FromSeconds(1.5);
        TransitionOffTime = TimeSpan.FromSeconds(0.5);

        _pauseAction = new InputAction(
            [Buttons.Start, Buttons.Back],
            [Keys.Back, Keys.Escape], true);
    }

    public List<Entity> Entities { get; set; } = [];

    public override void Activate()
    {
        if (_content == null)
            _content = new ContentManager(ScreenManager.Game.Services, "Content");

        DecisionMaker.GroundY = _groundY;
        _gameFont = _content.Load<SpriteFont>("gamefont");
        _groundTexture = new GroundSprite(ScreenManager.GraphicsDevice, _groundY, 100);

        _ant = new Ant()
        {
            Position = new Vector2(200, 200),
            IsControlled = true
        };

        _antEnemy = new AntEnemy
        {
            Position = new Vector2(500, 200)
        };

        _cameraPosition = _ant.Position;
        DecisionMaker.Entities.Add(_ant);

        _groundTexture.LoadContent(_content);
        _ant.LoadContent(_content, "ant-side_Rev2", 3, 1, new BoundingRectangle(), 0.25f);
        _ant.LoadSound(_content);
        _antEnemy.LoadContent(_content, "antEnemy-side_Rev3", 3, 1, new BoundingRectangle(), 0.3f);

        InitializeGame();

        foreach (Crop crop in _crops)
        {
            crop.LoadContent(_content, "crops", 8, 1, new BoundingCircle(), 1.0f);
        }

        foreach (Fly fly in _flies)
        {
            fly.LoadContent(_content, "flies", 4, 4, new BoundingCircle(), 1.0f);
        }

        // Load sound effects and music
        _cropPickup = _content.Load<SoundEffect>("Pickup_Coin4");
        _fliesDestroy = _content.Load<SoundEffect>("damaged");
        _backgroundMusic = _content.Load<Song>("MaxBrhon_Cyberpunk");
        // When playing sound effects
        _cropPickup.Play();
        _fliesDestroy.Play();

        // Setup background music with volume from OptionsMenuScreen
        SoundEffect.MasterVolume = OptionsMenuScreen.SoundEffectVolume;
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = OptionsMenuScreen.BackgroundMusicVolume; // Use the volume setting from options
        MediaPlayer.Play(_backgroundMusic);
    }

    private void InitializeGame()
    {
        Random rand = new();
        _explosions = new ExplosionParticleSystem(ScreenManager.Game, 20);
        ScreenManager.Game.Components.Add(_explosions);


        _crops = new Crop[12];
        for (int i = 0; i < _crops.Length; i++)
        {
            _crops[i] = new Crop(new Vector2(
                (float)rand.NextDouble() * ScreenManager.GraphicsDevice.Viewport.Width, _cropY));
        }

        _cropsLeft = _crops.Length;

        int numberOfFlies = rand.Next(15, 21);
        _flies = new Fly[numberOfFlies];
        for (int i = 0; i < numberOfFlies; i++)
        {
            float xPos = rand.Next(0, 800);
            float yPos = rand.Next(0, 600);
            Direction randomDirection = (Direction)rand.Next(0, 4);
            _flies[i] = new Fly
            {
                Position = new Vector2(xPos, yPos),
                Direction = randomDirection
            };
        }

        _isGameOver = false;
        _isGameWon = false;
        _elapsedTime = 0;
        _damageTimer = 0;
    }

    public void ResetGame()
    {
        // Reset any internal state that needs resetting
        _ant.Position = new Vector2(200, 200);
        _antEnemy.Position = new Vector2(500, 200);
        _cameraPosition = _ant.Position;

        DecisionMaker.Entities.Clear();
        //DecisionMaker.Entities.Add(_ant);

        // Use the ScreenManager to reset this screen
        ScreenManager.ResetScreen(this);
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    public override void Unload()
    {
        _content.Unload();
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, false);

        if (!IsActive || _isGameOver || _isGameWon) return;

        _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
        _enemyCollisionTimer -= gameTime.ElapsedGameTime.TotalSeconds; // Update collision timer

        _ant.Update(gameTime);
        _antEnemy.Update(gameTime);
        _ant.Color = Color.White;

        foreach (Crop crop in _crops)
        {
            if (crop.Collected) continue;
            crop.Update(gameTime);
            if (crop.CollisionBounding.CollidesWith(_ant.CollisionBounding))
            {
                _ant.Color = Color.Gold;
                crop.Collected = true;
                _cropsLeft--;
                _cropPickup.Play();
            }
        }

        if (_cropsLeft <= 0) _isGameWon = true;

        foreach (Fly fly in _flies)
        {
            if (fly.Destroyed) continue;
            fly.Update(gameTime);
            if (fly.CollisionBounding.CollidesWith(_ant.CollisionBounding))
            {
                _ant.Color = Color.Gray;
                fly.Destroyed = true;
                _explosions.PlaceExplosion(fly.Position);
                _fliesDestroy.Play();
                _ant.HitPoints = Math.Max(0, _ant.HitPoints - 1);
            }
        }

        // Updated enemy collision check with interval
        if (_antEnemy.CollisionBounding.CollidesWith(_ant.CollisionBounding))
        {
            if (_enemyCollisionTimer <= 0)
            {
                _ant.HitPoints -= 10;
                _fliesDestroy.Play();
                _ant.Color = Color.Gray;
                _antEnemy.Color = Color.Gray;
                _enemyCollisionTimer = EnemyCollisionInterval;
            }
        }

        if (_ant.HitPoints <= 0) _isGameOver = true;
        _cameraPosition = _ant.Position;
    }


    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (_pauseAction.Occurred(input, ControllingPlayer, out _))
        {
            ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            return;
        }

        if (!_isGameOver && !_isGameWon) return;
        if (input.IsNewKeyPress(Keys.R, ControllingPlayer, out _))
            //ScreenManager.AddScreen(new GameplayScreen(), ControllingPlayer);
            ResetGame();
    }

    private Texture2D CreateTexture(GraphicsDevice graphicsDevice, Color color)
    {
        Texture2D texture = new(graphicsDevice, 1, 1);
        texture.SetData([color]);
        return texture;
    }

    private void DrawHealthBar(SpriteBatch spriteBatch)
    {
        int barWidth = 200;
        int barHeight = 20;
        int barX = 20;
        int barY = 20;

        Texture2D grayTexture = CreateTexture(spriteBatch.GraphicsDevice, Color.Gray);
        Texture2D redTexture = CreateTexture(spriteBatch.GraphicsDevice, Color.Red);

        spriteBatch.Draw(grayTexture, new Rectangle(barX, barY, barWidth, barHeight), Color.White);

        float healthPercentage = (float)_ant.HitPoints / _ant.MaxHitPoint;
        spriteBatch.Draw(redTexture, new Rectangle(barX, barY, (int)(barWidth * healthPercentage), barHeight),
            Color.White);
    }

    private void DrawCropsLeft(SpriteBatch spriteBatch)
    {
        string cropsLeftText = $"Crops Left: {_cropsLeft}";
        Vector2 textSize = _gameFont.MeasureString(cropsLeftText);
        Vector2 textPosition = new(
            ScreenManager.GraphicsDevice.Viewport.Width - textSize.X - 20,
            20
        );

        Vector2 shadowOffset = new(2, 2);
        spriteBatch.DrawString(_gameFont, cropsLeftText, textPosition + shadowOffset,
            Color.Black * 0.5f);

        spriteBatch.DrawString(_gameFont, cropsLeftText, textPosition, Color.White);
    }


    private void UpdateCameraMatrix()
    {
        _cameraMatrix = Matrix.CreateTranslation(new Vector3(
                            -_cameraPosition.X + ScreenManager.GraphicsDevice.Viewport.Width / 2.0f,
                            -_cameraPosition.Y + ScreenManager.GraphicsDevice.Viewport.Height / 2.0f,
                            0)) *
                        Matrix.CreateScale(_zoom);
    }

    public override void Draw(GameTime gameTime)
    {
        ScreenManager.GraphicsDevice.Clear(Color.CornflowerBlue);
        UpdateCameraMatrix();
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

        // Draw game world elements with camera transform
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            _cameraMatrix
        );

        // Draw all game world elements
        _groundTexture.Draw(spriteBatch);
        foreach (Crop crop in _crops) crop.Draw(gameTime, spriteBatch);
        foreach (Fly fly in _flies) fly.Draw(gameTime, spriteBatch);
        _ant.Draw(gameTime, spriteBatch);
        _antEnemy.Draw(gameTime, spriteBatch);

        spriteBatch.End();

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone
        );

        DrawHealthBar(spriteBatch);
        DrawCropsLeft(spriteBatch);

        if (_antEnemy != null)
        {
            // Transform AntEnemy's world position into screen position
            Vector2 enemyScreenPosition = Vector2.Transform(
                _antEnemy.Position, // World position of the AntEnemy
                _cameraMatrix // Camera transformation matrix
            );

            // Offset to avoid overlapping the AntEnemy sprite
            Vector2 textOffset = new Vector2(0, -40);
            Vector2 textPosition = enemyScreenPosition + textOffset;

            // Text scaling factor for reduced size
            const float textScale = 0.4f;
            Vector2 currentOffset = Vector2.Zero;

            // Helper function to draw text with shadow
            void DrawTextWithShadow(string text, Vector2 position)
            {
                spriteBatch.DrawString(
                    _gameFont,
                    text,
                    position + new Vector2(2, 2),
                    Color.Black * 0.5f,
                    0,
                    Vector2.Zero,
                    textScale,
                    SpriteEffects.None,
                    0
                );
                spriteBatch.DrawString(
                    _gameFont,
                    text,
                    position,
                    Color.White,
                    0,
                    Vector2.Zero,
                    textScale,
                    SpriteEffects.None,
                    0
                );
            }

            // Draw current strategy
            string currentStrategyText = $"Current: {_antEnemy.Strategy}";
            DrawTextWithShadow(currentStrategyText, textPosition);
            currentOffset.Y += _gameFont.MeasureString(currentStrategyText).Y * textScale;

            // Draw position
            string positionText =
                $"Position: {Math.Round(_antEnemy.Position.X, 1):0.0}, {Math.Round(_antEnemy.Position.Y, 1):0.0}";
            DrawTextWithShadow(positionText, textPosition + currentOffset);
            currentOffset.Y += _gameFont.MeasureString(positionText).Y * textScale;

            // Draw distance
            float distance = Vector2.Distance(_ant.Position, _antEnemy.Position);
            string distanceText = $"Distance: {Math.Round(distance, 1):0.0}";
            DrawTextWithShadow(distanceText, textPosition + currentOffset);
            currentOffset.Y += _gameFont.MeasureString(distanceText).Y * textScale;

            // Draw history header
            string historyHeader = "History:";
            DrawTextWithShadow(historyHeader, textPosition + currentOffset);
            currentOffset.Y += _gameFont.MeasureString(historyHeader).Y * textScale;

            // Draw strategy history (last 3 entries)
            List<(Strategy Strategy, double StartTime, double LastActionTime)> recentHistory = _antEnemy.StrategyHistory.Skip(Math.Max(0, _antEnemy.StrategyHistory.Count - 3))
                .ToList();
            foreach ((Strategy strategy, double startTime, double lastActionTime) in recentHistory)
            {
                string historyText = $"- {strategy} Start: {Math.Round(startTime, 1):0.0}s Last: {Math.Round(lastActionTime, 1):0.0}s";
                DrawTextWithShadow(historyText, textPosition + currentOffset);
                currentOffset.Y += _gameFont.MeasureString(historyText).Y * textScale;
            }



            Vector2 shadowOffset = new(2, 2);

            if (_isGameOver)
            {
                string message = "You Lose";
                Vector2 textSize = _gameFont.MeasureString(message);
                textPosition = new(
                    (ScreenManager.GraphicsDevice.Viewport.Width - textSize.X) / 2,
                    (ScreenManager.GraphicsDevice.Viewport.Height - textSize.Y) / 2
                );


                spriteBatch.DrawString(_gameFont, message, textPosition + shadowOffset, Color.Black * 0.5f);


                spriteBatch.DrawString(_gameFont, message, textPosition, Color.Red);


                const string restartMessage = "Press R to Restart";
                Vector2 restartTextSize = _gameFont.MeasureString(restartMessage);
                Vector2 restartTextPosition = new(
                    (ScreenManager.GraphicsDevice.Viewport.Width - restartTextSize.X) / 2,
                    textPosition.Y + textSize.Y + 20
                );


                spriteBatch.DrawString(_gameFont, restartMessage, restartTextPosition + shadowOffset,
                    Color.Black * 0.5f);


                spriteBatch.DrawString(_gameFont, restartMessage, restartTextPosition, Color.White);
            }

            if (_isGameWon)
            {
                string message = "You Win";
                Vector2 textSize = _gameFont.MeasureString(message);
                textPosition = new(
                    (ScreenManager.GraphicsDevice.Viewport.Width - textSize.X) / 2,
                    (ScreenManager.GraphicsDevice.Viewport.Height - textSize.Y) / 2
                );

                spriteBatch.DrawString(_gameFont, message, textPosition + shadowOffset, Color.Black * 0.5f);


                spriteBatch.DrawString(_gameFont, message, textPosition, Color.Green);

                string restartMessage = "Press R to Restart"; // Space added here
                Vector2 restartTextSize = _gameFont.MeasureString(restartMessage);
                Vector2 restartTextPosition = new(
                    (ScreenManager.GraphicsDevice.Viewport.Width - restartTextSize.X) / 2,
                    textPosition.Y + textSize.Y + 20
                );


                spriteBatch.DrawString(_gameFont, restartMessage, restartTextPosition + shadowOffset,
                    Color.Black * 0.5f);


                spriteBatch.DrawString(_gameFont, restartMessage, restartTextPosition, Color.White);
            }

            spriteBatch.End();

            if (TransitionPosition > 0 || _pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, _pauseAlpha / 2);
                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }
    }
}