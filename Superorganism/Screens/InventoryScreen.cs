using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Superorganism.Common;
using Superorganism.Core.Inventory;
using Superorganism.Core.Timing;
using Superorganism.ScreenManagement;

namespace Superorganism.Screens
{
    /// <summary>
    /// A screen that displays the player's inventory without pausing the game.
    /// Features a grid-based inventory system, character display, and status panel.
    /// </summary>
    public class InventoryScreen : GameScreen
    {
        // Indicates this screen doesn't pause the game
        public bool ShouldPauseGame { get; } = false;

        // Inventory grid configuration
        private const int GridColumns = 6;
        private const int GridRows = 8;
        private int _slotSize;
        private int _slotPadding;
        private float _uiScale = 1.0f;

        // UI elements and regions
        private Texture2D _backgroundTexture;
        private Rectangle _inventoryRect;
        private Rectangle _gridRect;
        private Rectangle _characterRect;
        private Rectangle _statsRect;
        private Rectangle _titleBarRect;
        private Rectangle _resizeHandleRect;

        // Font and text rendering
        private SpriteFont _font;
        private SpriteFont _titleFont;
        private float _fontScale = 1.0f;

        // Inventory data
        private readonly List<InventoryItem> _inventoryItems = [];
        private int _selectedItemIndex = -1;

        // Status display
        private EntityStatus _playerStatus;

        // UI interaction
        private bool _isDragging;
        private bool _isResizing;
        private Point _dragStartPos;
        private Point _lastMousePos;

        // Input actions
        private readonly InputAction _menuUp;
        private readonly InputAction _menuDown;
        private readonly InputAction _menuLeft;
        private readonly InputAction _menuRight;
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
                [Buttons.DPadUp, Buttons.LeftThumbstickUp],
                [Keys.Up, Keys.W], true);

            _menuDown = new InputAction(
                [Buttons.DPadDown, Buttons.LeftThumbstickDown],
                [Keys.Down, Keys.S], true);

            _menuLeft = new InputAction(
                [Buttons.DPadLeft, Buttons.LeftThumbstickLeft],
                [Keys.Left, Keys.A], true);

            _menuRight = new InputAction(
                [Buttons.DPadRight, Buttons.LeftThumbstickRight],
                [Keys.Right, Keys.D], true);

            _menuSelect = new InputAction(
                [Buttons.A],
                [Keys.Enter, Keys.Space], true);

            _menuCancel = new InputAction(
                [Buttons.B, Buttons.Back],
                [Keys.Escape, Keys.I], true);

            _menuUse = new InputAction(
                [Buttons.X],
                [Keys.E], true);

            // Create a default entity status for when we can't get it from the game
            _playerStatus = new EntityStatus();
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
                // You may need to load a larger font for titles if available
                _titleFont = ScreenManager.Font;

                // Create the panel background texture from the blank texture
                _backgroundTexture = ScreenManager.BlankTexture;

                // Calculate UI scale based on current resolution
                CalculateUIScale();

                // Create the UI layout based on the current resolution
                CreateUILayout();

                // Try to get player status from the GameplayScreen if available
                TryGetPlayerStatus();

                // Load player inventory items
                LoadInventoryItems();

                // Register for graphics device reset which occurs on window resize
                ScreenManager.GraphicsDevice.DeviceReset += OnScreenResize;
            }
        }

        /// <summary>
        /// Calculates the UI scale factor based on the current screen resolution
        /// </summary>
        private void CalculateUIScale()
        {
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            // Base scale on a reference resolution of 1280x720
            float baseWidth = 1280f;
            float baseHeight = 720f;

            // Calculate scale factors for width and height
            float widthScale = viewport.Width / baseWidth;
            float heightScale = viewport.Height / baseHeight;

            // Use the smaller scale to ensure UI fits on screen
            _uiScale = Math.Min(widthScale, heightScale);

            // Scale font based on resolution
            _fontScale = MathHelper.Clamp(_uiScale, 0.8f, 1.5f);

            // Scale the slot size and padding based on resolution
            _slotSize = (int)(48 * _uiScale);
            _slotPadding = (int)(4 * _uiScale);

            // Call the internal scale calculation to adjust elements based on inventory size
            CalculateInternalScale();
        }

        /// <summary>
        /// Creates the UI layout based on the current screen resolution
        /// </summary>
        private void CreateUILayout()
        {
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            // Set size based on screen percentage and scale
            int width = (int)(viewport.Width * 0.8f);
            int height = (int)(viewport.Height * 0.8f);

            // Ensure minimum size
            width = Math.Max(width, (int)(800 * _uiScale));
            height = Math.Max(height, (int)(600 * _uiScale));

            // Center the inventory on screen
            _inventoryRect = new Rectangle(
                (viewport.Width - width) / 2,
                (viewport.Height - height) / 2,
                width, height);

            // Create the title bar at the top
            int titleBarHeight = (int)(30 * _uiScale); // Scale title bar height
            _titleBarRect = new Rectangle(
                _inventoryRect.X,
                _inventoryRect.Y,
                _inventoryRect.Width,
                titleBarHeight);

            // Create resize handle in bottom-right corner
            int handleSize = (int)(20 * _uiScale);
            _resizeHandleRect = new Rectangle(
                _inventoryRect.Right - handleSize,
                _inventoryRect.Bottom - handleSize,
                handleSize,
                handleSize);

            // Calculate the three main sections (grid, character, stats)
            int sectionWidth = _inventoryRect.Width / 3;

            // Create the inventory grid on the left third
            _gridRect = new Rectangle(
                _inventoryRect.X,
                _inventoryRect.Y + _titleBarRect.Height,
                sectionWidth,
                _inventoryRect.Height - _titleBarRect.Height);

            // Create character section in middle third
            _characterRect = new Rectangle(
                _inventoryRect.X + sectionWidth,
                _inventoryRect.Y + _titleBarRect.Height,
                sectionWidth,
                _inventoryRect.Height - _titleBarRect.Height);

            // Create stats section on right third
            _statsRect = new Rectangle(
                _inventoryRect.X + (sectionWidth * 2),
                _inventoryRect.Y + _titleBarRect.Height,
                sectionWidth,
                _inventoryRect.Height - _titleBarRect.Height);
        }

        /// <summary>
        /// Tries to get the player entity status from the current GameplayScreen
        /// </summary>
        private void TryGetPlayerStatus()
        {
            if (ScreenManager == null) return;

            // Find the GameplayScreen
            foreach (GameScreen screen in ScreenManager.GetScreens())
            {
                if (screen is GameplayScreen gameplayScreen && gameplayScreen.GameStateOrganizer != null)
                {
                    // This assumes GameStateOrganizer has a way to get player status
                    // You'll need to adjust this based on your actual implementation
                    _playerStatus = new EntityStatus
                    {
                        HitPoints = gameplayScreen.GameStateOrganizer.GetPlayerHealth(),
                        MaxHitPoints = gameplayScreen.GameStateOrganizer.GetPlayerMaxHealth(),
                        Stamina = gameplayScreen.GameStateOrganizer.GetPlayerStamina(),
                        MaxStamina = gameplayScreen.GameStateOrganizer.GetPlayerMaxStamina(),
                        Hunger = gameplayScreen.GameStateOrganizer.GetPlayerHunger(),
                        MaxHunger = gameplayScreen.GameStateOrganizer.GetPlayerMaxHunger()
                    };
                    break;
                }
            }
        }

        /// <summary>
        /// Loads the player's inventory items.
        /// </summary>
        private void LoadInventoryItems()
        {
            // Clear existing items
            _inventoryItems.Clear();

            // Add sample items - in a real implementation, get these from game state
            _inventoryItems.Add(new InventoryItem("Seeds", 5, "Plant these to grow crops"));
            _inventoryItems.Add(new InventoryItem("Water Flask", 1, "Contains fresh water for plants"));
            _inventoryItems.Add(new InventoryItem("Fertilizer", 3, "Speeds up plant growth"));
            _inventoryItems.Add(new InventoryItem("Mushroom Spores", 2, "Exotic fungal growth"));
            _inventoryItems.Add(new InventoryItem("Insect Repellent", 1, "Keeps pests away"));
            _inventoryItems.Add(new InventoryItem("Fungicide", 2, "Prevents fungal diseases"));
            _inventoryItems.Add(new InventoryItem("Growth Hormone", 1, "Accelerates plant development"));
            _inventoryItems.Add(new InventoryItem("Compost", 4, "Enriches soil for better yields"));
            _inventoryItems.Add(new InventoryItem("Ant Pheromones", 2, "Attract friendly ants"));
        }

        /// <summary>
        /// Override Update to ensure the gameplay doesn't pause
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Override timer behavior for this specific screen - keep the game running
            GameTimer.Resume();
        }

        /// <summary>
        /// Handles input for the inventory screen
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            MouseState currentMouse = input.CurrentMouseState;
            MouseState prevMouse = input.LastMouseState;

            // Handle mouse dragging and resizing
            HandleMouseDragAndResize(currentMouse, prevMouse);

            // Handle cancel (close inventory)
            if (_menuCancel.Occurred(input, ControllingPlayer, out PlayerIndex playerIndex))
            {
                GameTimer.Resume();
                ExitScreen();
                return;
            }

            // Handle grid navigation with keyboard/gamepad
            HandleGridNavigation(input);

            // Handle item selection/usage
            if (_menuSelect.Occurred(input, ControllingPlayer, out playerIndex))
            {
                if (_selectedItemIndex >= 0 && _selectedItemIndex < _inventoryItems.Count)
                {
                    SelectItem(_selectedItemIndex, playerIndex);
                }
            }

            if (_menuUse.Occurred(input, ControllingPlayer, out playerIndex))
            {
                if (_selectedItemIndex >= 0 && _selectedItemIndex < _inventoryItems.Count)
                {
                    UseItem(_selectedItemIndex, playerIndex);
                }
            }

            // Handle mouse click on grid slots
            if (input.IsNewMouseButtonPress(MouseButtons.Left))
            {
                HandleInventoryClick(currentMouse.Position);
            }
        }

        /// <summary>
        /// Handles mouse dragging of the inventory panel and resizing
        /// </summary>
        private void HandleMouseDragAndResize(MouseState currentMouse, MouseState prevMouse)
        {
            Point mousePos = currentMouse.Position;

            // Check for initial drag/resize start
            if (currentMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
            {
                // Check if clicking in title bar (for dragging)
                if (_titleBarRect.Contains(mousePos))
                {
                    _isDragging = true;
                    _dragStartPos = new Point(mousePos.X - _inventoryRect.X, mousePos.Y - _inventoryRect.Y);
                }
                // Check if clicking in resize handle
                else if (_resizeHandleRect.Contains(mousePos))
                {
                    _isResizing = true;
                    _dragStartPos = mousePos;
                    _lastMousePos = mousePos; // Set initial position for resize
                }
            }

            // Handle ongoing drag
            if (_isDragging && currentMouse.LeftButton == ButtonState.Pressed)
            {
                // Calculate new position
                int newX = mousePos.X - _dragStartPos.X;
                int newY = mousePos.Y - _dragStartPos.Y;

                // Keep within screen bounds
                Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
                newX = Math.Clamp(newX, 0, viewport.Width - _inventoryRect.Width);
                newY = Math.Clamp(newY, 0, viewport.Height - _inventoryRect.Height);

                // Update positions
                int deltaX = newX - _inventoryRect.X;
                int deltaY = newY - _inventoryRect.Y;

                _inventoryRect.X = newX;
                _inventoryRect.Y = newY;

                // Move all child elements
                _titleBarRect.X += deltaX;
                _titleBarRect.Y += deltaY;
                _gridRect.X += deltaX;
                _gridRect.Y += deltaY;
                _characterRect.X += deltaX;
                _characterRect.Y += deltaY;
                _statsRect.X += deltaX;
                _statsRect.Y += deltaY;
                _resizeHandleRect.X += deltaX;
                _resizeHandleRect.Y += deltaY;
            }

            // Handle ongoing resize - only if mouse has actually moved
            if (_isResizing && currentMouse.LeftButton == ButtonState.Pressed)
            {
                // Only process if mouse has actually moved
                if (mousePos.X != _lastMousePos.X || mousePos.Y != _lastMousePos.Y)
                {
                    // Calculate new size
                    int newWidth = _inventoryRect.Width + (mousePos.X - _lastMousePos.X);
                    int newHeight = _inventoryRect.Height + (mousePos.Y - _lastMousePos.Y);

                    // Enforce minimum size
                    newWidth = Math.Max((int)(300 * _uiScale), newWidth);
                    newHeight = Math.Max((int)(200 * _uiScale), newHeight);

                    // Keep within screen bounds
                    Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
                    newWidth = Math.Min(newWidth, viewport.Width - _inventoryRect.X);
                    newHeight = Math.Min(newHeight, viewport.Height - _inventoryRect.Y);

                    // Update sizes
                    _inventoryRect.Width = newWidth;
                    _inventoryRect.Height = newHeight;

                    // Update title bar width
                    _titleBarRect.Width = newWidth;

                    // Recalculate the internal UI scale based on the new window size
                    CalculateInternalScale();

                    // Recreate the layout with new dimensions
                    RecreateLayout();

                    // Position the resize handle
                    _resizeHandleRect.X = _inventoryRect.Right - (int)(20 * _uiScale);
                    _resizeHandleRect.Y = _inventoryRect.Bottom - (int)(20 * _uiScale);
                }
            }

            // Handle drag/resize end
            if (currentMouse.LeftButton == ButtonState.Released && prevMouse.LeftButton == ButtonState.Pressed)
            {
                _isDragging = false;
                _isResizing = false;
            }

            _lastMousePos = mousePos;
        }

        /// <summary>
        /// Helper method to sanitize text for SpriteFont rendering
        /// </summary>
        private string SanitizeText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Create a StringBuilder to build the sanitized string
            StringBuilder sanitized = new();

            // Only include characters that the SpriteFont supports
            foreach (char c in text)
            {
                // Check if the character is likely to be supported (basic ASCII range)
                // This is a simple approach - for more complete validation you'd need to 
                // check against the actual font characters
                if (c >= 32 && c <= 126)
                {
                    sanitized.Append(c);
                }
                else
                {
                    // Replace unsupported characters with a space
                    sanitized.Append(' ');
                }
            }

            return sanitized.ToString();
        }
        /// <summary>
        /// Calculates the internal UI scale based on current inventory panel dimensions
        /// </summary>
        /// <summary>
        /// Calculates the internal UI scale based on current inventory panel dimensions
        /// </summary>
        private void CalculateInternalScale()
        {
            // Calculate text scale based on window dimensions
            // Smaller windows should have smaller text, larger windows should have larger text
            // Base scale would be for a 800x600 inventory window
            float basePanelWidth = 1280f * _uiScale;
            float basePanelHeight = 720f * _uiScale;

            float widthScale = _inventoryRect.Width / basePanelWidth;
            float heightScale = _inventoryRect.Height / basePanelHeight;

            // Use the smaller scale to ensure text fits
            float calculatedFontScale = Math.Min(widthScale, heightScale) * _uiScale;

            // Apply smoothing to font scale changes - only change by a small percentage of the difference
            // This makes changes more gradual rather than dramatic
            float targetFontScale = MathHelper.Clamp(
                calculatedFontScale,
                0.3f, // Min font scale
                1.5f  // Max font scale
            );

            // Apply smoothing - only change by at most 15% of the difference
            // This will make the scale change more gradually during resizing
            if (_fontScale != 0)
            {
                float changeAmount = (targetFontScale - _fontScale) * 0.15f;
                _fontScale += changeAmount;
                _fontScale *= 0.5f;
            }
            else
            {
                _fontScale = targetFontScale; // First initialization
            }

            // Scale slot size based on inventory dimensions and grid configuration
            // Calculate based on available width in grid section
            int gridContentWidth = _gridRect.Width - (int)(40 * _uiScale); // Account for margins
            int gridContentHeight = _gridRect.Height - (int)(60 * _uiScale); // Account for margins and title

            // Calculate max slot size that fits in the grid
            int maxSlotWidth = (gridContentWidth / GridColumns) - _slotPadding;
            int maxSlotHeight = (gridContentHeight / GridRows) - _slotPadding;

            // Use smaller dimension to keep slots square
            int targetSlotSize = Math.Min(maxSlotWidth, maxSlotHeight);

            // Ensure minimum slot size
            targetSlotSize = Math.Max(targetSlotSize, (int)(24 * _uiScale));

            // Similarly apply smoothing to slot size changes
            if (_slotSize != 0)
            {
                int slotSizeDifference = targetSlotSize - _slotSize;
                // Apply at most 20% of the slot size difference
                _slotSize += (int)(slotSizeDifference * 0.2f);
            }
            else
            {
                _slotSize = targetSlotSize; // First initialization
            }

            // Adjust padding based on slot size
            _slotPadding = Math.Max((int)(4 * _uiScale), _slotSize / 12);
        }

        /// <summary>
        /// Recreates the internal layout after a resize operation
        /// </summary>
        /// <summary>
        /// Recreates the internal layout after a resize operation
        /// </summary>
        private void RecreateLayout()
        {
            // Create the title bar at the top with proper scaling
            int titleBarHeight = (int)(30 * _uiScale); // Scale title bar height
            _titleBarRect = new Rectangle(
                _inventoryRect.X,
                _inventoryRect.Y,
                _inventoryRect.Width,
                titleBarHeight);

            // Create resize handle in bottom-right corner with proper scaling
            int handleSize = (int)(20 * _uiScale);
            _resizeHandleRect = new Rectangle(
                _inventoryRect.Right - handleSize,
                _inventoryRect.Bottom - handleSize,
                handleSize,
                handleSize);

            // Calculate the three main sections (grid, character, stats) - exactly as in CreateUILayout
            int sectionWidth = _inventoryRect.Width / 3;

            // Create the inventory grid on the left third - same as CreateUILayout
            _gridRect = new Rectangle(
                _inventoryRect.X,
                _inventoryRect.Y + _titleBarRect.Height,
                sectionWidth,
                _inventoryRect.Height - _titleBarRect.Height);

            // Create character section in middle third - same as CreateUILayout
            _characterRect = new Rectangle(
                _inventoryRect.X + sectionWidth,
                _inventoryRect.Y + _titleBarRect.Height,
                sectionWidth,
                _inventoryRect.Height - _titleBarRect.Height);

            // Create stats section on right third - same as CreateUILayout
            _statsRect = new Rectangle(
                _inventoryRect.X + (sectionWidth * 2),
                _inventoryRect.Y + _titleBarRect.Height,
                sectionWidth,
                _inventoryRect.Height - _titleBarRect.Height);
        }

        /// <summary>
        /// Handles keyboard/gamepad navigation in the inventory grid
        /// </summary>
        private void HandleGridNavigation(InputState input)
        {
            int gridSize = GridColumns * GridRows;
            int itemCount = Math.Min(_inventoryItems.Count, gridSize);

            if (itemCount == 0) return;

            // Initialize selection if none exists
            if (_selectedItemIndex < 0)
            {
                _selectedItemIndex = 0;
                return;
            }

            bool selectionChanged = false;

            // Handle up navigation
            if (_menuUp.Occurred(input, ControllingPlayer, out _))
            {
                if (_selectedItemIndex >= GridColumns)
                {
                    _selectedItemIndex -= GridColumns;
                    selectionChanged = true;
                }
                else
                {
                    // Wrap to bottom row
                    int column = _selectedItemIndex % GridColumns;
                    int lastRowStart = ((itemCount - 1) / GridColumns) * GridColumns;
                    _selectedItemIndex = Math.Min(lastRowStart + column, itemCount - 1);
                    selectionChanged = true;
                }
            }

            // Handle down navigation
            if (_menuDown.Occurred(input, ControllingPlayer, out _))
            {
                if (_selectedItemIndex + GridColumns < itemCount)
                {
                    _selectedItemIndex += GridColumns;
                    selectionChanged = true;
                }
                else
                {
                    // Wrap to top row
                    int column = _selectedItemIndex % GridColumns;
                    _selectedItemIndex = column;
                    selectionChanged = true;
                }
            }

            // Handle left navigation
            if (_menuLeft.Occurred(input, ControllingPlayer, out _))
            {
                if (_selectedItemIndex % GridColumns > 0)
                {
                    _selectedItemIndex--;
                    selectionChanged = true;
                }
                else
                {
                    // Wrap to end of row
                    int row = _selectedItemIndex / GridColumns;
                    int lastInRow = Math.Min((row + 1) * GridColumns - 1, itemCount - 1);
                    _selectedItemIndex = lastInRow;
                    selectionChanged = true;
                }
            }

            // Handle right navigation
            if (_menuRight.Occurred(input, ControllingPlayer, out _))
            {
                if (_selectedItemIndex % GridColumns < GridColumns - 1 && _selectedItemIndex < itemCount - 1)
                {
                    _selectedItemIndex++;
                    selectionChanged = true;
                }
                else
                {
                    // Wrap to start of row
                    int row = _selectedItemIndex / GridColumns;
                    _selectedItemIndex = row * GridColumns;
                    selectionChanged = true;
                }
            }

            // Ensure selection is within bounds
            if (selectionChanged)
            {
                _selectedItemIndex = Math.Clamp(_selectedItemIndex, 0, itemCount - 1);
            }
        }

        /// <summary>
        /// Handles mouse click on inventory grid slots
        /// </summary>
        private void HandleInventoryClick(Point mousePosition)
        {
            // Check for click outside inventory
            if (!_inventoryRect.Contains(mousePosition))
            {
                ExitScreen();
                return;
            }

            // Check for click in grid area
            if (_gridRect.Contains(mousePosition))
            {
                // Calculate grid metrics (same as in DrawInventoryGrid)
                int totalSlotSize = _slotSize + _slotPadding;
                int gridWidth = totalSlotSize * GridColumns + _slotPadding;
                int gridHeight = totalSlotSize * GridRows + _slotPadding;

                // Calculate grid start position (same as in DrawInventoryGrid)
                int gridStartX = _gridRect.X + (_gridRect.Width - gridWidth) / 2;
                int gridStartY = _gridRect.Y + (int)(10 * _uiScale);

                // Calculate slot position relative to grid start
                int relX = mousePosition.X - gridStartX;
                int relY = mousePosition.Y - gridStartY;

                // Calculate column and row
                int col = relX / totalSlotSize;
                int row = relY / totalSlotSize;

                // Ensure within grid bounds
                if (col >= 0 && col < GridColumns && row >= 0 && row < GridRows)
                {
                    int index = (row * GridColumns) + col;

                    // Make sure we don't select beyond the actual items
                    if (index < _inventoryItems.Count)
                    {
                        _selectedItemIndex = index;
                        SelectItem(index, PlayerIndex.One);
                    }
                }
            }
        }

        /// <summary>
        /// Selects an inventory item to view details
        /// </summary>
        private void SelectItem(int itemIndex, PlayerIndex playerIndex)
        {
            // Just update selection - details are shown in the stats panel
            _selectedItemIndex = itemIndex;
        }

        /// <summary>
        /// Uses the selected inventory item
        /// </summary>
        private void UseItem(int itemIndex, PlayerIndex playerIndex)
        {
            if (itemIndex < 0 || itemIndex >= _inventoryItems.Count)
                return;

            InventoryItem item = _inventoryItems[itemIndex];

            if (item.Quantity > 0)
            {
                // Apply effect (connect to your game systems)

                // Reduce quantity
                item.Quantity--;

                // Remove if depleted
                if (item.Quantity <= 0)
                {
                    _inventoryItems.RemoveAt(itemIndex);
                    _selectedItemIndex = Math.Min(_selectedItemIndex, _inventoryItems.Count - 1);
                }
            }
        }

        /// <summary>
        /// Draws the inventory screen
        /// </summary>
        /// <summary>
        /// Draws the inventory screen
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // Begin drawing with alpha blending
            spriteBatch.Begin();

            // Draw main inventory panel with semi-transparency
            Color panelColor = new Color(10, 10, 30, 180) * TransitionAlpha;
            Color borderColor = new Color(100, 100, 180, 200) * TransitionAlpha;
            Color titleBarColor = new Color(60, 60, 100, 200) * TransitionAlpha;

            // Draw main panel background
            spriteBatch.Draw(_backgroundTexture, _inventoryRect, panelColor);

            // Draw title bar
            spriteBatch.Draw(_backgroundTexture, _titleBarRect, titleBarColor);

            // Draw title - sanitize text to prevent font errors
            string title = SanitizeText("Inventory");
            Vector2 titleSize = _titleFont.MeasureString(title) * _fontScale;
            Vector2 titlePos = new(
                _titleBarRect.X + (_titleBarRect.Width - titleSize.X) / 2,
                _titleBarRect.Y + (_titleBarRect.Height - titleSize.Y) / 2);
            spriteBatch.DrawString(_titleFont, title, titlePos, Color.White * TransitionAlpha,
                0f, Vector2.Zero, _fontScale, SpriteEffects.None, 0f);

            // Draw the resize handle
            spriteBatch.Draw(_backgroundTexture, _resizeHandleRect, borderColor);

            // Draw section dividers
            // (code unchanged)

            // Draw inventory grid slots
            DrawInventoryGrid(spriteBatch);

            // Draw character display
            DrawCharacterDisplay(spriteBatch);

            // Draw stats panel
            DrawStatsPanel(spriteBatch);

            // Draw selected item details if any
            DrawSelectedItemDetails(spriteBatch);

            // Draw help text at bottom - sanitize text
            string helpText = SanitizeText("WASD/Arrows: Navigate  E: Use Item  Esc/I: Close");
            Vector2 helpSize = _font.MeasureString(helpText) * _fontScale;
            Vector2 helpPos = new(
                _inventoryRect.X + (_inventoryRect.Width - helpSize.X) / 2,
                _inventoryRect.Bottom - helpSize.Y - 5);

            spriteBatch.DrawString(_font, helpText, helpPos, Color.White * TransitionAlpha,
                0f, Vector2.Zero, _fontScale, SpriteEffects.None, 0f);

            spriteBatch.End();
        }

        /// <summary>
        /// Draws the inventory grid on the left panel
        /// </summary>
        private void DrawInventoryGrid(SpriteBatch spriteBatch)
        {
            // Border color for slots
            Color slotBorder = new Color(70, 70, 100, 200) * TransitionAlpha;
            Color slotFill = new Color(40, 40, 60, 180) * TransitionAlpha;
            Color selectedSlotFill = new Color(60, 60, 120, 220) * TransitionAlpha;

            // Calculate grid metrics
            int totalSlotSize = _slotSize + _slotPadding;
            int gridWidth = totalSlotSize * GridColumns + _slotPadding;
            int gridHeight = totalSlotSize * GridRows + _slotPadding;

            // Center the grid in the grid section
            int gridStartX = _gridRect.X + (_gridRect.Width - gridWidth) / 2;
            int gridStartY = _gridRect.Y + (int)(10 * _uiScale); // Small top margin

            // Draw grid title - sanitize text
            string gridTitle = SanitizeText("Items");
            Vector2 titleSize = _font.MeasureString(gridTitle) * _fontScale;
            spriteBatch.DrawString(_font, gridTitle,
                new Vector2(gridStartX, gridStartY - titleSize.Y - 5),
                Color.White * TransitionAlpha,
                0f, Vector2.Zero, _fontScale, SpriteEffects.None, 0f);

            // Draw all grid slots
            for (int row = 0; row < GridRows; row++)
            {
                for (int col = 0; col < GridColumns; col++)
                {
                    int index = (row * GridColumns) + col;

                    // Calculate slot position
                    Rectangle slotRect = new(
                        gridStartX + (col * totalSlotSize) + _slotPadding,
                        gridStartY + (row * totalSlotSize) + _slotPadding,
                        _slotSize,
                        _slotSize);

                    // Determine if this slot is selected
                    bool isSelected = (index == _selectedItemIndex);

                    // Draw slot background
                    spriteBatch.Draw(_backgroundTexture, slotRect,
                        isSelected ? selectedSlotFill : slotFill);

                    // Draw slot border
                    DrawRectangleBorder(spriteBatch, slotRect, slotBorder, 1);

                    // Draw item in slot if one exists
                    if (index < _inventoryItems.Count)
                    {
                        InventoryItem item = _inventoryItems[index];

                        // Calculate scaled font size and position for names
                        float itemFontScale = _fontScale * 0.8f; // Slightly smaller for items

                        // Sanitize item name
                        string itemName = SanitizeText(item.Name);

                        // Draw item name (or could be an icon in a real implementation)
                        Vector2 textSize = _font.MeasureString(itemName) * itemFontScale;
                        if (textSize.X > _slotSize - 4)
                        {
                            // Shorten name if too long
                            string shortName = SanitizeText(itemName.Length > 3 ?
                                itemName.Substring(0, 3) + ".." : itemName);
                            textSize = _font.MeasureString(shortName) * itemFontScale;

                            spriteBatch.DrawString(_font, shortName,
                                new Vector2(slotRect.X + (_slotSize - textSize.X) / 2,
                                    slotRect.Y + 5),
                                Color.White * TransitionAlpha,
                                0f, Vector2.Zero, itemFontScale, SpriteEffects.None, 0f);
                        }
                        else
                        {
                            spriteBatch.DrawString(_font, itemName,
                                new Vector2(slotRect.X + (_slotSize - textSize.X) / 2,
                                    slotRect.Y + 5),
                                Color.White * TransitionAlpha,
                                0f, Vector2.Zero, itemFontScale, SpriteEffects.None, 0f);
                        }

                        // Draw quantity - sanitize quantity text
                        string qtyText = SanitizeText("x" + item.Quantity.ToString());
                        Vector2 qtySize = _font.MeasureString(qtyText) * itemFontScale;

                        spriteBatch.DrawString(_font, qtyText,
                            new Vector2(slotRect.X + _slotSize - qtySize.X - 2,
                                slotRect.Y + _slotSize - qtySize.Y - 2),
                            Color.Yellow * TransitionAlpha,
                            0f, Vector2.Zero, itemFontScale, SpriteEffects.None, 0f);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the character display in the middle panel
        /// </summary>
        private void DrawCharacterDisplay(SpriteBatch spriteBatch)
        {
            // Draw section title
            string title = "Character";
            Vector2 titleSize = _font.MeasureString(title) * _fontScale;
            spriteBatch.DrawString(_font, title,
                new Vector2(_characterRect.X + (_characterRect.Width - titleSize.X) / 2,
                    _characterRect.Y + (int)(10 * _uiScale)),
                Color.White * TransitionAlpha,
                0f, Vector2.Zero, _fontScale, SpriteEffects.None, 0f);

            // Draw character silhouette or model
            // For now we'll just draw a placeholder rectangle
            Rectangle characterImageRect = new(
                _characterRect.X + (int)(20 * _uiScale),
                _characterRect.Y + (int)(40 * _uiScale),
                _characterRect.Width - (int)(40 * _uiScale),
                _characterRect.Height - (int)(80 * _uiScale));

            Color characterBg = new Color(50, 50, 80, 150) * TransitionAlpha;
            spriteBatch.Draw(_backgroundTexture, characterImageRect, characterBg);

            // Draw border
            DrawRectangleBorder(spriteBatch, characterImageRect,
                new Color(100, 100, 150, 200) * TransitionAlpha, (int)(2 * _uiScale));

            // Draw character type text
            string charType = "Ant Worker";
            Vector2 typeSize = _font.MeasureString(charType) * _fontScale;
            spriteBatch.DrawString(_font, charType,
                new Vector2(_characterRect.X + (_characterRect.Width - typeSize.X) / 2,
                    characterImageRect.Bottom + (int)(10 * _uiScale)),
                Color.White * TransitionAlpha,
                0f, Vector2.Zero, _fontScale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws the stats panel in the right section
        /// </summary>
        private void DrawStatsPanel(SpriteBatch spriteBatch)
        {
            // Draw section title
            string title = "Statistics";
            Vector2 titleSize = _font.MeasureString(title) * _fontScale;
            spriteBatch.DrawString(_font, title,
                new Vector2(_statsRect.X + (_statsRect.Width - titleSize.X) / 2,
                    _statsRect.Y + (int)(10 * _uiScale)),
                Color.White * TransitionAlpha,
                0f, Vector2.Zero, _fontScale, SpriteEffects.None, 0f);

            // Starting position for stats
            Vector2 statPos = new(_statsRect.X + (int)(20 * _uiScale), _statsRect.Y + (int)(50 * _uiScale));
            float lineHeight = _font.LineSpacing * _fontScale * 1.2f;

            // Draw Health
            DrawStatBar(spriteBatch, statPos, "Health",
                _playerStatus.HitPoints, _playerStatus.MaxHitPoints,
                Color.Red);
            statPos.Y += lineHeight;

            // Draw Stamina
            DrawStatBar(spriteBatch, statPos, "Stamina",
                _playerStatus.Stamina, _playerStatus.MaxStamina,
                Color.Green);
            statPos.Y += lineHeight;

            // Draw Hunger
            DrawStatBar(spriteBatch, statPos, "Hunger",
                _playerStatus.Hunger, _playerStatus.MaxHunger,
                Color.Yellow);
            statPos.Y += lineHeight * 1.5f;

            // Draw attributes
            spriteBatch.DrawString(_font, $"Strength: {_playerStatus.Strength}",
                statPos, Color.White * TransitionAlpha,
                0f, Vector2.Zero, _fontScale, SpriteEffects.None, 0f);
            statPos.Y += lineHeight;

            spriteBatch.DrawString(_font, $"Agility: {_playerStatus.Agility}",
                statPos, Color.White * TransitionAlpha,
                0f, Vector2.Zero, _fontScale, SpriteEffects.None, 0f);
            statPos.Y += lineHeight;

            spriteBatch.DrawString(_font, $"Endurance: {_playerStatus.Endurance}",
                statPos, Color.White * TransitionAlpha,
                0f, Vector2.Zero, _fontScale, SpriteEffects.None, 0f);
            statPos.Y += lineHeight;

            spriteBatch.DrawString(_font, $"Intelligence: {_playerStatus.Intelligence}",
                statPos, Color.White * TransitionAlpha,
                0f, Vector2.Zero, _fontScale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a stat bar with label, current value, and colored fill bar
        /// </summary>
        private void DrawStatBar(SpriteBatch spriteBatch, Vector2 position, string label,
                                float currentValue, float maxValue, Color barColor)
        {
            // Draw label
            spriteBatch.DrawString(_font, label, position, Color.White * TransitionAlpha,
                0f, Vector2.Zero, _fontScale, SpriteEffects.None, 0f);

            // Calculate bar position and size
            int barWidth = _statsRect.Width - (int)(40 * _uiScale);
            int barHeight = (int)(16 * _uiScale);
            Rectangle barBg = new(
                (int)position.X,
                (int)position.Y + (int)(_font.LineSpacing * _fontScale),
                barWidth,
                barHeight);

            // Draw background
            Color bgColor = new Color(40, 40, 60, 150) * TransitionAlpha;
            spriteBatch.Draw(_backgroundTexture, barBg, bgColor);

            // Draw filled portion
            float fillPercent = maxValue > 0 ? (currentValue / maxValue) : 0;
            fillPercent = Math.Clamp(fillPercent, 0, 1);

            Rectangle barFill = new(
                barBg.X,
                barBg.Y,
                (int)(barBg.Width * fillPercent),
                barBg.Height);

            spriteBatch.Draw(_backgroundTexture, barFill, barColor * TransitionAlpha);

            // Draw border
            DrawRectangleBorder(spriteBatch, barBg, Color.White * TransitionAlpha, 1);

            // Draw text values
            string valueText = $"{(int)currentValue}/{(int)maxValue}";
            Vector2 textSize = _font.MeasureString(valueText) * (_fontScale * 0.9f); // Slightly smaller to fit in bar
            Vector2 textPos = new(
                barBg.X + (barBg.Width - textSize.X) / 2,
                barBg.Y + (barBg.Height - textSize.Y) / 2);

            // Draw with drop shadow for better visibility
            spriteBatch.DrawString(_font, valueText, textPos + new Vector2(1, 1),
                Color.Black * TransitionAlpha,
                0f, Vector2.Zero, _fontScale * 0.9f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font, valueText, textPos,
                Color.White * TransitionAlpha,
                0f, Vector2.Zero, _fontScale * 0.9f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws details of the selected item in the stats panel
        /// </summary>
        private void DrawSelectedItemDetails(SpriteBatch spriteBatch)
        {
            // Only draw if an item is selected
            if (_selectedItemIndex < 0 || _selectedItemIndex >= _inventoryItems.Count)
                return;

            InventoryItem item = _inventoryItems[_selectedItemIndex];

            // Draw in a panel at the bottom of the stats section
            Rectangle detailsRect = new(
                _statsRect.X + (int)(10 * _uiScale),
                _statsRect.Bottom - (int)(140 * _uiScale),
                _statsRect.Width - (int)(20 * _uiScale),
                (int)(130 * _uiScale));

            // Draw panel background
            Color detailsBg = new Color(50, 50, 80, 180) * TransitionAlpha;
            spriteBatch.Draw(_backgroundTexture, detailsRect, detailsBg);

            // Draw border
            DrawRectangleBorder(spriteBatch, detailsRect,
                new Color(100, 100, 150, 200) * TransitionAlpha, 1);

            // Draw item details
            Vector2 textPos = new(detailsRect.X + (int)(10 * _uiScale), detailsRect.Y + (int)(10 * _uiScale));
            float detailsFontScale = _fontScale * 0.9f; // Slightly smaller for details

            // Draw item name
            spriteBatch.DrawString(_font, item.Name, textPos,
                Color.Yellow * TransitionAlpha,
                0f, Vector2.Zero, detailsFontScale, SpriteEffects.None, 0f);
            textPos.Y += _font.LineSpacing * detailsFontScale;

            // Draw quantity
            spriteBatch.DrawString(_font, $"Quantity: {item.Quantity}", textPos,
                Color.White * TransitionAlpha,
                0f, Vector2.Zero, detailsFontScale, SpriteEffects.None, 0f);
            textPos.Y += _font.LineSpacing * detailsFontScale;

            // Draw description - we'll need to wrap text for longer descriptions
            string description = item.Description;
            float maxWidth = detailsRect.Width - (int)(20 * _uiScale);

            List<string> lines = WrapText(description, _font, maxWidth / detailsFontScale);
            foreach (string line in lines)
            {
                spriteBatch.DrawString(_font, line, textPos,
                    Color.White * TransitionAlpha,
                    0f, Vector2.Zero, detailsFontScale, SpriteEffects.None, 0f);
                textPos.Y += _font.LineSpacing * detailsFontScale;
            }
        }

        /// <summary>
        /// Draws a rectangle border
        /// </summary>
        private void DrawRectangleBorder(SpriteBatch spriteBatch, Rectangle rect,
            Color color, int thickness)
        {
            // Top
            spriteBatch.Draw(_backgroundTexture,
                new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);

            // Bottom
            spriteBatch.Draw(_backgroundTexture,
                new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);

            // Left
            spriteBatch.Draw(_backgroundTexture,
                new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);

            // Right
            spriteBatch.Draw(_backgroundTexture,
                new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }

        /// <summary>
        /// Wraps text to fit within a specified width
        /// </summary>
        private List<string> WrapText(string text, SpriteFont font, float maxWidth)
        {
            List<string> lines = new();

            if (string.IsNullOrEmpty(text))
                return lines;

            string[] words = text.Split(' ');
            string currentLine = words[0];

            for (int i = 1; i < words.Length; i++)
            {
                string word = words[i];
                Vector2 size = font.MeasureString(currentLine + " " + word);

                if (size.X <= maxWidth)
                {
                    currentLine += " " + word;
                }
                else
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);

            return lines;
        }

        /// <summary>
        /// Overrides ExitScreen to ensure the game timer is resumed when the screen exits
        /// </summary>
        public new void ExitScreen()
        {
            // Unregister device reset event
            if (ScreenManager != null)
            {
                ScreenManager.GraphicsDevice.DeviceReset -= OnScreenResize;
            }

            // Make sure the timer is resumed when the inventory screen is closed
            GameTimer.Resume();
            base.ExitScreen();
        }

        /// <summary>
        /// Called when the screen is resized to update UI layout
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        public void OnScreenResize(object sender, EventArgs e)
        {
            // Only process when the screen is active
            if (ScreenState != ScreenState.Hidden && ScreenManager != null)
            {
                // Recalculate UI scale based on new resolution
                CalculateUIScale();

                // Recreate UI layout while preserving position and relative size
                // Store current position and relative size before recreating layout
                Point currentPosition = new(_inventoryRect.X, _inventoryRect.Y);
                float widthRatio = (float)_inventoryRect.Width / ScreenManager.GraphicsDevice.Viewport.Width;
                float heightRatio = (float)_inventoryRect.Height / ScreenManager.GraphicsDevice.Viewport.Height;

                // Recreate base layout
                CreateUILayout();

                // Apply stored position
                int deltaX = currentPosition.X - _inventoryRect.X;
                int deltaY = currentPosition.Y - _inventoryRect.Y;

                // Move inventory and all child elements
                _inventoryRect.X = currentPosition.X;
                _inventoryRect.Y = currentPosition.Y;
                _titleBarRect.X += deltaX;
                _titleBarRect.Y += deltaY;
                _gridRect.X += deltaX;
                _gridRect.Y += deltaY;
                _characterRect.X += deltaX;
                _characterRect.Y += deltaY;
                _statsRect.X += deltaX;
                _statsRect.Y += deltaY;
                _resizeHandleRect.X += deltaX;
                _resizeHandleRect.Y += deltaY;

                // Apply stored size ratio
                int targetWidth = (int)(ScreenManager.GraphicsDevice.Viewport.Width * widthRatio);
                int targetHeight = (int)(ScreenManager.GraphicsDevice.Viewport.Height * heightRatio);

                // Store current dimensions
                int oldWidth = _inventoryRect.Width;
                int oldHeight = _inventoryRect.Height;

                // Update to new dimensions
                _inventoryRect.Width = targetWidth;
                _inventoryRect.Height = targetHeight;

                // Recalculate internal scale
                CalculateInternalScale();

                // Recreate internal layout with new dimensions
                RecreateLayout();

                // Update resize handle position
                _resizeHandleRect.X = _inventoryRect.Right - (int)(20 * _uiScale);
                _resizeHandleRect.Y = _inventoryRect.Bottom - (int)(20 * _uiScale);
            }
        }
    }
}