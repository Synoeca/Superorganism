using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

        private readonly InputAction _menuUp;
        private readonly InputAction _menuDown;
        private readonly InputAction _menuLeft;
        private readonly InputAction _menuRight;
        private readonly InputAction _menuSelect;
        private readonly InputAction _menuCancel;

        // Gets the list of menu entries, so derived classes can add or change the menu contents.
        protected IList<MenuEntry> MenuEntries => _menuEntries;

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
            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // start at Y = 175; each X value is generated per entry
            Vector2 position = new(0f, 175f);

            // update each menu entry's location in turn
            foreach (MenuEntry menuEntry in _menuEntries)
            {
                // each entry is to be centered horizontally
                position.X = ScreenManager.GraphicsDevice.Viewport.Width / 2f - menuEntry.GetWidth(this) / 2f;

                if (ScreenState == ScreenState.TransitionOn)
                    position.X -= transitionOffset * 256;
                else
                    position.X += transitionOffset * 512;

                // set the entry's position
                menuEntry.Position = position;

                // move down for the next entry the size of this entry
                position.Y += menuEntry.GetHeight(this);
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update each nested MenuEntry object.
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

            spriteBatch.Begin();

            // Menu entries
            for (int i = 0; i < _menuEntries.Count; i++)
            {
                MenuEntry menuEntry = _menuEntries[i];
                bool isSelected = IsActive && i == _selectedEntry;
                Color color = isSelected ? Color.Yellow : Color.White;

                // Replace single spaces with three consecutive spaces in menu entry text
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

            // Title with existing shadow
            Vector2 titlePosition = new(graphics.Viewport.Width / 2f, 100);
            Vector2 titleOrigin = font.MeasureString(_menuTitle) / 2;
            const float titleScale = 1.5f;

            // Replace single spaces with three consecutive spaces in title text
            string adjustedTitleText = _menuTitle.Replace(" ", "   ");

            // Draw title shadow
            spriteBatch.DrawString(font, adjustedTitleText,
                titlePosition + new Vector2(4),
                Color.Black * TransitionAlpha,
                0, titleOrigin, titleScale, SpriteEffects.None, 0);

            // Draw title text
            spriteBatch.DrawString(font, adjustedTitleText,
                titlePosition,
                new Color(220, 220, 220) * TransitionAlpha,
                0, titleOrigin, titleScale, SpriteEffects.None, 0);

            spriteBatch.End();
        }

    }
}
