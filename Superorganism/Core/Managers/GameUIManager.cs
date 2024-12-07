﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.AI;
using Superorganism.Entities;
using Superorganism.Enums;

namespace Superorganism.Core.Managers
{
    public class GameUiManager
    {
        private readonly SpriteFont _gameFont;
        private readonly SpriteBatch _spriteBatch;
        private Texture2D _grayTexture;
        private Texture2D _redTexture;

        // UI Constants
        private const int ScreenMargin = 40; 
        private const int BarPadding = 10;

        public GameUiManager(SpriteFont gameFont, SpriteBatch spriteBatch)
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
            const int barWidth = 200;
            const int barHeight = 30;

            // Draw background (gray) bar
            Rectangle backgroundRect = new(ScreenMargin, ScreenMargin, barWidth, barHeight);
            _spriteBatch.Draw(_grayTexture, backgroundRect, Color.White);

            // Calculate and clamp health percentage
            float healthPercentage = Math.Clamp((float)currentHealth / maxHealth, 0f, 1f);

            // Draw foreground (red) bar only if health > 0
            if (healthPercentage > 0)
            {
                Rectangle healthRect = new(ScreenMargin, ScreenMargin, (int)(barWidth * healthPercentage), barHeight);
                _spriteBatch.Draw(_redTexture, healthRect, Color.White);
            }

            // Draw health text with padding
            string healthText = $"{currentHealth}/{maxHealth}";
            const float textScale = 0.55f;
            Vector2 textSize = _gameFont.MeasureString(healthText) * textScale;
            Vector2 textPosition = new(
                ScreenMargin + (barWidth - textSize.X) / 2,
                ScreenMargin + (barHeight - textSize.Y) / 2 - 2
            );
            DrawTextWithShadow(healthText, textPosition, Color.White, textScale);
        }

        public void DrawCropsLeft(int cropsLeft)
        {
            string cropsLeftText = $"Crops Left: {cropsLeft}";
            const float textScale = 0.75f;
            Vector2 textSize = _gameFont.MeasureString(cropsLeftText) * textScale;
            Vector2 textPosition = new(
                _spriteBatch.GraphicsDevice.Viewport.Width - textSize.X - ScreenMargin,
                ScreenMargin
            );
            DrawTextWithShadow(cropsLeftText, textPosition, Color.White, textScale);
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

            // Add padding to the debug info position
            Vector2 textOffset = new(BarPadding, -40);
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
            const string historyHeader = "History:";
            DrawDebugText(historyHeader, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(historyHeader).Y * textScale;

            // Draw strategy history
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
            string adjustedText = text.Replace(" ", "   ");

            Vector2 shadowOffset = new(2, 2);
            _spriteBatch.DrawString(_gameFont, adjustedText, position + shadowOffset, Color.Black * 0.5f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
            _spriteBatch.DrawString(_gameFont, adjustedText, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
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