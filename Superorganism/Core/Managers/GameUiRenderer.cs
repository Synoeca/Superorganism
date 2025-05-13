using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Superorganism.AI;
using Superorganism.Collisions;
using Superorganism.Entities;
using Superorganism.Tiles;

namespace Superorganism.Core.Managers
{
    /// <summary>
    /// Renders game UI elements and debug visualizations
    /// </summary>
    public class GameUiRenderer
    {
        private readonly SpriteFont _gameFont;
        private readonly SpriteBatch _spriteBatch;
        private Texture2D _grayTexture;
        private Texture2D _redTexture;
        private Texture2D _greenTexture;
        private Texture2D _orangeTexture;
        private Texture2D _borderTexture;
        private Texture2D _itemIndicatorTexture;

        // UI Constants
        private const int ScreenMargin = 40; 
        private const int BarPadding = 10;
        private const int BarSpacing = 10;

        // Debug flags
        private bool _showCollisionBounds;
        private bool _showEntityInfo;
        private bool _showMousePosition;

        private float _itemIndicatorPulse = 0f;
        private const float PulseSpeed = 4.0f;  // Adjust for faster/slower pulsing

        /// <summary>
        /// Toggles the visibility of collision boundaries in debug view.
        /// </summary>
        public void ToggleCollisionBounds() => _showCollisionBounds = !_showCollisionBounds;

        /// <summary>
        /// Toggles the display of detailed entity information in debug view.
        /// </summary>
        public void ToggleEntityInfo() => _showEntityInfo = !_showEntityInfo;

        /// <summary>
        /// Toggles the visibility of mouse position coordinates in debug view.
        /// </summary>
        public void ToggleMousePosition() => _showMousePosition = !_showMousePosition;

        /// <summary>
        /// Initializes a new instance of the GameUiRenderer with required rendering resources.
        /// </summary>
        /// <param name="gameFont">The font used for rendering text elements.</param>
        /// <param name="spriteBatch">The sprite batch used for drawing operations.</param>
        public GameUiRenderer(SpriteFont gameFont, SpriteBatch spriteBatch)
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
            _greenTexture = CreateTexture(_spriteBatch.GraphicsDevice, Color.Green);
            _orangeTexture = CreateTexture(_spriteBatch.GraphicsDevice, Color.Orange);
            _borderTexture = CreateTexture(_spriteBatch.GraphicsDevice, Color.Red);
            _itemIndicatorTexture = CreateTexture(_spriteBatch.GraphicsDevice, Color.White);
        }

        public void DrawCollisionBounds(Entity entity, ICollisionBounding collisionBounds, Matrix cameraMatrix)
        {
            const int borderThickness = 2;

            if (collisionBounds is BoundingRectangle rect)
            {
                // Transform entity position to screen space, same as circle logic
                Vector2 screenEntityPos = Vector2.Transform(entity.Position, cameraMatrix);

                // Add half width/height offset (equivalent to adding radius for circle)
                screenEntityPos.X += rect.Width / 2;
                screenEntityPos.Y += rect.Height / 2;

                // Draw entity position marker (yellow cross)
                DrawLine(screenEntityPos - new Vector2(5, 0), screenEntityPos + new Vector2(5, 0), Color.Yellow, 1);
                DrawLine(screenEntityPos - new Vector2(0, 5), screenEntityPos + new Vector2(0, 5), Color.Yellow, 1);

                // Create rectangle centered on screen position, compensating for border thickness
                Rectangle screenBounds = new(
                    (int)(screenEntityPos.X - rect.Width / 2) + borderThickness,  // Compensate for left border
                    (int)(screenEntityPos.Y - rect.Height / 2) + borderThickness, // Compensate for top border
                    (int)rect.Width - borderThickness * 2,  // Adjust width for both borders
                    (int)rect.Height - borderThickness * 2  // Adjust height for both borders
                );

                DrawRectangleOutline(screenBounds, borderThickness, Color.Red);
            }
            else if (collisionBounds is BoundingCircle circle)
            {
                // Transform entity position to screen space
                Vector2 screenEntityPos = Vector2.Transform(entity.Position, cameraMatrix);

                screenEntityPos.X += circle.Radius;
                screenEntityPos.Y += circle.Radius;

                // Draw entity position marker (yellow cross)
                DrawLine(screenEntityPos - new Vector2(5, 0), screenEntityPos + new Vector2(5, 0), Color.Yellow, 1);
                DrawLine(screenEntityPos - new Vector2(0, 5), screenEntityPos + new Vector2(0, 5), Color.Yellow, 1);

                if (entity is Crop crop)
                {
                    DrawCircleOutline(screenEntityPos, circle.Radius, borderThickness, Color.Red);
                }
                else
                {
                    DrawCircleOutline(screenEntityPos, circle.Radius, borderThickness, Color.Red);
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

        /// <summary>
        /// Draws health, stamina, and hunger bars for the player.
        /// </summary>
        /// <param name="hitPoints">Current hit points value.</param>
        /// <param name="maxHitPoints">Maximum hit points value.</param>
        /// <param name="stamina">Current stamina value.</param>
        /// <param name="maxStamina">Maximum stamina value.</param>
        /// <param name="hunger">Current hunger value.</param>
        /// <param name="maxHunger">Maximum hunger value.</param>
        public void DrawPlayerStatus(float hitPoints, float maxHitPoints, float stamina, float maxStamina, float hunger, float maxHunger)
        {
            const int barWidth = 200;
            const int barHeight = 30;

            // Calculate positions
            int healthBarY = ScreenMargin;
            int staminaBarY = healthBarY + barHeight + BarSpacing;
            int hungerBarY = staminaBarY + barHeight + BarSpacing;

            // Draw health bar
            DrawStatusBar(hitPoints, maxHitPoints, barWidth, barHeight, ScreenMargin, healthBarY, _redTexture, "Health");

            // Draw stamina bar
            DrawStatusBar((int)stamina, (int)maxStamina, barWidth, barHeight, ScreenMargin, staminaBarY, _greenTexture, "Stamina");

            // Draw hunger bar
            DrawStatusBar((int)hunger, (int)maxHunger, barWidth, barHeight, ScreenMargin, hungerBarY, _orangeTexture, "Hunger");
        }

        /// <summary>
        /// Draws a generic status bar with label.
        /// </summary>
        /// <param name="currentValue">Current value to display.</param>
        /// <param name="maxValue">Maximum possible value.</param>
        /// <param name="barWidth">Width of the status bar.</param>
        /// <param name="barHeight">Height of the status bar.</param>
        /// <param name="xPosition">X position of the bar.</param>
        /// <param name="yPosition">Y position of the bar.</param>
        /// <param name="fillTexture">Texture to use for filling the bar.</param>
        /// <param name="label">Label text to display.</param>
        private void DrawStatusBar(float currentValue, float maxValue, int barWidth, int barHeight,
                                   int xPosition, int yPosition, Texture2D fillTexture, string label)
        {
            // Draw background (gray) bar
            Rectangle backgroundRect = new(xPosition, yPosition, barWidth, barHeight);
            _spriteBatch.Draw(_grayTexture, backgroundRect, Color.White);

            // Calculate and clamp percentage
            float percentage = Math.Clamp((float)currentValue / maxValue, 0f, 1f);

            // Draw foreground bar only if value > 0
            if (percentage > 0)
            {
                Rectangle foregroundRect = new(xPosition, yPosition, (int)(barWidth * percentage), barHeight);
                _spriteBatch.Draw(fillTexture, foregroundRect, Color.White);
            }

            // Draw text with padding
            string statusText = $"{label}: {currentValue}/{maxValue}";
            const float textScale = 0.55f;
            Vector2 textSize = _gameFont.MeasureString(statusText) * textScale;
            Vector2 textPosition = new(
                xPosition + (barWidth - textSize.X) / 2,
                yPosition + (barHeight - textSize.Y) / 2 - 2
            );
            DrawTextWithShadow(statusText, textPosition, Color.White, textScale);
        }

        // Original DrawHealthBar can now be simplified to use DrawStatusBar
        public void DrawHealthBar(int currentHealth, int maxHealth)
        {
            const int barWidth = 200;
            const int barHeight = 30;
            DrawStatusBar(currentHealth, maxHealth, barWidth, barHeight, ScreenMargin, ScreenMargin, _redTexture, "Health");
        }


        //public void DrawHealthBar(int currentHealth, int maxHealth)
        //{
        //    const int barWidth = 200;
        //    const int barHeight = 30;

        //    // Draw background (gray) bar
        //    Rectangle backgroundRect = new(ScreenMargin, ScreenMargin, barWidth, barHeight);
        //    _spriteBatch.Draw(_grayTexture, backgroundRect, Color.White);

        //    // Calculate and clamp health percentage
        //    float healthPercentage = Math.Clamp((float)currentHealth / maxHealth, 0f, 1f);

        //    // Draw foreground (red) bar only if health > 0
        //    if (healthPercentage > 0)
        //    {
        //        Rectangle healthRect = new(ScreenMargin, ScreenMargin, (int)(barWidth * healthPercentage), barHeight);
        //        _spriteBatch.Draw(_redTexture, healthRect, Color.White);
        //    }

        //    // Draw health text with padding
        //    string healthText = $"{currentHealth}/{maxHealth}";
        //    const float textScale = 0.55f;
        //    Vector2 textSize = _gameFont.MeasureString(healthText) * textScale;
        //    Vector2 textPosition = new(
        //        ScreenMargin + (barWidth - textSize.X) / 2,
        //        ScreenMargin + (barHeight - textSize.Y) / 2 - 2
        //    );
        //    DrawTextWithShadow(healthText, textPosition, Color.White, textScale);
        //}

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

        /// <summary>
        /// Draws an indicator when items are nearby and can be collected
        /// </summary>
        /// <param name="gameTime">Current game time for animation</param>
        /// <param name="nearbyItem">The nearby item that can be collected, or null if none</param>
        public void DrawNearbyItemIndicator(GameTime gameTime, DroppedItem nearbyItem)
        {
            // Early return if there's no nearby item or it can't be collected
            if (nearbyItem == null || !nearbyItem.CanBeCollected || nearbyItem.Collected)
                return;

            // Update the pulse animation
            _itemIndicatorPulse += (float)gameTime.ElapsedGameTime.TotalSeconds * PulseSpeed;
            float pulse = (float)Math.Sin(_itemIndicatorPulse) * 0.2f + 0.8f;

            // Determine indicator text
            string indicatorText = $"Press G to collect: {nearbyItem.ItemName}";

            // Draw at bottom center of screen
            float textScale = 0.8f;
            Vector2 textSize = _gameFont.MeasureString(indicatorText) * textScale;

            // Position text at bottom center with some margin
            int bottomMargin = 60;
            Vector2 textPosition = new(
                (_spriteBatch.GraphicsDevice.Viewport.Width - textSize.X) / 2,
                _spriteBatch.GraphicsDevice.Viewport.Height - textSize.Y - bottomMargin
            );

            // Draw background panel with animation
            Rectangle panelRect = new(
                (int)textPosition.X - 15,
                (int)textPosition.Y - 5,
                (int)textSize.X + 60,
                (int)textSize.Y + 20
            );

            // Background panel with pulsating transparency
            Color panelColor = new(40, 40, 60, 200);
            _spriteBatch.Draw(_grayTexture, panelRect, panelColor);

            // Draw border with highlight color
            Color borderColor = new(
                255,
                (215 * pulse),  // Yellow-gold pulsating
                0,
                255
            );
            DrawRectangleOutline(panelRect, 2, borderColor);

            // Draw text with shadow for better readability
            DrawTextWithShadow(indicatorText, textPosition, Color.White, textScale);
        }

        // Add this method to your GameUiRenderer class
        /// <summary>
        /// Draws a visual indicator at the position of collectible item in the world
        /// </summary>
        /// <param name="gameTime">Current game time for animation</param>
        /// <param name="nearbyItem">The nearby item to highlight</param>
        /// <param name="cameraMatrix">The camera transformation matrix</param>
        public void DrawItemWorldIndicator(GameTime gameTime, DroppedItem nearbyItem, Matrix cameraMatrix)
        {
            if (nearbyItem == null || !nearbyItem.CanBeCollected || nearbyItem.Collected)
                return;

            // Update the pulse animation (uses the shared pulse value)
            float pulse = (float)Math.Sin(_itemIndicatorPulse) * 0.3f + 0.7f;

            // Transform item position to screen coordinates
            Vector2 screenPos = Vector2.Transform(nearbyItem.Position, cameraMatrix);

            // Calculate size of the indicator (increased for better visibility)
            int indicatorSize = 24;

            // Draw an arrow or indicator above the item
            Vector2 arrowTop = screenPos + new Vector2(0, -indicatorSize * 2.2f);
            Vector2 arrowLeft = screenPos + new Vector2(-indicatorSize / 2, -indicatorSize * 1.4f);
            Vector2 arrowRight = screenPos + new Vector2(indicatorSize / 2, -indicatorSize * 1.4f);

            // Brighter, more pulsing gold/yellow color
            Color indicatorColor = new Color(255, (byte)(215 * pulse), 0) * pulse;

            // Draw a triangle arrow pointing down at the item
            DrawLine(arrowTop, arrowLeft, indicatorColor, 3); // Thicker lines
            DrawLine(arrowLeft, arrowRight, indicatorColor, 3);
            DrawLine(arrowRight, arrowTop, indicatorColor, 3);

            // Draw a circle around the item
            float circleRadius = indicatorSize * (0.8f + (pulse * 0.3f)); // More pulsating
            DrawCircleOutline(screenPos, circleRadius, 3, indicatorColor); // Thicker circle
        }

        public void DrawDebugInfo(Vector2 position, Matrix cameraMatrix, float distanceToPlayer, ICollisionBounding collisionBounding)
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
            Vector2 velocity,
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

            string velocityText = $"Velocity: {velocity.X:0.0}, {velocity.Y:0.0}";
            DrawDebugText(velocityText, textPosition + currentOffset, textScale);
            currentOffset.Y += _gameFont.MeasureString(velocityText).Y * textScale;

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

        /// <summary>
        /// Draws the remaining enemy ants counter on the UI
        /// </summary>
        /// <param name="enemiesRemaining">Current number of remaining enemies</param>
        public void DrawEnemiesRemaining(int enemiesRemaining)
        {
            // Display the enemy counter near the crops counter in the top-right
            string enemiesText = $"Enemy Ants: {enemiesRemaining}";
            const float textScale = 0.75f;
            Vector2 textSize = _gameFont.MeasureString(enemiesText) * textScale;

            // Position below the crops counter
            Vector2 cropsTextSize = _gameFont.MeasureString("Crops Left: 0") * textScale;
            Vector2 textPosition = new(
                _spriteBatch.GraphicsDevice.Viewport.Width - textSize.X - ScreenMargin,
                ScreenMargin + cropsTextSize.Y + BarSpacing
            );

            // If near zero, use red color to highlight
            Color textColor = enemiesRemaining <= 3 ? Color.Red : Color.White;

            DrawTextWithShadow(enemiesText, textPosition, textColor, textScale);

            // If only a few enemies remain, add a pulsing effect
            if (enemiesRemaining <= 3 && enemiesRemaining > 0)
            {
                // Draw attention-grabbing indicator
                float pulse = (float)Math.Sin(_itemIndicatorPulse) * 0.3f + 0.7f;
                Color indicatorColor = new Color(255, (byte)(50 * pulse), 0) * pulse;

                // Draw arrow/indicator
                Vector2 arrowLeft = textPosition + new Vector2(-25, textSize.Y / 2);
                Vector2 arrowRight = textPosition + new Vector2(-5, textSize.Y / 2);
                DrawLine(arrowLeft, arrowRight, indicatorColor, 3);
            }
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
            Vector2 mouseScreenPosition = new(mouseState.X, mouseState.Y);

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
            (int tileX, int tileY) = TilePhysicsInspector.WorldToTile(mouseWorldPosition);
            string tilePosText = $"Tile: {tileX}, {tileY}";
            DrawDebugText(tilePosText, textPosition + currentOffset, textScale);

            System.Diagnostics.Debug.WriteLine("=== Mouse Debug Info ===");
            System.Diagnostics.Debug.WriteLine($"Screen Position: ({mouseScreenPosition.X:0.0}, {mouseScreenPosition.Y:0.0})");
            System.Diagnostics.Debug.WriteLine($"World Position: ({mouseWorldPosition.X:0.0}, {mouseWorldPosition.Y:0.0})");
            System.Diagnostics.Debug.WriteLine($"Tile Position: ({tileX}, {tileY})");
            System.Diagnostics.Debug.WriteLine("=========================");
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

        public void DrawDebugInfo(GameTime gameTime, IEnumerable<Entity> entities, Matrix cameraMatrix, Vector2 playerPosition)
        {
            IEnumerable<Entity> enumerableEntity = entities as Entity[] ?? entities.ToArray();
            if (_showCollisionBounds)
            {
                DrawCollisionBounds(enumerableEntity, cameraMatrix);
            }

            if (_showEntityInfo)
            {
                DrawEntityInfo(enumerableEntity, cameraMatrix, playerPosition);
            }

            if (_showMousePosition)
            {
                DrawMousePositionDebug(gameTime, cameraMatrix);
            }
        }

        private void DrawCollisionBounds(IEnumerable<Entity> entities, Matrix cameraMatrix)
        {
            foreach (Entity entity in entities)
            {
                if (entity.CollisionBounding != null)
                {
                    switch (entity)
                    {
                        case Crop { Collected: false } crop:
                            DrawCollisionBounds(crop, entity.CollisionBounding, cameraMatrix);
                            break;
                        case Fly { Destroyed: false } fly:
                            DrawCollisionBounds(fly, entity.CollisionBounding, cameraMatrix);
                            break;
                        case Ant ant:
                            DrawCollisionBounds(ant, entity.CollisionBounding, cameraMatrix);
                            break;
                        case AntEnemy antEnemy:
                            DrawCollisionBounds(antEnemy, entity.CollisionBounding, cameraMatrix);
                            break;
                    }
                }
            }
        }

        private void DrawEntityInfo(IEnumerable<Entity> entities, Matrix cameraMatrix, Vector2 playerPosition)
        {
            foreach (Entity entity in entities)
            {
                float distanceToPlayer = Vector2.Distance(playerPosition, entity.Position);
                switch (entity)
                {
                    case Crop { Collected: false } crop:
                        DrawDebugInfo(
                            crop.Position,
                            cameraMatrix,
                            distanceToPlayer,
                            crop.CollisionBounding
                        );
                        break;
                    case Fly { Destroyed: false } fly:
                        DrawDebugInfo(
                            fly.Position,
                            cameraMatrix,
                            fly.Strategy,
                            distanceToPlayer,
                            fly.StrategyHistory,
                            fly.CollisionBounding
                        );
                        break;
                    case AntEnemy antEnemy:
                        DrawDebugInfo(
                            antEnemy.Position,
                            antEnemy.Velocity,
                            cameraMatrix,
                            antEnemy.Strategy,
                            distanceToPlayer,
                            antEnemy.StrategyHistory,
                            antEnemy.CollisionBounding
                        );
                        break;
                    case Ant ant:
                        DrawDebugInfo(
                            ant.Position,
                            cameraMatrix,
                            distanceToPlayer,
                            ant.CollisionBounding
                        );
                        break;
                }
            }
        }

        public void Dispose()
        {
            _grayTexture?.Dispose();
            _redTexture?.Dispose();
            _greenTexture?.Dispose();
            _orangeTexture?.Dispose();
            _borderTexture?.Dispose();
            _itemIndicatorTexture?.Dispose();
        }
    }
}