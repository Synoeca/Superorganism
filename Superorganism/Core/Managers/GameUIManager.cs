using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.AI;
using Superorganism.Collisions;
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
        private Texture2D _borderTexture;

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
            _grayTexture = CreateTexture(_spriteBatch.GraphicsDevice, Color.Gray);
            _redTexture = CreateTexture(_spriteBatch.GraphicsDevice, Color.Red);
            _borderTexture = CreateTexture(_spriteBatch.GraphicsDevice, Color.Red);
        }

        /// <summary>
        /// Draws collision bounds for debugging. Supports both rectangles and circles.
        /// </summary>
        public void DrawCollisionBounds(ICollisionBounding collisionBounds, Matrix cameraMatrix)
        {
            const int borderThickness = 10; // Visible line thickness

            if (collisionBounds is BoundingRectangle rect)
            {
                // Transform rectangle coordinates to screen space
                Rectangle screenBounds = new(
                    (int)(rect.X * 2560), // Adjust to screen width
                    (int)(rect.Y * 1440), // Adjust to screen height
                    (int)(rect.Width * 2560),
                    (int)(rect.Height * 1440)
                );

                // Draw the rectangle outline
                // Top
                _spriteBatch.Draw(_redTexture, new Rectangle(screenBounds.X, screenBounds.Y, screenBounds.Width, borderThickness), Color.Red);
                // Bottom
                _spriteBatch.Draw(_redTexture, new Rectangle(screenBounds.X, screenBounds.Y + screenBounds.Height - borderThickness, screenBounds.Width, borderThickness), Color.Red);
                // Left
                _spriteBatch.Draw(_redTexture, new Rectangle(screenBounds.X, screenBounds.Y, borderThickness, screenBounds.Height), Color.Red);
                // Right
                _spriteBatch.Draw(_redTexture, new Rectangle(screenBounds.X + screenBounds.Width - borderThickness, screenBounds.Y, borderThickness, screenBounds.Height), Color.Red);
            }
            else if (collisionBounds is BoundingCircle circle)
            {
                const int segments = 32;
                for (int i = 0; i < segments; i++)
                {
                    float angle1 = i * MathHelper.TwoPi / segments;
                    float angle2 = (i + 1) * MathHelper.TwoPi / segments;

                    Vector2 point1 = new(
                        circle.Center.X + (float)Math.Cos(angle1) * circle.Radius,
                        circle.Center.Y + (float)Math.Sin(angle1) * circle.Radius
                    );

                    Vector2 point2 = new(
                        circle.Center.X + (float)Math.Cos(angle2) * circle.Radius,
                        circle.Center.Y + (float)Math.Sin(angle2) * circle.Radius
                    );

                    // Transform points to screen space
                    Vector2 screenPoint1 = Vector2.Transform(point1 * new Vector2(2560, 1440), cameraMatrix);
                    Vector2 screenPoint2 = Vector2.Transform(point2 * new Vector2(2560, 1440), cameraMatrix);

                    // Draw the line segment
                    DrawLine(screenPoint1, screenPoint2, Color.Red, borderThickness);

                    // Debug markers
                    const int markerSize = 10;
                    _spriteBatch.Draw(
                        _redTexture,
                        new Rectangle((int)screenPoint1.X - markerSize / 2, (int)screenPoint1.Y - markerSize / 2, markerSize, markerSize),
                        Color.Blue
                    );
                    _spriteBatch.Draw(
                        _redTexture,
                        new Rectangle((int)screenPoint2.X - markerSize / 2, (int)screenPoint2.Y - markerSize / 2, markerSize, markerSize),
                        Color.Green
                    );
                }
            }
        }


        private void DrawLine(Vector2 start, Vector2 end, Color color, int borderThickness)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            // Calculate the length of the line
            float length = edge.Length();

            // Log debug info for verification
            Console.WriteLine($"Drawing Line: Start={start}, End={end}, Angle={angle}");

            // Draw the line with the specified thickness
            _spriteBatch.Draw(
                _redTexture,
                new Rectangle((int)start.X, (int)start.Y, (int)length, borderThickness),
                null,
                color,
                angle,
                Vector2.Zero,
                SpriteEffects.None,
                0
            );
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
            _borderTexture?.Dispose();
        }
    }
}