using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Superorganism.ScreenManagement;
using Superorganism.Graphics;

namespace Superorganism.Screens
{
    public class InstructionsScreen : GameScreen
    {
        private readonly List<List<InstructionEntry>> _pages = [];
        private readonly InstructionEntry _backButton;
        private int _currentPage;
        private const int EntriesPerPage = 6;
        private const float InitialY = 175f;
        private const float YSpacing = 35f;
        private Vector2 _pageIndicatorPosition;
        private PixelTextRenderer _titleRenderer;
        private readonly string _menuTitle = "Instructions";

        private readonly InputAction _menuLeft;
        private readonly InputAction _menuRight;
        private readonly InputAction _menuSelect;
        private readonly InputAction _menuCancel;

        public InstructionsScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            string[] instructions =
            [
                // Page 1 - Basic Controls
                "Movement Controls:",
                "    A = Move Left        D = Move Right",
                "Jump:",
                "    Press SPACE while on ground",
                "Dash:",
                "    Hold SHIFT to increase speed",
                "Restart:",
                "    Press R to restart current level",
                "Exit Game:",
                "    ESC > Pause Menu > Select 'Quit Game'",

                // Page 2 - Game Objectives
                "Game Objective:",
                "    Collect all crops in the level",
                "Avoid Flies:",
                "    Do not make contact with flies",
                "Avoid Enemy Ants:",
                "    Stay away from red enemy ants",
                "Level Completion:",
                "    Collect all crops to complete level",
                "Strategic Dash:",
                "    Use SHIFT to avoid enemies strategically"
            ];


            for (int i = 0; i < instructions.Length; i += EntriesPerPage)
            {
                List<InstructionEntry> page = instructions.Skip(i).Take(EntriesPerPage)
                    .Select(text => new InstructionEntry(text))
                    .ToList();
                _pages.Add(page);
            }

            _backButton = new InstructionEntry("Back");

            _menuLeft = new InputAction(
                [Buttons.DPadLeft, Buttons.LeftThumbstickLeft],
                [Keys.Left], true);
            _menuRight = new InputAction(
                [Buttons.DPadRight, Buttons.LeftThumbstickRight],
                [Keys.Right], true);
            _menuSelect = new InputAction(
                [Buttons.A, Buttons.Start],
                [Keys.Enter, Keys.Space], true);
            _menuCancel = new InputAction(
                [Buttons.B, Buttons.Back],
                [Keys.Escape, Keys.Back], true);
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (_menuLeft.Occurred(input, ControllingPlayer, out _))
            {
                _currentPage = (_currentPage - 1 + _pages.Count) % _pages.Count;
            }

            if (_menuRight.Occurred(input, ControllingPlayer, out PlayerIndex _))
            {
                _currentPage = (_currentPage + 1) % _pages.Count;
            }
            if (_menuSelect.Occurred(input, ControllingPlayer, out PlayerIndex _) ||
                _menuCancel.Occurred(input, ControllingPlayer, out PlayerIndex _))
            {
                ExitScreen();
            }
        }

        public override void Activate()
        {
            base.Activate();

            // Initialize the title renderer if it doesn't exist
            if (_titleRenderer == null && !string.IsNullOrEmpty(_menuTitle))
            {
                _titleRenderer = new PixelTextRenderer(
                    ScreenManager.Game,
                    _menuTitle
                );
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update the title renderer
            if (_titleRenderer != null && !string.IsNullOrEmpty(_menuTitle))
            {
                _titleRenderer.Update(gameTime, TransitionPosition, ScreenState);
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                ExitScreen();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            // Draw the 3D title first if it exists
            if (_titleRenderer != null && !string.IsNullOrEmpty(_menuTitle) && !IsExiting)
            {
                if (ScreenState == ScreenState.TransitionOn ||
                    ScreenState == ScreenState.Active ||
                    ScreenState == ScreenState.TransitionOff)
                {
                    _titleRenderer.Draw();
                }
            }

            UpdateEntryLocations();
            spriteBatch.Begin();

            // Draw instructions
            foreach (InstructionEntry instruction in _pages[_currentPage])
                instruction.Draw(this);

            // Draw back button
            _backButton.Draw(this, true);

            // Draw page indicator
            if (_pages.Count > 1)
            {
                DrawPageIndicator(_pageIndicatorPosition);
            }

            spriteBatch.End();
        }

        private void DrawPageIndicator(Vector2 position)
        {
            SpriteFont font = ScreenManager.Font;
            string text = $"Page   {_currentPage + 1}/{_pages.Count}";

            ScreenManager.SpriteBatch.DrawString(font, text,
                position + new Vector2(2),
                Color.Black * TransitionAlpha,
                0, Vector2.Zero, 0.8f, SpriteEffects.None, 0);

            ScreenManager.SpriteBatch.DrawString(font, text,
                position,
                Color.White * TransitionAlpha,
                0, Vector2.Zero, 0.8f, SpriteEffects.None, 0);
        }

        private void UpdateEntryLocations()
        {
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            float viewportHeight = viewport.Height;
            float viewportWidth = viewport.Width;

            // Calculate the center of the screen
            float centerY = viewportHeight / 2f;

            // Calculate total height of all entries
            float totalHeight = _pages[_currentPage].Count * YSpacing;

            // Start position, centered vertically
            float y = centerY - totalHeight / 2f;

            float leftMargin = viewportWidth * 0.2f;
            float bottomY = y;

            // Left-align instructions
            foreach (InstructionEntry instruction in _pages[_currentPage])
            {
                instruction.Position = new Vector2(leftMargin, y);
                y += YSpacing;
                bottomY = y; // Track the last instruction's bottom position
            }

            float backButtonY = bottomY + 20;
            _backButton.Position = new Vector2(leftMargin, backButtonY);

            // Set page indicator position to be on the same line as the back button
            _pageIndicatorPosition = new Vector2(viewportWidth * 0.8f, backButtonY);
        }

        public override void Unload()
        {
            base.Unload();
            _titleRenderer = null;
        }
    }
}
