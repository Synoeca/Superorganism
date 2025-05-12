using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Superorganism.Common;
using Superorganism.Core.Inventory;
using Superorganism.Core.Timing;
using Superorganism.Entities;
using Superorganism.ScreenManagement;
using GameTimer = Superorganism.Core.Timing.GameTimer;

namespace Superorganism.Screens
{
    /// <summary>
    /// A screen that displays the player's inventory without pausing the game.
    /// Features a grid-based inventory system, character display, and status panel.
    /// </summary>
    public class InventoryScreen : GameScreen
    {
        // Indicates this screen doesn't pause the game

        // Inventory grid configuration
        private const int GridColumns = 6;
        private const int GridRows = 8;
        private int _slotSize;
        private int _slotPadding;
        private float _uiScale = 1.0f;

        // UI elements and regions
        private Texture2D _backgroundTexture;
        private Rectangle _defaultWindowRect;
        private Rectangle _inventoryRect;
        private Rectangle _gridRect;
        private Rectangle _characterRect;
        private Rectangle _statsRect;
        private Rectangle _titleBarRect;
        private Rectangle _resizeHandleRect;

        // Window control buttons
        private Rectangle _closeButtonRect;
        private Rectangle _maximizeButtonRect;
        private Rectangle _minimizeButtonRect;
        private bool _isMinimized = false;
        private bool _isMaximized = false;
        private Rectangle _savedWindowRect; // Stores original size/position when maximized

        // Font and text rendering
        private SpriteFont _font;
        private SpriteFont _titleFont;
        private float _fontScale = 1.0f;

        // Inventory data
        private readonly List<InventoryItem> _inventoryItems = [];
        private Inventory _playerInventory;
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
            const float baseWidth = 1280f;
            const float baseHeight = 720f;

            // Calculate scale factors for width and height
            float widthScale = viewport.Width / baseWidth;
            float heightScale = viewport.Height / baseHeight;

            // Use the smaller scale to ensure UI fits on screen
            _uiScale = Math.Min(widthScale, heightScale);

            // Scale font based on resolution, but use smaller values for fonts only
            // Reduce font scale by about 40% compared to UI scale
            _fontScale = MathHelper.Clamp(_uiScale * 0.6f, 0.5f, 0.9f); // Smaller font scale range

            // Keep original slot size and padding (no change here)
            _slotSize = (int)(48 * _uiScale);
            _slotPadding = (int)(4 * _uiScale);
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

            // Store the default window state when first created
            _defaultWindowRect = _inventoryRect;

            // Create the title bar at the top
            int titleBarHeight = (int)(30 * _uiScale); // Scale title bar height
            _titleBarRect = new Rectangle(
                _inventoryRect.X,
                _inventoryRect.Y,
                _inventoryRect.Width,
                titleBarHeight);

            // Create window control buttons
            int buttonSize = (int)(24 * _uiScale);
            int buttonPadding = (int)(4 * _uiScale);

            // Close button (X) at the far right
            _closeButtonRect = new Rectangle(
                _titleBarRect.Right - buttonSize - buttonPadding,
                _titleBarRect.Y + ((_titleBarRect.Height - buttonSize) / 2),
                buttonSize,
                buttonSize);

            // Maximize button to the left of close button
            _maximizeButtonRect = new Rectangle(
                _closeButtonRect.X - buttonSize - buttonPadding,
                _closeButtonRect.Y,
                buttonSize,
                buttonSize);

            // Minimize button to the left of maximize button
            _minimizeButtonRect = new Rectangle(
                _maximizeButtonRect.X - buttonSize - buttonPadding,
                _closeButtonRect.Y,
                buttonSize,
                buttonSize);

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

            foreach (GameScreen screen in ScreenManager.GetScreens())
            {
                if (screen is GameplayScreen gameplayScreen && gameplayScreen.GameStateOrganizer != null)
                {
                    // Get player entity status
                    _playerStatus = gameplayScreen.GameStateOrganizer.GetPlayerEntityStatus;

                    // Get player entity and its inventory
                    Ant playerEntity = gameplayScreen.GameStateOrganizer.GetPlayerAnt();
                    if (playerEntity != null)
                    {
                        // Unsubscribe from previous inventory if any
                        if (_playerInventory != null)
                        {
                            _playerInventory.CollectionChanged -= OnInventoryChanged;
                        }

                        // Store reference to player's inventory and subscribe to changes
                        _playerInventory = playerEntity.Inventory;
                        _playerInventory.CollectionChanged += OnInventoryChanged;

                        // Initialize inventory items from player inventory
                        LoadInventoryItems();
                    }

                    break;
                }
            }
        }

        // Handle inventory changes
        private void OnInventoryChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Reload inventory items to reflect changes
            LoadInventoryItems();
        }

        /// <summary>
        /// Loads the player's inventory items.
        /// </summary>
        private void LoadInventoryItems()
        {
            _inventoryItems.Clear();

            if (_playerInventory != null)
            {
                // Add items from player's inventory
                foreach (InventoryItem item in _playerInventory)
                {
                    _inventoryItems.Add(item);
                }
            }
            else
            {
                // Fallback to sample items if no player inventory available
                _inventoryItems.Add(new InventoryItem("Seeds", 5, "Plant these to grow crops"));
                _inventoryItems.Add(new InventoryItem("Water Flask", 1, "Contains fresh water for plants"));
            }
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

            // Handle window control buttons (X, maximize, minimize)
            if (input.IsNewMouseButtonPress(MouseButtons.Left))
            {
                Point mousePos = currentMouse.Position;

                // Check if close button clicked
                if (_closeButtonRect.Contains(mousePos))
                {
                    GameTimer.Resume();
                    ExitScreen();
                    return;
                }

                // Check if maximize button clicked
                if (_maximizeButtonRect.Contains(mousePos))
                {
                    if (_isMinimized)
                    {
                        // If minimized, restore to default first, then maximize
                        _isMinimized = false;
                        _inventoryRect = _defaultWindowRect;
                        ToggleMaximize();
                    }
                    else if (_isMaximized)
                    {
                        // If already maximized, go back to default state
                        _isMaximized = false;
                        _inventoryRect = _defaultWindowRect;
                    }
                    else
                    {
                        // If in normal state, save current state and maximize
                        _savedWindowRect = _inventoryRect;
                        ToggleMaximize();
                    }

                    // Recalculate layout after state change
                    CalculateInternalScale();
                    RecreateLayout();
                    return;
                }

                // Check if minimize button clicked
                if (_minimizeButtonRect.Contains(mousePos))
                {
                    if (_isMaximized)
                    {
                        // If maximized, restore to default first, then minimize
                        _isMaximized = false;
                        _inventoryRect = _defaultWindowRect;
                        MinimizeWindow();
                    }
                    else if (_isMinimized)
                    {
                        // If already minimized, go back to default state
                        _isMinimized = false;
                        _inventoryRect = _defaultWindowRect;
                    }
                    else
                    {
                        // If in normal state, save current state and minimize
                        _savedWindowRect = _inventoryRect;
                        MinimizeWindow();
                    }

                    // Recalculate layout after state change
                    CalculateInternalScale();
                    RecreateLayout();
                    return;
                }
            }

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
        /// Toggles between maximized and regular window state
        /// </summary>
        private void ToggleMaximize()
        {
            if (_isMaximized)
            {
                // Restore to saved dimensions (the state before maximizing)
                _inventoryRect = _savedWindowRect;
                _isMaximized = false;
            }
            else
            {
                // Save current dimensions if not already saved
                if (!_isMinimized)
                {
                    _savedWindowRect = _inventoryRect;
                }

                // Maximize to fill most of the screen
                Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
                _inventoryRect = new Rectangle(
                    (int)(viewport.Width * 0.05f),  // 5% margin
                    (int)(viewport.Height * 0.05f), // 5% margin
                    (int)(viewport.Width * 0.9f),   // 90% of screen width
                    (int)(viewport.Height * 0.9f)); // 90% of screen height

                _isMaximized = true;
                _isMinimized = false;
            }

            // Recalculate UI scale for the new size
            CalculateInternalScale();

            // Recreate the layout with new dimensions
            RecreateLayout();
        }

        /// <summary>
        /// Minimizes the window to just show the title bar
        /// </summary>
        private void MinimizeWindow()
        {
            if (_isMinimized)
            {
                // Restore to saved dimensions
                _inventoryRect = _savedWindowRect;
                _isMinimized = false;
            }
            else
            {
                // Save current dimensions if not already saved
                if (!_isMaximized)
                {
                    _savedWindowRect = _inventoryRect;
                }

                // Shrink to a minimal size (just the title bar + small margin)
                int minWidth = Math.Max((int)(300 * _uiScale), _titleBarRect.Width);
                int oldX = _inventoryRect.X;
                int oldY = _inventoryRect.Y;

                _inventoryRect.Width = minWidth;
                _inventoryRect.Height = (int)(40 * _uiScale); // Title bar + small margin

                // Keep the same position
                _inventoryRect.X = oldX;
                _inventoryRect.Y = oldY;

                _isMinimized = true;
                _isMaximized = false;
            }

            // Recalculate UI scale for the new size
            CalculateInternalScale();

            // Recreate the layout with new dimensions
            RecreateLayout();
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
                if (_titleBarRect.Contains(mousePos) &&
                    !_closeButtonRect.Contains(mousePos) &&
                    !_maximizeButtonRect.Contains(mousePos) &&
                    !_minimizeButtonRect.Contains(mousePos))
                {
                    _isDragging = true;
                    _dragStartPos = new Point(mousePos.X - _inventoryRect.X, mousePos.Y - _inventoryRect.Y);

                    if (_isMaximized)
                    {
                        _isMaximized = false;
                        // Set a reasonable restored size
                        int width = _savedWindowRect.Width;
                        int height = _savedWindowRect.Height;

                        // Position under the mouse cursor
                        int newX = mousePos.X - (width / 2);
                        int newY = mousePos.Y - (int)(15 * _uiScale); // Half of title bar height

                        _inventoryRect.Width = width;
                        _inventoryRect.Height = height;
                        _inventoryRect.X = newX;
                        _inventoryRect.Y = newY;

                        // Recalculate layout
                        RecreateLayout();
                    }
                }
                // Check if clicking in resize handle
                else if (_resizeHandleRect.Contains(mousePos))
                {
                    _isResizing = true;
                    _dragStartPos = mousePos;
                    _lastMousePos = mousePos; // Set initial position for resize

                    if (_isMaximized)
                    {
                        _isMaximized = false;
                    }
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
                _closeButtonRect.X += deltaX;
                _closeButtonRect.Y += deltaY;
                _maximizeButtonRect.X += deltaX;
                _maximizeButtonRect.Y += deltaY;
                _minimizeButtonRect.X += deltaX;
                _minimizeButtonRect.Y += deltaY;
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
        private void CalculateInternalScale()
        {
            // Calculate text scale based on window dimensions
            // Base scale would be for a 800x600 inventory window
            float basePanelWidth = 800f * _uiScale;
            float basePanelHeight = 600f * _uiScale;

            float widthScale = _inventoryRect.Width / basePanelWidth;
            float heightScale = _inventoryRect.Height / basePanelHeight;

            // Use the smaller scale to ensure text fits
            float calculatedScale = Math.Min(widthScale, heightScale) * _uiScale;

            // Apply the smaller font scale factor (0.6) to maintain consistency
            float targetFontScale = MathHelper.Clamp(calculatedScale * 0.6f, 0.5f, 0.9f);

            // Apply smoothing for gradual changes
            if (_fontScale != 0)
            {
                float changeAmount = (targetFontScale - _fontScale) * 0.3f;
                _fontScale += changeAmount;
            }
            else
            {
                _fontScale = targetFontScale;
            }

            // Keep original slot size calculation logic
            int gridContentWidth = _gridRect.Width - (int)(40 * _uiScale);
            int gridContentHeight = _gridRect.Height - (int)(60 * _uiScale);

            int maxSlotWidth = (gridContentWidth / GridColumns) - _slotPadding;
            int maxSlotHeight = (gridContentHeight / GridRows) - _slotPadding;

            int targetSlotSize = Math.Min(maxSlotWidth, maxSlotHeight);
            targetSlotSize = Math.Max(targetSlotSize, (int)(24 * _uiScale));

            if (_slotSize != 0)
            {
                int slotSizeDifference = targetSlotSize - _slotSize;
                _slotSize += (int)(slotSizeDifference * 0.3f);
            }
            else
            {
                _slotSize = targetSlotSize;
            }

            _slotPadding = Math.Max((int)(4 * _uiScale), _slotSize / 12);
        }

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

            // Update window control buttons
            int buttonSize = (int)(24 * _uiScale);
            int buttonPadding = (int)(4 * _uiScale);

            // Close button (X) at the far right
            _closeButtonRect = new Rectangle(
                _titleBarRect.Right - buttonSize - buttonPadding,
                _titleBarRect.Y + ((_titleBarRect.Height - buttonSize) / 2),
                buttonSize,
                buttonSize);

            // Maximize button to the left of close button
            _maximizeButtonRect = new Rectangle(
                _closeButtonRect.X - buttonSize - buttonPadding,
                _closeButtonRect.Y,
                buttonSize,
                buttonSize);

            // Minimize button to the left of maximize button
            _minimizeButtonRect = new Rectangle(
                _maximizeButtonRect.X - buttonSize - buttonPadding,
                _closeButtonRect.Y,
                buttonSize,
                buttonSize);

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
                }
                else
                {
                    // Wrap to start of row
                    int row = _selectedItemIndex / GridColumns;
                    _selectedItemIndex = row * GridColumns;
                }

                selectionChanged = true;
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
            // Only update selection if it's a valid index
            if (itemIndex >= 0 && itemIndex < _inventoryItems.Count)
            {
                _selectedItemIndex = itemIndex;

                // Optionally, you could trigger a UI update here to refresh the details panel
                // This isn't strictly necessary if your Draw method already uses _selectedItemIndex
                // to determine what to display
            }
            else
            {
                // Invalid selection, reset it
                _selectedItemIndex = -1;
            }
        }

        /// <summary>
        /// Uses the selected inventory item
        /// </summary>
        private void UseItem(int itemIndex, PlayerIndex playerIndex)
        {
            if (itemIndex < 0 || itemIndex >= _inventoryItems.Count)
                return;

            InventoryItem item = _inventoryItems[itemIndex];

            if (_playerInventory != null)
            {
                // Use the item in the player's inventory
                _playerInventory.UseItem(item);

                // We don't need to update _inventoryItems or _selectedItemIndex
                // because the OnInventoryChanged handler will be called due to the
                // UseItem method triggering a CollectionChanged event
            }
            else
            {
                // Fallback behavior for when no player inventory is available
                if (item.Quantity > 0)
                {
                    item.Quantity--;

                    if (item.Quantity <= 0)
                    {
                        _inventoryItems.RemoveAt(itemIndex);
                        _selectedItemIndex = Math.Min(_selectedItemIndex, _inventoryItems.Count - 1);
                    }
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

            // Draw window control buttons
            DrawWindowControlButtons(spriteBatch);

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
        /// Draws the window control buttons (minimize, maximize, close)
        /// </summary>
        /// <summary>
        /// Draws the window control buttons (minimize, maximize, close)
        /// </summary>
        private void DrawWindowControlButtons(SpriteBatch spriteBatch)
        {
            // Colors for buttons
            Color buttonBgColor = new Color(80, 80, 120, 200) * TransitionAlpha;
            Color buttonHoverColor = new Color(100, 100, 150, 220) * TransitionAlpha;
            Color iconColor = Color.White * TransitionAlpha;
            int iconThickness = Math.Max(1, (int)(2 * _uiScale));

            // Get mouse position to check for hover states
            MouseState mouse = Mouse.GetState();
            Point mousePos = new(mouse.X, mouse.Y);

            // --- Draw Close Button (X) ---
            bool closeHover = _closeButtonRect.Contains(mousePos);
            spriteBatch.Draw(_backgroundTexture, _closeButtonRect,
                closeHover ? buttonHoverColor : buttonBgColor);

            // Draw X icon
            int iconPadding = (int)(6 * _uiScale);

            // Draw X (diagonal lines)
            // Top-left to bottom-right diagonal
            Rectangle closeX1 = new(
                _closeButtonRect.X + iconPadding,
                _closeButtonRect.Y + iconPadding,
                _closeButtonRect.Width - (iconPadding * 2),
                iconThickness);

            // Need to draw a rotated rectangle, so create a diagonal line
            for (int i = 0; i < _closeButtonRect.Height - (iconPadding * 2); i++)
            {
                Rectangle diagPiece = new(
                    _closeButtonRect.X + iconPadding + i,
                    _closeButtonRect.Y + iconPadding + i,
                    iconThickness,
                    iconThickness);
                spriteBatch.Draw(_backgroundTexture, diagPiece, iconColor);
            }

            // Bottom-left to top-right diagonal
            for (int i = 0; i < _closeButtonRect.Height - (iconPadding * 2); i++)
            {
                Rectangle diagPiece = new(
                    _closeButtonRect.X + iconPadding + i,
                    _closeButtonRect.Y + _closeButtonRect.Height - iconPadding - i,
                    iconThickness,
                    iconThickness);
                spriteBatch.Draw(_backgroundTexture, diagPiece, iconColor);
            }

            // --- Draw Maximize Button (square or restore icon) ---
            bool maxHover = _maximizeButtonRect.Contains(mousePos);
            spriteBatch.Draw(_backgroundTexture, _maximizeButtonRect,
                maxHover ? buttonHoverColor : buttonBgColor);

            // Draw appropriate icon based on window state
            Rectangle maxIconRect = new(
                _maximizeButtonRect.X + iconPadding,
                _maximizeButtonRect.Y + iconPadding,
                _maximizeButtonRect.Width - (iconPadding * 2),
                _maximizeButtonRect.Height - (iconPadding * 2));

            if (_isMaximized)
            {
                // Draw "restore down" icon (two overlapping squares)
                // Draw back square (top-left)
                Rectangle backSquare = new(
                    maxIconRect.X,
                    maxIconRect.Y,
                    (int)(maxIconRect.Width * 0.7f),
                    (int)(maxIconRect.Height * 0.7f));

                // Draw front square (bottom-right)
                Rectangle frontSquare = new(
                    maxIconRect.X + (int)(maxIconRect.Width * 0.3f),
                    maxIconRect.Y + (int)(maxIconRect.Height * 0.3f),
                    (int)(maxIconRect.Width * 0.7f),
                    (int)(maxIconRect.Height * 0.7f));

                // Draw square outlines
                DrawRectangleBorder(spriteBatch, backSquare, iconColor, iconThickness);
                DrawRectangleBorder(spriteBatch, frontSquare, iconColor, iconThickness);
            }
            else if (_isMinimized)
            {
                // When minimized, show maximize icon (arrow pointing outward)
                // Draw square outline
                DrawRectangleBorder(spriteBatch, maxIconRect, iconColor, iconThickness);

                // Draw diagonal arrow inside (from bottom-left to top-right)
                Point arrowStart = new(
                    maxIconRect.X + (int)(maxIconRect.Width * 0.3f),
                    maxIconRect.Y + (int)(maxIconRect.Height * 0.7f));

                Point arrowEnd = new(
                    maxIconRect.X + (int)(maxIconRect.Width * 0.7f),
                    maxIconRect.Y + (int)(maxIconRect.Height * 0.3f));

                // Draw arrow line
                for (int i = 0; i < iconThickness; i++)
                {
                    DrawLine(spriteBatch, arrowStart.X, arrowStart.Y + i,
                        arrowEnd.X, arrowEnd.Y + i, iconColor);
                }

                // Draw arrowhead
                int arrowHeadSize = (int)(4 * _uiScale);

                // Top part of arrowhead
                DrawLine(spriteBatch, arrowEnd.X, arrowEnd.Y,
                    arrowEnd.X - arrowHeadSize, arrowEnd.Y + arrowHeadSize, iconColor);

                // Right part of arrowhead
                DrawLine(spriteBatch, arrowEnd.X, arrowEnd.Y,
                    arrowEnd.X - arrowHeadSize, arrowEnd.Y - arrowHeadSize, iconColor);
            }
            else
            {
                // Draw "maximize" icon (simple square)
                DrawRectangleBorder(spriteBatch, maxIconRect, iconColor, iconThickness);
            }

            // --- Draw Minimize Button (horizontal line) ---
            bool minHover = _minimizeButtonRect.Contains(mousePos);
            spriteBatch.Draw(_backgroundTexture, _minimizeButtonRect,
                minHover ? buttonHoverColor : buttonBgColor);

            // Draw horizontal line for minimize
            int lineY = _minimizeButtonRect.Y + _minimizeButtonRect.Height - iconPadding - iconThickness;
            Rectangle minLine = new(
                _minimizeButtonRect.X + iconPadding,
                lineY,
                _minimizeButtonRect.Width - (iconPadding * 2),
                iconThickness);

            spriteBatch.Draw(_backgroundTexture, minLine, iconColor);

            // Draw borders around all buttons
            Color buttonBorderColor = new Color(120, 120, 180, 200) * TransitionAlpha;
            DrawRectangleBorder(spriteBatch, _closeButtonRect, buttonBorderColor, 1);
            DrawRectangleBorder(spriteBatch, _maximizeButtonRect, buttonBorderColor, 1);
            DrawRectangleBorder(spriteBatch, _minimizeButtonRect, buttonBorderColor, 1);
        }

        /// <summary>
        /// Draws a line from (x1,y1) to (x2,y2)
        /// </summary>
        private void DrawLine(SpriteBatch spriteBatch, int x1, int y1, int x2, int y2, Color color)
        {
            // Calculate slope
            float dx = x2 - x1;
            float dy = y2 - y1;
            float steps = Math.Max(Math.Abs(dx), Math.Abs(dy));

            // Calculate increment
            float xInc = dx / steps;
            float yInc = dy / steps;

            // Start point
            float x = x1;
            float y = y1;

            // Draw each pixel along the line
            for (int i = 0; i <= steps; i++)
            {
                Rectangle pixelRect = new(
                    (int)x, (int)y, 1, 1);
                spriteBatch.Draw(_backgroundTexture, pixelRect, color);

                x += xInc;
                y += yInc;
            }
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
                        float itemFontScale = _fontScale * 0.8f; // Slightly smaller for items

                        // Draw item texture if available
                        if (item.Texture != null)
                        {
                            // Determine texture properties
                            Rectangle sourceRect;
                            float textureWidth, textureHeight;

                            if (item.IsSpriteAtlas && item.SourceRectangle != Rectangle.Empty)
                            {
                                sourceRect = item.SourceRectangle;
                                textureWidth = sourceRect.Width;
                                textureHeight = sourceRect.Height;
                            }
                            else
                            {
                                sourceRect = new Rectangle(0, 0, item.Texture.Width, item.Texture.Height);
                                textureWidth = item.Texture.Width;
                                textureHeight = item.Texture.Height;
                            }

                            // Calculate scale to fit slot (with small padding)
                            float maxSize = _slotSize - 6;
                            float scale = Math.Min(
                                maxSize / textureWidth,
                                maxSize / textureHeight
                            ) * item.Scale;

                            // Center texture in slot
                            Vector2 position = new(
                                slotRect.X + (_slotSize - textureWidth * scale) / 2,
                                slotRect.Y + (_slotSize - textureHeight * scale) / 2
                            );

                            // Draw the texture
                            spriteBatch.Draw(
                                item.Texture,
                                position,
                                sourceRect,
                                Color.White * TransitionAlpha,
                                0f,
                                Vector2.Zero,
                                scale,
                                SpriteEffects.None,
                                0f
                            );
                        }
                        else
                        {
                            // Fallback to text if no texture is available
                            string itemName = SanitizeText(item.Name);
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

            // Calculate space needed for help text at bottom
            string helpText = "WASD/Arrows: Navigate  E: Use Item  Esc/I: Close";
            Vector2 helpSize = _font.MeasureString(helpText) * _fontScale;
            int bottomPadding = (int)(helpSize.Y + 25 * _uiScale); // Add extra padding for safety

            // Draw character background
            Rectangle characterImageRect = new(
                _characterRect.X + (int)(20 * _uiScale),
                _characterRect.Y + (int)(40 * _uiScale),
                _characterRect.Width - (int)(40 * _uiScale),
                _characterRect.Height - bottomPadding - (int)(60 * _uiScale));

            Color characterBg = new Color(50, 50, 80, 150) * TransitionAlpha;
            spriteBatch.Draw(_backgroundTexture, characterImageRect, characterBg);

            // Draw border
            DrawRectangleBorder(spriteBatch, characterImageRect,
                new Color(100, 100, 150, 200) * TransitionAlpha, (int)(2 * _uiScale));

            // Get reference to the player ant
            MovableAnimatedEntity playerAnt = null;
            if (ScreenManager != null)
            {
                foreach (GameScreen screen in ScreenManager.GetScreens())
                {
                    if (screen is GameplayScreen gameplayScreen && gameplayScreen.GameStateOrganizer != null)
                    {
                        // Try to get player ant from the game state
                        playerAnt = gameplayScreen.GameStateOrganizer.GetPlayerAnt();
                        break;
                    }
                }
            }

            // Draw the player ant sprite if we have it
            if (playerAnt != null && playerAnt.Texture != null)
            {
                // Calculate center of character panel
                Vector2 centerPosition = new(
                    characterImageRect.X + characterImageRect.Width / 2,
                    characterImageRect.Y + characterImageRect.Height / 2);

                // Use the idle frame for display (usually frame 0)
                int frameToShow = 0; // Idle frame

                // Get sprite dimensions from TextureInfo
                int frameWidth = (int)(playerAnt.TextureInfo.TextureWidth / playerAnt.TextureInfo.NumOfSpriteCols);
                int frameHeight = (int)(playerAnt.TextureInfo.TextureHeight);

                if (!playerAnt.HasDirection)
                {
                    // For sprites without direction (like the ant with 3 frames in 1 row)
                    Rectangle source = new(
                        frameToShow * frameWidth,
                        0, // y is always 0 for single row
                        frameWidth,
                        frameHeight);

                    // Calculate scale to fit in panel
                    float scale = Math.Min(
                        characterImageRect.Width / (float)frameWidth,
                        characterImageRect.Height / (float)frameHeight) * 0.8f; // 80% of max size for some padding

                    // Calculate draw position (center the frame)
                    Vector2 drawPosition = new(
                        centerPosition.X - (frameWidth * scale / 2),
                        centerPosition.Y - (frameHeight * scale / 2));

                    // Draw the ant frame
                    spriteBatch.Draw(
                        playerAnt.Texture,
                        drawPosition,
                        source,
                        Color.White * TransitionAlpha,
                        0f,
                        Vector2.Zero,
                        scale,
                        SpriteEffects.None, // No flipping for display
                        0f);
                }
                else
                {
                    // For directional sprites
                    int directionIndex = 0; // Default direction (usually down)

                    Rectangle source = new(
                        frameToShow * frameWidth,
                        directionIndex * (frameHeight / playerAnt.TextureInfo.NumOfSpriteRows),
                        frameWidth,
                        frameHeight / playerAnt.TextureInfo.NumOfSpriteRows);

                    // Calculate scale to fit in panel
                    float scale = Math.Min(
                        characterImageRect.Width / (float)source.Width,
                        characterImageRect.Height / (float)source.Height) * 0.8f;

                    // Calculate draw position (center the frame)
                    Vector2 drawPosition = new(
                        centerPosition.X - (source.Width * scale / 2),
                        centerPosition.Y - (source.Height * scale / 2));

                    // Draw the ant frame
                    spriteBatch.Draw(
                        playerAnt.Texture,
                        drawPosition,
                        source,
                        Color.White * TransitionAlpha,
                        0f,
                        Vector2.Zero,
                        scale,
                        SpriteEffects.None,
                        0f);
                }
            }
            else
            {
                // If player sprite isn't available, draw a placeholder
                DrawAntPlaceholder(spriteBatch, characterImageRect);
            }

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
        /// Draws a simple ant placeholder if the actual sprite is not available
        /// </summary>
        private void DrawAntPlaceholder(SpriteBatch spriteBatch, Rectangle rect)
        {
            // Draw a simple ant silhouette as a placeholder

            // Ant body (oval)
            int bodyWidth = (int)(rect.Width * 0.6f);
            int bodyHeight = (int)(rect.Height * 0.4f);
            Rectangle bodyRect = new(
                rect.X + (rect.Width - bodyWidth) / 2,
                rect.Y + (rect.Height - bodyHeight) / 2,
                bodyWidth,
                bodyHeight);

            Color antColor = new Color(120, 70, 20, 200) * TransitionAlpha; // Brown for ant
            spriteBatch.Draw(_backgroundTexture, bodyRect, antColor);

            // Ant head (circle)
            int headSize = (int)(bodyHeight * 0.8f);
            Rectangle headRect = new(
                bodyRect.X - headSize / 3,
                bodyRect.Y + (bodyHeight - headSize) / 2,
                headSize,
                headSize);

            spriteBatch.Draw(_backgroundTexture, headRect, antColor);

            // Legs (lines)
            int legLength = (int)(bodyWidth * 0.4f);
            int legThickness = (int)(3 * _uiScale);

            // Draw 6 legs (3 on each side)
            for (int i = 0; i < 3; i++)
            {
                // Position along body
                float position = 0.2f + (i * 0.3f);

                // Left leg
                Rectangle leftLeg = new(
                    bodyRect.X + (int)(bodyWidth * position),
                    bodyRect.Y + bodyHeight / 2,
                    legThickness,
                    legLength);
                spriteBatch.Draw(_backgroundTexture, leftLeg, antColor);

                // Right leg
                Rectangle rightLeg = new(
                    bodyRect.X + (int)(bodyWidth * position),
                    bodyRect.Y + bodyHeight / 2,
                    legThickness,
                    legLength);
                spriteBatch.Draw(_backgroundTexture, rightLeg, antColor);
            }

            // Antennae
            int antennaLength = (int)(headSize * 0.8f);
            int antennaThickness = (int)(2 * _uiScale);

            Rectangle leftAntenna = new(
                headRect.X + headSize / 4,
                headRect.Y - antennaLength + antennaThickness,
                antennaThickness,
                antennaLength);

            Rectangle rightAntenna = new(
                headRect.X + headSize * 3 / 4,
                headRect.Y - antennaLength + antennaThickness,
                antennaThickness,
                antennaLength);

            spriteBatch.Draw(_backgroundTexture, leftAntenna, antColor);
            spriteBatch.Draw(_backgroundTexture, rightAntenna, antColor);
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
                    _statsRect.Y + (int)(8 * _uiScale)),
                Color.White * TransitionAlpha,
                0f, Vector2.Zero, _fontScale, SpriteEffects.None, 0f);

            // Starting position for stats
            Vector2 statPos = new(_statsRect.X + (int)(15 * _uiScale), _statsRect.Y + (int)(35 * _uiScale));

            // Calculate a better line height with more spacing
            float lineHeight = _font.LineSpacing * _fontScale * 1.3f;

            // Create category headers with distinct styling
            float headerFontScale = _fontScale * 1.1f;
            Color headerColor = new Color(200, 200, 255) * TransitionAlpha;

            // --- VITAL STATISTICS SECTION ---
            spriteBatch.DrawString(_font, "Vital Statistics",
                statPos, headerColor,
                0f, Vector2.Zero, headerFontScale, SpriteEffects.None, 0f);
            statPos.Y += lineHeight;

            // Draw Health
            DrawStatBar(spriteBatch, statPos, "Health",
                _playerStatus.HitPoints, _playerStatus.MaxHitPoints,
                Color.Red);
            statPos.Y += lineHeight * 1.3f; // Increased from 1.0 to 1.1 for more padding

            // Draw Stamina
            DrawStatBar(spriteBatch, statPos, "Stamina",
                _playerStatus.Stamina, _playerStatus.MaxStamina,
                Color.Green);
            statPos.Y += lineHeight * 1.3f; // Increased from 1.0 to 1.1 for more padding

            // Draw Hunger
            DrawStatBar(spriteBatch, statPos, "Hunger",
                _playerStatus.Hunger, _playerStatus.MaxHunger,
                Color.Yellow);
            statPos.Y += lineHeight * 1.8f; // Increased from 1.5 to 1.6 for more padding before attributes

            // --- SPECIAL ATTRIBUTES SECTION ---
            spriteBatch.DrawString(_font, "SPECIAL Attributes",
                statPos, headerColor,
                0f, Vector2.Zero, headerFontScale, SpriteEffects.None, 0f);
            statPos.Y += lineHeight;

            // Determine if we have enough space for two columns or need a compact format
            float attributeFontScale = _fontScale * 0.9f; // Slightly smaller for attributes
            float columnWidth = _statsRect.Width / 2 - (int)(25 * _uiScale);
            Vector2 rightColPos = new(statPos.X + columnWidth, statPos.Y);

            // Check available width
            Vector2 sampleTextSize = _font.MeasureString("Strength: 10") * attributeFontScale;
            bool useColumns = sampleTextSize.X < columnWidth; // Only use columns if text fits

            if (useColumns)
            {
                // Left column - S, P, E, C
                spriteBatch.DrawString(_font, $"Strength: {_playerStatus.Strength}",
                    statPos, Color.White * TransitionAlpha,
                    0f, Vector2.Zero, attributeFontScale, SpriteEffects.None, 0f);
                statPos.Y += lineHeight * 0.8f;

                spriteBatch.DrawString(_font, $"Perception: {_playerStatus.Perception}",
                    statPos, Color.White * TransitionAlpha,
                    0f, Vector2.Zero, attributeFontScale, SpriteEffects.None, 0f);
                statPos.Y += lineHeight * 0.8f;

                spriteBatch.DrawString(_font, $"Endurance: {_playerStatus.Endurance}",
                    statPos, Color.White * TransitionAlpha,
                    0f, Vector2.Zero, attributeFontScale, SpriteEffects.None, 0f);
                statPos.Y += lineHeight * 0.8f;

                spriteBatch.DrawString(_font, $"Charisma: {_playerStatus.Charisma}",
                    statPos, Color.White * TransitionAlpha,
                    0f, Vector2.Zero, attributeFontScale, SpriteEffects.None, 0f);

                // Right column - I, A, L
                spriteBatch.DrawString(_font, $"Intelligence: {_playerStatus.Intelligence}",
                    rightColPos, Color.White * TransitionAlpha,
                    0f, Vector2.Zero, attributeFontScale, SpriteEffects.None, 0f);
                rightColPos.Y += lineHeight * 0.8f;

                spriteBatch.DrawString(_font, $"Agility: {_playerStatus.Agility}",
                    rightColPos, Color.White * TransitionAlpha,
                    0f, Vector2.Zero, attributeFontScale, SpriteEffects.None, 0f);
                rightColPos.Y += lineHeight * 0.8f;

                spriteBatch.DrawString(_font, $"Luck: {_playerStatus.Luck}",
                    rightColPos, Color.White * TransitionAlpha,
                    0f, Vector2.Zero, attributeFontScale, SpriteEffects.None, 0f);

                // Use maximum Y position from either column for next section
                statPos.Y = Math.Max(statPos.Y, rightColPos.Y) + lineHeight * 0.8f;
            }
            else
            {
                // Compact single column format - use abbreviations for SPECIAL
                float smallPadding = lineHeight * 0.7f;

                spriteBatch.DrawString(_font, $"S: {_playerStatus.Strength}   P: {_playerStatus.Perception}",
                    statPos, Color.White * TransitionAlpha,
                    0f, Vector2.Zero, attributeFontScale, SpriteEffects.None, 0f);
                statPos.Y += smallPadding;

                spriteBatch.DrawString(_font, $"E: {_playerStatus.Endurance}   C: {_playerStatus.Charisma}",
                    statPos, Color.White * TransitionAlpha,
                    0f, Vector2.Zero, attributeFontScale, SpriteEffects.None, 0f);
                statPos.Y += smallPadding;

                spriteBatch.DrawString(_font, $"I: {_playerStatus.Intelligence}   A: {_playerStatus.Agility}",
                    statPos, Color.White * TransitionAlpha,
                    0f, Vector2.Zero, attributeFontScale, SpriteEffects.None, 0f);
                statPos.Y += smallPadding;

                spriteBatch.DrawString(_font, $"L: {_playerStatus.Luck}",
                    statPos, Color.White * TransitionAlpha,
                    0f, Vector2.Zero, attributeFontScale, SpriteEffects.None, 0f);
                statPos.Y += smallPadding;
            }

            // Add a bit more space after attributes section
            statPos.Y += lineHeight * 0.5f;

            // --- RECOVERY RATES SECTION ---
            float remainingHeight = _statsRect.Bottom - statPos.Y;
            if (remainingHeight >= lineHeight * 3) // Only show if enough space
            {
                spriteBatch.DrawString(_font, "Recovery Rates",
                    statPos, headerColor,
                    0f, Vector2.Zero, headerFontScale, SpriteEffects.None, 0f);
                statPos.Y += lineHeight;

                // Format recovery rates nicely
                string staminaRegen = $"Stamina: {_playerStatus.StaminaRegenRate:F1}/sec";
                if (_playerStatus.StaminaRegenDelay > 0)
                    staminaRegen += $" (after {_playerStatus.StaminaRegenDelay:F1}s)";

                spriteBatch.DrawString(_font, staminaRegen,
                    statPos, Color.White * TransitionAlpha,
                    0f, Vector2.Zero, _fontScale * 0.9f, SpriteEffects.None, 0f);
                statPos.Y += lineHeight * 0.8f;

                // Show hunger consumption rates with compact formatting
                if (remainingHeight >= lineHeight * 5) // Only if enough space
                {
                    float hungerFontScale = _fontScale * 0.85f;

                    spriteBatch.DrawString(_font, "Hunger rates:",
                        statPos, Color.White * TransitionAlpha,
                        0f, Vector2.Zero, hungerFontScale, SpriteEffects.None, 0f);
                    statPos.Y += lineHeight * 0.7f;

                    if (_playerStatus.IdleHungerRate > 0)
                    {
                        spriteBatch.DrawString(_font, $"  Idle: {_playerStatus.IdleHungerRate * 60:F2}/min",
                            statPos, Color.White * TransitionAlpha,
                            0f, Vector2.Zero, hungerFontScale, SpriteEffects.None, 0f);
                        statPos.Y += lineHeight * 0.7f;
                    }

                    if (_playerStatus.MovingHungerRate > 0 && remainingHeight >= lineHeight * 6)
                    {
                        spriteBatch.DrawString(_font, $"  Moving: {_playerStatus.MovingHungerRate * 60:F2}/min",
                            statPos, Color.White * TransitionAlpha,
                            0f, Vector2.Zero, hungerFontScale, SpriteEffects.None, 0f);
                    }
                }
            }
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

            // Calculate bar position and size with increased vertical padding
            int barWidth = _statsRect.Width - (int)(30 * _uiScale);
            int barHeight = (int)(14 * _uiScale);

            // Add more padding between label and bar
            int topPadding = (int)(_font.LineSpacing * _fontScale) + (int)(4 * _uiScale);

            Rectangle barBg = new(
                (int)position.X,
                (int)position.Y + topPadding,
                barWidth,
                barHeight);

            // Draw background with semi-transparency
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

            // Make color slightly more transparent for nicer appearance
            Color fillColor = new Color(
                barColor.R,
                barColor.G,
                barColor.B,
                (byte)(barColor.A * 0.9f)) * TransitionAlpha;

            spriteBatch.Draw(_backgroundTexture, barFill, fillColor);

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

            // Return the adjusted position with added vertical padding after the bar
            // This is handled by the caller incrementing statPos.Y by lineHeight
        }

        /// <summary>
        /// Draws details of the selected item in a panel below the inventory grid
        /// </summary>
        private void DrawSelectedItemDetails(SpriteBatch spriteBatch)
        {
            // Only draw if an item is selected
            if (_selectedItemIndex < 0 || _selectedItemIndex >= _inventoryItems.Count)
                return;

            InventoryItem item = _inventoryItems[_selectedItemIndex];

            // Get the total grid height to ensure details don't overlap with grid
            int totalSlotSize = _slotSize + _slotPadding;
            int gridWidth = totalSlotSize * GridColumns + _slotPadding;
            int gridHeight = totalSlotSize * GridRows + _slotPadding;

            // Get grid start position
            int gridStartX = _gridRect.X + (_gridRect.Width - gridWidth) / 2;
            int gridStartY = _gridRect.Y + (int)(10 * _uiScale);

            // Calculate grid bottom position
            int gridBottom = gridStartY + gridHeight + (int)(20 * _uiScale); // Add padding

            // Calculate details panel dimensions
            int detailsHeight = (int)(120 * _uiScale);
            int detailsWidth = gridWidth - (int)(20 * _uiScale);

            // Calculate available space between grid bottom and window bottom
            int availableSpace = _gridRect.Bottom - gridBottom - (int)(20 * _uiScale);

            // Adjust details height if not enough space
            if (availableSpace < detailsHeight)
            {
                detailsHeight = Math.Max(availableSpace, (int)(70 * _uiScale)); // Minimum height
            }

            // Position the details panel below the grid
            Rectangle detailsRect = new(
                gridStartX + (int)(10 * _uiScale), // Align with grid
                gridBottom, // Position just below grid
                detailsWidth,
                detailsHeight);

            // Draw panel background
            Color detailsBg = new Color(50, 50, 80, 180) * TransitionAlpha;
            spriteBatch.Draw(_backgroundTexture, detailsRect, detailsBg);

            // Draw border
            DrawRectangleBorder(spriteBatch, detailsRect,
                new Color(100, 100, 150, 200) * TransitionAlpha, 1);

            // Draw item details
            Vector2 textPos = new(detailsRect.X + (int)(10 * _uiScale), detailsRect.Y + (int)(8 * _uiScale));
            float detailsFontScale = _fontScale * 0.9f;

            // If texture exists, show larger preview on left and details on right
            if (item.Texture != null)
            {
                // Calculate image area on left side of panel (1/3 width)
                int previewSize = Math.Min(
                    (int)(detailsRect.Width * 0.25f), // 25% of panel width 
                    (int)(detailsHeight - 16)        // Almost full height with padding
                );

                Rectangle imageArea = new(
                    detailsRect.X + (int)(10 * _uiScale),
                    detailsRect.Y + (int)(8 * _uiScale),
                    previewSize,
                    previewSize
                );

                // Get source rectangle
                Rectangle sourceRect;
                float textureWidth, textureHeight;

                if (item.IsSpriteAtlas && item.SourceRectangle != Rectangle.Empty)
                {
                    sourceRect = item.SourceRectangle;
                    textureWidth = sourceRect.Width;
                    textureHeight = sourceRect.Height;
                }
                else
                {
                    sourceRect = new Rectangle(0, 0, item.Texture.Width, item.Texture.Height);
                    textureWidth = item.Texture.Width;
                    textureHeight = item.Texture.Height;
                }

                // Calculate scale to fit preview area
                float scale = Math.Min(
                    imageArea.Width / textureWidth,
                    imageArea.Height / textureHeight
                ) * item.Scale;

                // Center texture in preview area
                Vector2 imagePos = new(
                    imageArea.X + (imageArea.Width - textureWidth * scale) / 2,
                    imageArea.Y + (imageArea.Height - textureHeight * scale) / 2
                );

                // Draw texture
                spriteBatch.Draw(
                    item.Texture,
                    imagePos,
                    sourceRect,
                    Color.White * TransitionAlpha,
                    0f,
                    Vector2.Zero,
                    scale,
                    SpriteEffects.None,
                    0f
                );

                // Start text position to the right of image
                textPos = new Vector2(
                    imageArea.Right + (int)(15 * _uiScale),
                    imageArea.Y
                );

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

                // Calculate available width for description (narrower due to image)
                float maxWidth = detailsRect.Right - textPos.X - (int)(10 * _uiScale);

                // Draw description header
                spriteBatch.DrawString(_font, "Description:", textPos,
                    Color.LightBlue * TransitionAlpha,
                    0f, Vector2.Zero, detailsFontScale, SpriteEffects.None, 0f);
                textPos.Y += _font.LineSpacing * detailsFontScale;

                // Calculate available space for description text
                float availableDescriptionSpace = detailsRect.Bottom - textPos.Y - (int)(8 * _uiScale);
                int maxLines = (int)(availableDescriptionSpace / (_font.LineSpacing * detailsFontScale));

                // Draw description with line limiting
                string description = item.Description;
                List<string> lines = WrapText(description, _font, maxWidth / detailsFontScale);

                // Show only what fits
                for (int i = 0; i < Math.Min(lines.Count, maxLines); i++)
                {
                    spriteBatch.DrawString(_font, lines[i], textPos,
                        Color.White * TransitionAlpha,
                        0f, Vector2.Zero, detailsFontScale, SpriteEffects.None, 0f);
                    textPos.Y += _font.LineSpacing * detailsFontScale;
                }
            }
            else
            {
                // Original layout for items without textures
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

                // Calculate available space for description
                float availableDescriptionSpace = detailsRect.Bottom - textPos.Y - (int)(8 * _uiScale);
                int maxLines = (int)(availableDescriptionSpace / (_font.LineSpacing * detailsFontScale));

                // Draw description with line limiting
                string description = item.Description;
                float maxWidth = detailsRect.Width - (int)(20 * _uiScale);
                List<string> lines = WrapText(description, _font, maxWidth / detailsFontScale);

                // Show only what fits
                for (int i = 0; i < Math.Min(lines.Count, maxLines); i++)
                {
                    spriteBatch.DrawString(_font, lines[i], textPos,
                        Color.White * TransitionAlpha,
                        0f, Vector2.Zero, detailsFontScale, SpriteEffects.None, 0f);
                    textPos.Y += _font.LineSpacing * detailsFontScale;
                }
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