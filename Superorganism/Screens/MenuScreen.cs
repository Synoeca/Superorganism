using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Superorganism.Graphics;
using Superorganism.ScreenManagement;

namespace Superorganism.Screens
{
    // Base class for screens that contain a menu of options. The user can
    // move up and down to select an entry, or cancel to back out of the screen.
    public abstract class MenuScreen : GameScreen
    {
        private readonly List<MenuEntry> _menuEntries = [];
        private int _selectedEntry;
        private readonly string _menuTitle;
        public PixelTextRenderer TitleRenderer;

        private readonly InputAction _menuUp;
        private readonly InputAction _menuDown;
        private readonly InputAction _menuLeft;
        private readonly InputAction _menuRight;
        private readonly InputAction _menuSelect;
        private readonly InputAction _menuCancel;

        // Gets the list of menu entries, so derived classes can add or change the menu contents.
        public IList<MenuEntry> MenuEntries => _menuEntries;

        protected MenuScreen(string menuTitle)
        {
            _menuTitle = menuTitle;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            _menuUp = new InputAction(
                [Buttons.DPadUp, Buttons.LeftThumbstickUp],
                [Keys.Up], true);
            _menuDown = new InputAction(
                [Buttons.DPadDown, Buttons.LeftThumbstickDown],
                [Keys.Down], true);
            _menuSelect = new InputAction(
                [Buttons.A, Buttons.Start],
                [Keys.Enter, Keys.Space], true);
            _menuCancel = new InputAction(
                [Buttons.B, Buttons.Back],
                [Keys.Back, Keys.Escape], true);
            _menuLeft = new InputAction(
                [Buttons.DPadLeft, Buttons.LeftThumbstickLeft],
                [Keys.Left], true);
            _menuRight = new InputAction(
                [Buttons.DPadRight, Buttons.LeftThumbstickRight],
                [Keys.Right], true);
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (_menuUp.Occurred(input, ControllingPlayer, out PlayerIndex playerIndex))
            {
                _selectedEntry--;
                if (_selectedEntry < 0)
                    _selectedEntry = _menuEntries.Count - 1;
            }
            if (_menuDown.Occurred(input, ControllingPlayer, out playerIndex))
            {
                _selectedEntry++;
                if (_selectedEntry >= _menuEntries.Count)
                    _selectedEntry = 0;
            }
            if (_menuSelect.Occurred(input, ControllingPlayer, out playerIndex))
                OnSelectEntry(_selectedEntry, playerIndex);
            if (_menuCancel.Occurred(input, ControllingPlayer, out playerIndex))
                OnCancel(playerIndex);
            if (_menuLeft.Occurred(input, ControllingPlayer, out playerIndex))
                OnAdjustValue(_selectedEntry, -1, playerIndex);
            if (_menuRight.Occurred(input, ControllingPlayer, out playerIndex))
                OnAdjustValue(_selectedEntry, 1, playerIndex);
        }

        protected virtual void OnAdjustValue(int entryIndex, int direction, PlayerIndex playerIndex)
        {
            _menuEntries[entryIndex].OnAdjustValue(direction, playerIndex);
        }

        protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex)
        {
            _menuEntries[entryIndex].OnSelectEntry(playerIndex);
        }

        protected virtual void OnCancel(PlayerIndex playerIndex)
        {
            ExitScreen();
        }

        // Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        protected void OnCancel(object sender, PlayerIndexEventArgs e)
        {
            OnCancel(e.PlayerIndex);
        }

        // Allows the screen the chance to position the menu entries. By default,
        // all menu entries are lined up in a vertical list, centered on the screen.
        protected virtual void UpdateMenuEntryLocations()
        {
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // Calculate the center of the screen
            float centerX = ScreenManager.GraphicsDevice.Viewport.Width / 2f;
            float centerY = ScreenManager.GraphicsDevice.Viewport.Height / 2f;

            // Calculate total height of all menu entries
            float totalHeight = _menuEntries.Sum(entry => entry.GetHeight(this));

            // Start position, centered vertically
            Vector2 position = new(0f, centerY - totalHeight / 2f);

            foreach (MenuEntry menuEntry in _menuEntries)
            {
                // First center the text
                float baseX = centerX - menuEntry.GetWidth(this) / 2f;

                // Then apply the transition offset
                if (ScreenState == ScreenState.TransitionOn)
                    position.X = baseX - transitionOffset * 256;
                else
                    position.X = baseX + transitionOffset * 512;

                // Set the entry's position
                menuEntry.Position = position;

                // Move down for the next entry
                position.Y += menuEntry.GetHeight(this);
            }
        }

        public override void Activate()
        {
            base.Activate();

            // Initialize the title renderer if it doesn't exist
            if (TitleRenderer == null && !string.IsNullOrEmpty(_menuTitle))
            {
                TitleRenderer = new PixelTextRenderer(
                    ScreenManager.Game,
                    _menuTitle  // or whatever text you want to display
                );
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update the title renderer
            if (TitleRenderer != null && !string.IsNullOrEmpty(_menuTitle))
            {
                TitleRenderer.Update(gameTime, TransitionPosition, ScreenState);
            }

            // Update menu entries
            for (int i = 0; i < _menuEntries.Count; i++)
            {
                bool isSelected = IsActive && i == _selectedEntry;
                _menuEntries[i].Update(this, isSelected, gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            UpdateMenuEntryLocations();

            GraphicsDevice graphics = ScreenManager.GraphicsDevice;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;
            const float shadowOffset = 3f;

            // Draw the 3D title first if it exists
            if (TitleRenderer != null && !string.IsNullOrEmpty(_menuTitle) && !IsExiting)
            {
                if (ScreenState == ScreenState.TransitionOn ||
                    ScreenState == ScreenState.Active ||
                    ScreenState == ScreenState.TransitionOff)
                {
                    TitleRenderer.Draw();
                }
            }

            // Draw menu entries
            spriteBatch.Begin();

            for (int i = 0; i < _menuEntries.Count; i++)
            {
                MenuEntry menuEntry = _menuEntries[i];
                bool isSelected = IsActive && i == _selectedEntry;
                Color color = isSelected ? Color.Yellow : Color.White;

                string adjustedMenuEntryText = menuEntry.Text.Replace(" ", "   ");

                // Draw shadow
                spriteBatch.DrawString(font, adjustedMenuEntryText,
                    menuEntry.Position + new Vector2(shadowOffset),
                    Color.Black * TransitionAlpha);

                // Draw text
                spriteBatch.DrawString(font, adjustedMenuEntryText,
                    menuEntry.Position,
                    color * TransitionAlpha);
            }

            spriteBatch.End();
        }

        public override void Unload()
        {
            base.Unload();
            TitleRenderer = null;
        }

    }
}
