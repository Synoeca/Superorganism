using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.AI;
using Superorganism.Entities;
using Superorganism.Enums;

namespace Superorganism.Core.Managers
{
    public class GameUIManager
    {
        private readonly SpriteFont _gameFont;
        private readonly SpriteBatch _spriteBatch;
        private Texture2D _grayTexture;
        private Texture2D _redTexture;

        public GameUIManager(SpriteFont gameFont, SpriteBatch spriteBatch)
        {
            _gameFont = gameFont;
            _spriteBatch = spriteBatch;
            InitializeTextures();
        }

        private void InitializeTextures()
        {
            // Create textures once and store them
            _grayTexture = new Texture2D(_spriteBatch.GraphicsDevice, 1, 1);
            _grayTexture.SetData([Color.Gray]);

            _redTexture = new Texture2D(_spriteBatch.GraphicsDevice, 1, 1);
            _redTexture.SetData([Color.Red]);
        }

        public void DrawHealthBar(int currentHealth, int maxHealth)
        {
            // Debug output
            System.Diagnostics.Debug.WriteLine($"Drawing health bar - Current: {currentHealth}, Max: {maxHealth}");

            const int barWidth = 200;
            const int barHeight = 20;
            const int barX = 20;
            const int barY = 20;

            // Draw background (gray) bar
            Rectangle backgroundRect = new(barX, barY, barWidth, barHeight);
            _spriteBatch.Draw(_grayTexture, backgroundRect, Color.White);

            // Calculate and clamp health percentage
            float healthPercentage = Math.Clamp((float)currentHealth / maxHealth, 0f, 1f);

            // Debug output
            System.Diagnostics.Debug.WriteLine($"Health percentage: {healthPercentage}");

            // Draw foreground (red) bar only if health > 0
            if (healthPercentage > 0)
            {
                Rectangle healthRect = new(barX, barY, (int)(barWidth * healthPercentage), barHeight);
                _spriteBatch.Draw(_redTexture, healthRect, Color.White);
            }

            // Draw health text
            string healthText = $"{currentHealth}/{maxHealth}";
            Vector2 textPosition = new(barX + barWidth + 10, barY);
            DrawTextWithShadow(healthText, textPosition, Color.White);
        }

        public void DrawCropsLeft(int cropsLeft)
        {
            string cropsLeftText = $"Crops Left: {cropsLeft}";
            Vector2 textSize = _gameFont.MeasureString(cropsLeftText);
            Vector2 textPosition = new(
                _spriteBatch.GraphicsDevice.Viewport.Width - textSize.X - 20,
                20
            );
            DrawTextWithShadow(cropsLeftText, textPosition, Color.White);
        }

        public void DrawEnemyDebugInfo(
            Vector2 enemyPosition,
            Matrix cameraMatrix,
            Strategy currentStrategy,
            float distanceToPlayer,
            List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory)
        {
            // Transform enemy position to screen coordinates
            Vector2 enemyScreenPosition = Vector2.Transform(enemyPosition, cameraMatrix);

            // Offset to avoid overlapping the enemy sprite
            Vector2 textOffset = new(0, -40);
            Vector2 textPosition = enemyScreenPosition + textOffset;

            // Text scaling factor for reduced size
            const float textScale = 0.4f;
            Vector2 currentOffset = Vector2.Zero;

            // Draw current strategy
            string currentStrategyText = $"Current: {currentStrategy}";
            DrawDebugText(currentStrategyText, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(currentStrategyText).Y * textScale;

            // Draw position
            string positionText = $"Position: {enemyPosition.X:0.0}, {enemyPosition.Y:0.0}";
            DrawDebugText(positionText, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(positionText).Y * textScale;

            // Draw distance
            string distanceText = $"Distance: {distanceToPlayer:0.0}";
            DrawDebugText(distanceText, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(distanceText).Y * textScale;

            // Draw history header
            string historyHeader = "History:";
            DrawDebugText(historyHeader, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(historyHeader).Y * textScale;

            // Draw strategy history (last 3 entries)
            foreach ((Strategy strategy, double startTime, double lastActionTime) in strategyHistory.Skip(Math.Max(0, strategyHistory.Count - 3)))
            {
                string historyText = $"- {strategy} Start: {startTime:0.0}s Last: {lastActionTime:0.0}s";
                DrawDebugText(historyText, textPosition + currentOffset, textScale);
                currentOffset.Y += _gameFont.MeasureString(historyText).Y * textScale;
            }
        }

        private void DrawDebugText(string text, Vector2 position, float scale)
        {
            DrawTextWithShadow(text, position, Color.White, scale);
        }

        public void DrawGameOverScreen()
        {
            DrawCenteredMessage("You Lose", Color.Red);
            DrawRestartPrompt();
        }

        public void DrawWinScreen()
        {
            DrawCenteredMessage("You Win", Color.Green);
            DrawRestartPrompt();
        }

        private void DrawCenteredMessage(string message, Color color)
        {
            Vector2 textSize = _gameFont.MeasureString(message);
            Vector2 position = new(
                (_spriteBatch.GraphicsDevice.Viewport.Width - textSize.X) / 2,
                (_spriteBatch.GraphicsDevice.Viewport.Height - textSize.Y) / 2
            );
            DrawTextWithShadow(message, position, color);
        }

        private void DrawRestartPrompt()
        {
            const string restartMessage = "Press R to Restart";
            Vector2 textSize = _gameFont.MeasureString(restartMessage);
            Vector2 position = new(
                (_spriteBatch.GraphicsDevice.Viewport.Width - textSize.X) / 2,
                (_spriteBatch.GraphicsDevice.Viewport.Height + textSize.Y) / 2 + 20
            );
            DrawTextWithShadow(restartMessage, position, Color.White);
        }

        private void DrawTextWithShadow(string text, Vector2 position, Color color, float scale = 1.0f)
        {
            Vector2 shadowOffset = new(2, 2);
            _spriteBatch.DrawString(_gameFont, text, position + shadowOffset, Color.Black * 0.5f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
            _spriteBatch.DrawString(_gameFont, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        private static Texture2D CreateTexture(GraphicsDevice graphicsDevice, Color color)
        {
            Texture2D texture = new(graphicsDevice, 1, 1);
            texture.SetData([color]);
            return texture;
        }

        public void Dispose()
        {
            _grayTexture?.Dispose();
            _redTexture?.Dispose();
        }
    }
}