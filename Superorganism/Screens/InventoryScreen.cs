using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Superorganism.Core.Timing;
using Superorganism.ScreenManagement;

namespace Superorganism.Screens
{
    /// <summary>
    /// A screen that displays the player's inventory without pausing the game.
    /// </summary>
    public class InventoryScreen : GameScreen
    {
        // Add a property to explicitly indicate this screen doesn't pause the game
        public bool ShouldPauseGame { get; } = false;

        private readonly List<InventoryItem> _inventoryItems = new List<InventoryItem>();
        private Texture2D _panelBackground;
        private Rectangle _inventoryRect;
        private int _selectedItemIndex = -1;
        private SpriteFont _font;
        private const int ItemHeight = 40;
        private const int ItemPadding = 10;
        private Vector2 _scrollPosition = Vector2.Zero;
        private float _maxScroll = 0;

        private readonly InputAction _menuUp;
        private readonly InputAction _menuDown;
        private readonly InputAction _menuSelect;
        private readonly InputAction _menuCancel;
        private readonly InputAction _menuUse;

        /// <summary>
        /// Initializes a new instance of the InventoryScreen class.
        /// </summary>
        public InventoryScreen()
        {
            // Set this as a popup so the gameplay screen stays active
            IsPopup = true;

            // Set transition times
            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);

            // Define input actions
            _menuUp = new InputAction(
                new[] { Buttons.DPadUp, Buttons.LeftThumbstickUp },
                new[] { Keys.Up, Keys.W }, true);

            _menuDown = new InputAction(
                new[] { Buttons.DPadDown, Buttons.LeftThumbstickDown },
                new[] { Keys.Down, Keys.S }, true);

            _menuSelect = new InputAction(
                new[] { Buttons.A },
                new[] { Keys.Enter, Keys.Space }, true);

            _menuCancel = new InputAction(
                new[] { Buttons.B, Buttons.Back },
                new[] { Keys.Escape, Keys.I }, true);

            _menuUse = new InputAction(
                new[] { Buttons.X },
                new[] { Keys.E }, true);
        }

        /// <summary>
        /// Load graphics content for the inventory screen
        /// </summary>
        public override void Activate()
        {
            base.Activate();

            if (ScreenManager != null)
            {
                _font = ScreenManager.Font;

                // Create the panel background texture from the blank texture
                _panelBackground = ScreenManager.BlankTexture;

                // Set inventory panel size and position
                int width = 400;
                int height = 500;
                _inventoryRect = new Rectangle(
                    (ScreenManager.GraphicsDevice.Viewport.Width - width) / 2,
                    (ScreenManager.GraphicsDevice.Viewport.Height - height) / 2,
                    width, height);

                // Load player inventory items
                LoadInventoryItems();
            }
        }

        /// <summary>
        /// Loads the player's inventory items from the game state.
        /// </summary>
        private void LoadInventoryItems()
        {
            // Clear existing items
            _inventoryItems.Clear();

            // This method would normally get items from your game state
            // For now, we'll add some example items
            _inventoryItems.Add(new InventoryItem("Seeds", 5, "Plant these to grow crops"));
            _inventoryItems.Add(new InventoryItem("Water Flask", 1, "Contains fresh water for plants"));
            _inventoryItems.Add(new InventoryItem("Fertilizer", 3, "Speeds up plant growth"));
            _inventoryItems.Add(new InventoryItem("Mushroom Spores", 2, "Exotic fungal growth"));
            _inventoryItems.Add(new InventoryItem("Insect Repellent", 1, "Keeps pests away"));
            _inventoryItems.Add(new InventoryItem("Fungicide", 2, "Prevents fungal diseases"));
            _inventoryItems.Add(new InventoryItem("Growth Hormone", 1, "Accelerates plant development"));

            // Calculate max scroll based on number of items
            _maxScroll = Math.Max(0, (_inventoryItems.Count * (ItemHeight + ItemPadding)) - (_inventoryRect.Height - 100));
        }

        /// <summary>
        /// Override Update to ensure the gameplay doesn't pause
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Override timer behavior for this specific screen
            // Resume the game timer when inventory is active
            GameTimer.Resume();
        }

        /// <summary>
        /// Handles input for the inventory screen
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            // Handle cancel (close inventory)
            if (_menuCancel.Occurred(input, ControllingPlayer, out PlayerIndex playerIndex))
            {
                // Make sure the game keeps running when we exit
                GameTimer.Resume();
                ExitScreen();
                return;
            }

            // Handle navigation
            if (_menuUp.Occurred(input, ControllingPlayer, out playerIndex))
            {
                if (_selectedItemIndex > 0)
                {
                    _selectedItemIndex--;
                    AdjustScroll();
                }
                else if (_inventoryItems.Count > 0)
                {
                    // Wrap around to the bottom
                    _selectedItemIndex = _inventoryItems.Count - 1;
                    AdjustScroll();
                }
            }

            if (_menuDown.Occurred(input, ControllingPlayer, out playerIndex))
            {
                if (_selectedItemIndex < _inventoryItems.Count - 1)
                {
                    _selectedItemIndex++;
                    AdjustScroll();
                }
                else
                {
                    // Wrap around to the top
                    _selectedItemIndex = 0;
                    AdjustScroll();
                }
            }

            if (_menuSelect.Occurred(input, ControllingPlayer, out playerIndex))
            {
                if (_selectedItemIndex >= 0 && _selectedItemIndex < _inventoryItems.Count)
                {
                    // Select the item (could show a detail view or options)
                    SelectItem(_selectedItemIndex, playerIndex);
                }
            }

            if (_menuUse.Occurred(input, ControllingPlayer, out playerIndex))
            {
                if (_selectedItemIndex >= 0 && _selectedItemIndex < _inventoryItems.Count)
                {
                    // Use the selected item
                    UseItem(_selectedItemIndex, playerIndex);
                }
            }

            // Handle mouse input if supported
            if (input.IsNewMouseButtonPress(MouseButtons.Left))
            {
                HandleMouseClick(input.CurrentMouseState.Position);
            }

            // Handle mouse wheel for scrolling
            int scrollWheelDelta = input.CurrentMouseState.ScrollWheelValue - input.LastMouseState.ScrollWheelValue;
            if (scrollWheelDelta != 0 && _inventoryRect.Contains(input.CurrentMouseState.Position))
            {
                // Scroll up or down based on wheel direction
                _scrollPosition.Y -= scrollWheelDelta / 120f * 20f; // Adjust 20f for scroll speed
                _scrollPosition.Y = MathHelper.Clamp(_scrollPosition.Y, 0, _maxScroll);
            }
        }

        /// <summary>
        /// Adjusts the scroll position to ensure the selected item is visible.
        /// </summary>
        private void AdjustScroll()
        {
            // If no items or invalid selection, reset scroll position
            if (_inventoryItems.Count == 0 || _selectedItemIndex < 0)
            {
                _scrollPosition.Y = 0;
                return;
            }

            // Calculate the position of the selected item
            float itemPosition = _selectedItemIndex * (ItemHeight + ItemPadding);

            // Adjust scroll to keep the selected item in view
            if (itemPosition < _scrollPosition.Y)
            {
                _scrollPosition.Y = itemPosition;
            }
            else if (itemPosition + ItemHeight > _scrollPosition.Y + (_inventoryRect.Height - 100))
            {
                _scrollPosition.Y = itemPosition - (_inventoryRect.Height - 100) + ItemHeight;
            }

            // Clamp scroll position
            _scrollPosition.Y = MathHelper.Clamp(_scrollPosition.Y, 0, _maxScroll);
        }

        /// <summary>
        /// Handles mouse click on inventory items.
        /// </summary>
        private void HandleMouseClick(Point mousePosition)
        {
            // Check if the click is within the inventory panel
            if (_inventoryRect.Contains(mousePosition))
            {
                // Convert to local coordinates - account for header and scrolling
                int localY = mousePosition.Y - _inventoryRect.Y - 60 + (int)_scrollPosition.Y;

                // Skip if clicked in header area
                if (localY < 0)
                    return;

                // Determine which item was clicked
                int clickedIndex = localY / (ItemHeight + ItemPadding);

                if (clickedIndex >= 0 && clickedIndex < _inventoryItems.Count)
                {
                    _selectedItemIndex = clickedIndex;
                    SelectItem(_selectedItemIndex, PlayerIndex.One); // Use default player index for mouse
                }
            }
            else
            {
                // If clicking outside the inventory panel, close it
                GameTimer.Resume();
                ExitScreen();
            }
        }

        /// <summary>
        /// Selects an inventory item to view details or actions.
        /// </summary>
        private void SelectItem(int itemIndex, PlayerIndex playerIndex)
        {
            // In a real implementation, this might show item details or options
            _selectedItemIndex = itemIndex;
        }

        /// <summary>
        /// Uses the selected inventory item.
        /// </summary>
        private void UseItem(int itemIndex, PlayerIndex playerIndex)
        {
            if (itemIndex < 0 || itemIndex >= _inventoryItems.Count)
                return;

            InventoryItem item = _inventoryItems[itemIndex];

            // If the item has quantity remaining
            if (item.Quantity > 0)
            {
                // Apply the item's effect (this would connect to your game systems)
                // For example: GameState.UseItem(item.Name);

                // Reduce quantity
                item.Quantity--;

                // Remove item if quantity reaches 0
                if (item.Quantity <= 0)
                {
                    _inventoryItems.RemoveAt(itemIndex);
                    _selectedItemIndex = Math.Min(_selectedItemIndex, _inventoryItems.Count - 1);
                    _maxScroll = Math.Max(0, (_inventoryItems.Count * (ItemHeight + ItemPadding)) - (_inventoryRect.Height - 100));
                }
            }
        }

        /// <summary>
        /// Draws the inventory screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // Begin drawing
            spriteBatch.Begin();

            // Draw inventory panel background with border
            Color backColor = new Color(10, 10, 30, 230) * TransitionAlpha;
            Color borderColor = new Color(100, 100, 180, 255) * TransitionAlpha;

            // Draw the border
            Rectangle borderRect = new Rectangle(
                _inventoryRect.X - 2,
                _inventoryRect.Y - 2,
                _inventoryRect.Width + 4,
                _inventoryRect.Height + 4);
            spriteBatch.Draw(_panelBackground, borderRect, borderColor);

            // Draw the panel background
            spriteBatch.Draw(_panelBackground, _inventoryRect, backColor);

            // Draw inventory title
            string title = "Inventory";
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePosition = new Vector2(
                _inventoryRect.X + (_inventoryRect.Width - titleSize.X) / 2,
                _inventoryRect.Y + 15);

            // Draw title shadow and then title
            spriteBatch.DrawString(_font, title, titlePosition + new Vector2(2, 2), new Color(0, 0, 0, 128) * TransitionAlpha);
            spriteBatch.DrawString(_font, title, titlePosition, Color.White * TransitionAlpha);

            // End current batch to set up scissor rect for clipping
            spriteBatch.End();

            // Set up scissor rect for inventory item area
            Rectangle originalScissorRect = ScreenManager.GraphicsDevice.ScissorRectangle;
            Rectangle scissorRect = new Rectangle(
                _inventoryRect.X,
                _inventoryRect.Y + 60,
                _inventoryRect.Width,
                _inventoryRect.Height - 90);

            // Begin new batch with scissor test enabled
            RasterizerState rasterizerState = new RasterizerState { ScissorTestEnable = true };
            ScreenManager.GraphicsDevice.ScissorRectangle = scissorRect;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, rasterizerState);

            // Draw inventory items
            for (int i = 0; i < _inventoryItems.Count; i++)
            {
                InventoryItem item = _inventoryItems[i];
                bool isSelected = (i == _selectedItemIndex);

                // Calculate item position
                Rectangle itemRect = new Rectangle(
                    _inventoryRect.X + ItemPadding,
                    _inventoryRect.Y + 60 + (i * (ItemHeight + ItemPadding)) - (int)_scrollPosition.Y,
                    _inventoryRect.Width - (ItemPadding * 2),
                    ItemHeight);

                // Skip if item is outside the visible area
                if (itemRect.Bottom < _inventoryRect.Y + 60 || itemRect.Y > _inventoryRect.Bottom - 30)
                    continue;

                // Draw item background
                Color itemBackColor = isSelected ?
                    new Color(60, 60, 140, 200) * TransitionAlpha :
                    new Color(40, 40, 70, 200) * TransitionAlpha;
                spriteBatch.Draw(_panelBackground, itemRect, itemBackColor);

                // Draw item name and quantity
                string itemText = $"{item.Name} x{item.Quantity}";
                Vector2 textPos = new Vector2(itemRect.X + 10, itemRect.Y + 10);
                Color textColor = isSelected ? Color.Yellow : Color.White;

                // Draw text shadow
                spriteBatch.DrawString(_font, itemText, textPos + new Vector2(1, 1), new Color(0, 0, 0, 150) * TransitionAlpha);
                // Draw text
                spriteBatch.DrawString(_font, itemText, textPos, textColor * TransitionAlpha);
            }

            // End scissor batch
            spriteBatch.End();

            // Restore original scissor rectangle
            ScreenManager.GraphicsDevice.ScissorRectangle = originalScissorRect;

            // Draw help text
            spriteBatch.Begin();

            string helpText = "W/S: Navigate  E: Use Item  Esc: Close";
            Vector2 helpSize = _font.MeasureString(helpText);
            Vector2 helpPos = new Vector2(
                _inventoryRect.X + (_inventoryRect.Width - helpSize.X) / 2,
                _inventoryRect.Bottom - 30);

            // Draw help text shadow
            spriteBatch.DrawString(_font, helpText, helpPos + new Vector2(1, 1), new Color(0, 0, 0, 150) * TransitionAlpha);
            // Draw help text
            spriteBatch.DrawString(_font, helpText, helpPos, Color.White * TransitionAlpha);

            spriteBatch.End();
        }

        /// <summary>
        /// Overrides ExitScreen to ensure the game timer is resumed when the screen exits
        /// </summary>
        public new void ExitScreen()
        {
            // Make sure the timer is resumed when the inventory screen is closed
            GameTimer.Resume();
            base.ExitScreen();
        }
    }

    /// <summary>
    /// Represents an item in the inventory.
    /// </summary>
    public class InventoryItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }

        public InventoryItem(string name, int quantity, string description)
        {
            Name = name;
            Quantity = quantity;
            Description = description;
        }
    }
}