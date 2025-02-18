using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Superorganism.ScreenManagement;

namespace Superorganism.Screens
{
    public class MessageBoxScreen : GameScreen
    {
        private readonly string _message;
        private readonly string _confirmText;
        private readonly string _cancelText;
        private Texture2D _backgroundTexture;
        private readonly Vector2 _padding = new(40, 24);
        private readonly Color _backgroundColor = new(0, 0, 0, 230);
        private readonly Color _textColor = Color.White;
        private readonly Color _selectedColor = Color.Yellow;
        private bool _isConfirmSelected = true;

        private readonly InputAction _menuSelect;
        private readonly InputAction _menuCancel;
        private readonly InputAction _menuLeft;
        private readonly InputAction _menuRight;

        public event EventHandler<PlayerIndexEventArgs> Accepted;
        public event EventHandler<PlayerIndexEventArgs> Cancelled;

        public MessageBoxScreen(string message, string confirmText = "OK", string cancelText = "Cancel")
        {
            _message = message;
            _confirmText = confirmText;
            _cancelText = cancelText;
            IsPopup = true;
            TransitionOnTime = TimeSpan.FromSeconds(0.15);
            TransitionOffTime = TimeSpan.FromSeconds(0.15);

            _menuSelect = new InputAction([Buttons.A, Buttons.Start], 
                [Keys.Enter, Keys.Space], true);
            _menuCancel = new InputAction([Buttons.B, Buttons.Back], 
                [Keys.Back, Keys.Escape], true);
            _menuLeft = new InputAction([Buttons.DPadLeft, Buttons.LeftThumbstickLeft], 
                [Keys.Left], true);
            _menuRight = new InputAction([Buttons.DPadRight, Buttons.LeftThumbstickRight], 
                [Keys.Right], true);
        }

        public override void Activate()
        {
            GraphicsDevice graphics = ScreenManager.GraphicsDevice;
            _backgroundTexture = new Texture2D(graphics, 1, 1);
            _backgroundTexture.SetData([Color.White]);
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (_menuLeft.Occurred(input, ControllingPlayer, out PlayerIndex playerIndex) ||
                _menuRight.Occurred(input, ControllingPlayer, out playerIndex))
            {
                _isConfirmSelected = !_isConfirmSelected;
            }
            else if (_menuSelect.Occurred(input, ControllingPlayer, out playerIndex))
            {
                if (_isConfirmSelected)
                    Accepted?.Invoke(this, new PlayerIndexEventArgs(playerIndex));
                else
                    Cancelled?.Invoke(this, new PlayerIndexEventArgs(playerIndex));
                ExitScreen();
            }
            else if (_menuCancel.Occurred(input, ControllingPlayer, out playerIndex))
            {
                Cancelled?.Invoke(this, new PlayerIndexEventArgs(playerIndex));
                ExitScreen();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 0.7f);

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            // Replace single spaces with three consecutive spaces in message, confirmText, and cancelText
            string adjustedMessage = _message.Replace(" ", "   ");
            string adjustedConfirmText = _confirmText.Replace(" ", "   ");
            string adjustedCancelText = _cancelText.Replace(" ", "   ");

            Vector2 messageSize = font.MeasureString(adjustedMessage);
            float buttonTextSize = Math.Max(
                font.MeasureString(adjustedConfirmText).X,
                font.MeasureString(adjustedCancelText).X
            );

            float boxWidth = Math.Max(messageSize.X, buttonTextSize * 2.5f) + _padding.X * 2;
            float boxHeight = messageSize.Y + font.LineSpacing * 2 + _padding.Y * 2;

            Vector2 boxPosition = new(
                (viewport.Width - boxWidth) / 2,
                (viewport.Height - boxHeight) / 2
            );

            Vector2 messagePosition = new(
                boxPosition.X + _padding.X,
                boxPosition.Y + _padding.Y
            );

            spriteBatch.Begin();

            Rectangle boxRect = new((int)boxPosition.X, (int)boxPosition.Y, (int)boxWidth, (int)boxHeight);
            DrawRoundedRect(spriteBatch, boxRect, _backgroundColor * TransitionAlpha);
            spriteBatch.DrawString(font, adjustedMessage, messagePosition, _textColor * TransitionAlpha);

            float buttonY = boxPosition.Y + boxHeight - font.LineSpacing - _padding.Y;
            DrawButton(spriteBatch, font, adjustedConfirmText, new Vector2(boxPosition.X + boxWidth * 0.3f, buttonY), _isConfirmSelected);
            DrawButton(spriteBatch, font, adjustedCancelText, new Vector2(boxPosition.X + boxWidth * 0.7f, buttonY), !_isConfirmSelected);

            spriteBatch.End();
        }


        private void DrawButton(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, bool isSelected)
        {
            Vector2 size = font.MeasureString(text);
            position.X -= size.X / 2;
            spriteBatch.DrawString(font, text, position, (isSelected ? _selectedColor : _textColor) * TransitionAlpha);
        }

        private void DrawRoundedRect(SpriteBatch spriteBatch, Rectangle rect, Color color) =>
            spriteBatch.Draw(_backgroundTexture, rect, color); 
    }
}