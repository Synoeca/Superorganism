using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Superorganism.AI;
using Superorganism.Collisions;
using Superorganism.Entities;
using Superorganism.Enums;
using Superorganism.Tiles;

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

        public void DrawCollisionBounds(Entity entity, ICollisionBounding collisionBounds, Matrix cameraMatrix)
        {
            const int borderThickness = 2;

            if (collisionBounds is BoundingRectangle rect)
            {
                // Draw both the entity position and the collision bounds for debugging
                Vector2 screenEntityPos = Vector2.Transform(entity.CollisionBounding.Center, cameraMatrix);

                // Draw a small cross at the entity position
                DrawLine(screenEntityPos - new Vector2(5, 0), screenEntityPos + new Vector2(5, 0), Color.Yellow, 1);
                DrawLine(screenEntityPos - new Vector2(0, 5), screenEntityPos + new Vector2(0, 5), Color.Yellow, 1);

                // Assume rect coordinates should be relative to entity position
                Vector2 worldTopLeft = new Vector2(
                    entity.Position.X + (rect.X % 1000), // Use modulo to handle potential world coordinates
                    entity.Position.Y + (rect.Y % 1000)
                );

                Rectangle screenBounds = new(
                    (int)worldTopLeft.X,
                    (int)worldTopLeft.Y,
                    (int)rect.Width,
                    (int)rect.Height
                );

                DrawRectangleOutline(screenBounds, borderThickness, Color.Red);
            }
            else if (collisionBounds is BoundingCircle circle)
            {
                // Transform entity position to screen space
                Vector2 screenEntityPos = Vector2.Transform(entity.Position, cameraMatrix);
                //screenEntityPos.X += 5;
                //screenEntityPos.Y += 5;

                screenEntityPos.X += circle.Radius;
                screenEntityPos.Y += circle.Radius;

                // Draw entity position marker (yellow cross)
                DrawLine(screenEntityPos - new Vector2(5, 0), screenEntityPos + new Vector2(5, 0), Color.Yellow, 1);
                DrawLine(screenEntityPos - new Vector2(0, 5), screenEntityPos + new Vector2(0, 5), Color.Yellow, 1);

                if (entity is Crop crop)
                {
                    // For crops, draw the circle at the transformed entity position
                    DrawCircleOutline(screenEntityPos, circle.Radius, borderThickness, Color.Red);
                    // Debug print
                    System.Diagnostics.Debug.WriteLine($"Crop Position: {crop.Position}, Screen Pos: {screenEntityPos}, Radius: {circle.Radius}");
                }
                else
                {
                    // For other entities, use the relative center calculation
                    Vector2 relativeCenter = new Vector2(
                        circle.Center.X % MapHelper.TileSize, // Use TileSize instead of 1000
                        circle.Center.Y % MapHelper.TileSize
                    );

                    Vector2 worldCenter = entity.Position + relativeCenter;
                    Vector2 screenCenter = Vector2.Transform(worldCenter, cameraMatrix);

                    DrawCircleOutline(screenCenter, circle.Radius, borderThickness, Color.Red);
                }
            }
        }

        private void DrawRectangleOutline(Rectangle rect, int thickness, Color color)
        {
            _spriteBatch.Draw(_redTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            _spriteBatch.Draw(_redTexture, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            _spriteBatch.Draw(_redTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            _spriteBatch.Draw(_redTexture, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }

        private void DrawCircleOutline(Vector2 center, float radius, int thickness, Color color)
        {
            const int segments = 32;
            Vector2 prevPoint = center + new Vector2(radius, 0);
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * MathHelper.TwoPi / segments;
                Vector2 newPoint = center + radius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                DrawLine(prevPoint, newPoint, color, thickness);
                prevPoint = newPoint;
            }
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color, int thickness)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();
            _spriteBatch.Draw(
                _redTexture,
                new Rectangle((int)start.X, (int)start.Y, (int)length, thickness),
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

        public void DrawDebugInfo(Vector2 position, Matrix cameraMatrix,
    float distanceToPlayer, ICollisionBounding collisionBounding)
        {
            Vector2 enemyScreenPosition = Vector2.Transform(position, cameraMatrix);
            Vector2 textOffset = new(BarPadding, -40);
            Vector2 textPosition = enemyScreenPosition + textOffset;
            const float textScale = 0.4f;
            Vector2 currentOffset = Vector2.Zero;

            string positionText = $"Position: {position.X:0.0}, {position.Y:0.0}";
            DrawDebugText(positionText, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(positionText).Y * textScale;

            string screenPosText = $"Screen Position: {enemyScreenPosition.X:0.0}, {enemyScreenPosition.Y:0.0}";
            DrawDebugText(screenPosText, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(screenPosText).Y * textScale;

            string distanceText = $"Distance: {distanceToPlayer:0.0}";
            DrawDebugText(distanceText, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(distanceText).Y * textScale;

            // Add collision bounding information
            string boundingText = "Bounding: ";
            if (collisionBounding is BoundingCircle circle)
            {
                boundingText += $"Circle (Center: {circle.Center.X:0.0}, {circle.Center.Y:0.0}, Radius: {circle.Radius:0.0})";
                DrawDebugText(boundingText, textPosition + currentOffset, textScale);
            }
            else if (collisionBounding is BoundingRectangle rect)
            {
                boundingText += $"Rectangle (X: {rect.X:0.0}, Y: {rect.Y:0.0}, W: {rect.Width:0.0}, H: {rect.Height:0.0})";
                string leftRightText = $"Left: {rect.Left:0.0}, Right: {rect.Right:0.0}";
                string topBottomText = $"Top: {rect.Top:0.0}, Bottom: {rect.Bottom:0.0}";
                string rectCenterText = $"  Center: {rect.X + (rect.Width / 2):0.0}, {rect.Y + (rect.Height / 2):0.0}";

                DrawDebugText(boundingText, textPosition + currentOffset, textScale);
                currentOffset.Y += _gameFont.MeasureString(boundingText).Y * textScale;
                DrawDebugText(leftRightText, textPosition + currentOffset, textScale);
                currentOffset.Y += _gameFont.MeasureString(boundingText).Y * textScale;
                DrawDebugText(topBottomText, textPosition + currentOffset, textScale);
                currentOffset.Y += _gameFont.MeasureString(boundingText).Y * textScale;
                DrawDebugText(rectCenterText, textPosition + currentOffset, textScale);
                currentOffset.Y += _gameFont.MeasureString(rectCenterText).Y * textScale;
            }

            currentOffset.Y += _gameFont.MeasureString(boundingText).Y * textScale;

            const string historyHeader = "History:";
            DrawDebugText(historyHeader, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(historyHeader).Y * textScale;
        }

        public void DrawDebugInfo(
            Vector2 position,
            Matrix cameraMatrix,
            Strategy currentStrategy,
            float distanceToPlayer,
            List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory,
            ICollisionBounding collisionBounding)
        {
            Vector2 screenPosition = Vector2.Transform(position, cameraMatrix);
            Vector2 textOffset = new(BarPadding, -40);
            Vector2 textPosition = screenPosition + textOffset;
            const float textScale = 0.4f;
            Vector2 currentOffset = Vector2.Zero;

            string currentStrategyText = $"Current: {currentStrategy}";
            DrawDebugText(currentStrategyText, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(currentStrategyText).Y * textScale;

            string positionText = $"Position: {position.X:0.0}, {position.Y:0.0}";
            DrawDebugText(positionText, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(positionText).Y * textScale;

            string screenPosText = $"Screen Position: {screenPosition.X:0.0}, {screenPosition.Y:0.0}";
            DrawDebugText(screenPosText, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(screenPosText).Y * textScale;

            string distanceText = $"Distance: {distanceToPlayer:0.0}";
            DrawDebugText(distanceText, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(distanceText).Y * textScale;

            // Enhanced collision bounding information
            string boundingText = "Bounding: ";
            if (collisionBounding is BoundingCircle circle)
            {
                boundingText += $"Circle (Center: {circle.Center.X:0.0}, {circle.Center.Y:0.0}, Radius: {circle.Radius:0.0})";
                DrawDebugText(boundingText, textPosition + currentOffset, textScale);
                currentOffset.Y += _gameFont.MeasureString(boundingText).Y * textScale;
            }
            else if (collisionBounding is BoundingRectangle rect)
            {
                boundingText += $"Rectangle (X: {rect.X:0.0}, Y: {rect.Y:0.0}, W: {rect.Width:0.0}, H: {rect.Height:0.0})";
                string leftRightText = $"Left: {rect.Left:0.0}, Right: {rect.Right:0.0}";
                string topBottomText = $"Top: {rect.Top:0.0}, Bottom: {rect.Bottom:0.0}";
                string rectCenterText = $"  Center: {rect.Center.X:0.0}, {rect.Center.Y:0.0}";

                DrawDebugText(boundingText, textPosition + currentOffset, textScale);
                currentOffset.Y += _gameFont.MeasureString(boundingText).Y * textScale;
                DrawDebugText(leftRightText, textPosition + currentOffset, textScale);
                currentOffset.Y += _gameFont.MeasureString(boundingText).Y * textScale;
                DrawDebugText(topBottomText, textPosition + currentOffset, textScale);
                currentOffset.Y += _gameFont.MeasureString(boundingText).Y * textScale;
                DrawDebugText(rectCenterText, textPosition + currentOffset, textScale);
                currentOffset.Y += _gameFont.MeasureString(rectCenterText).Y * textScale;
            }

            const string historyHeader = "History:";
            DrawDebugText(historyHeader, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(historyHeader).Y * textScale;

            foreach ((Strategy strategy, double startTime, double lastActionTime) in strategyHistory.Skip(Math.Max(0, strategyHistory.Count - 3)))
            {
                string historyText = $"- {strategy} Start: {startTime:0.0}s Last: {lastActionTime:0.0}s";
                DrawDebugText(historyText, textPosition + currentOffset, textScale);
                currentOffset.Y += _gameFont.MeasureString(historyText).Y * textScale;
            }
        }

        public void DrawMousePositionDebug(GameTime gameTime, Matrix cameraMatrix)
        {
            // Get current mouse state
            MouseState mouseState = Mouse.GetState();
            Vector2 mouseScreenPosition = new Vector2(mouseState.X, mouseState.Y);

            // Convert screen position to world position
            Matrix invertedMatrix = Matrix.Invert(cameraMatrix);
            Vector2 mouseWorldPosition = Vector2.Transform(mouseScreenPosition, invertedMatrix);

            // Setup text position and scale
            Vector2 textPosition = mouseScreenPosition + new Vector2(20, 20); // Offset from cursor
            const float textScale = 0.4f;
            Vector2 currentOffset = Vector2.Zero;

            // Draw screen coordinates
            string screenPosText = $"Screen: {mouseScreenPosition.X:0.0}, {mouseScreenPosition.Y:0.0}";
            DrawDebugText(screenPosText, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(screenPosText).Y * textScale;

            // Draw world coordinates
            string worldPosText = $"World: {mouseWorldPosition.X:0.0}, {mouseWorldPosition.Y:0.0}";
            DrawDebugText(worldPosText, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(worldPosText).Y * textScale;

            // Draw tile coordinates
            (int tileX, int tileY) = MapHelper.WorldToTile(mouseWorldPosition);
            string tilePosText = $"Tile: {tileX}, {tileY}";
            DrawDebugText(tilePosText, textPosition + currentOffset, textScale);
        }

        private void DrawDebugText(string text, Vector2 position, float scale)
        {
            DrawTextWithShadow(text, position, Color.White, scale);
        }

        private void DrawTextWithShadow(string text, Vector2 position, Color color, float scale = 1.0f)
        {
            string adjustedText = text.Replace(" ", "   ");

            Vector2 shadowOffset = new(2, 2);
            _spriteBatch.DrawString(_gameFont, adjustedText, position + shadowOffset, Color.Black * 0.5f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
            _spriteBatch.DrawString(_gameFont, adjustedText, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
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