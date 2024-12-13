using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.ScreenManagement;

namespace Superorganism.Screens
{
    public class SaveFileEntry
    {
        private const float FontScale = 0.8f;
        public string Text { get; }
        public string FileName { get; }
        public Vector2 Position { get; set; }
        public bool IsValid { get; }

        public SaveFileEntry(string text, string fileName, bool isValid = true)
        {
            Text = text;
            FileName = fileName;
            IsValid = isValid;
        }

        public void Draw(GameScreen screen, bool isSelected)
        {
            SpriteBatch spriteBatch = screen.ScreenManager.SpriteBatch;
            SpriteFont font = screen.ScreenManager.Font;
            const float shadowOffset = 2f;

            Color textColor = isSelected ? Color.Yellow : (IsValid ? Color.White : Color.Gray);

            spriteBatch.DrawString(font, Text,
                Position + new Vector2(shadowOffset),
                Color.Black * 0.8f * screen.TransitionAlpha,
                0, Vector2.Zero, FontScale, SpriteEffects.None, 0);

            spriteBatch.DrawString(font, Text,
                Position,
                textColor * screen.TransitionAlpha,
                0, Vector2.Zero, FontScale, SpriteEffects.None, 0);
        }

        public int GetHeight(ScreenManager screenManager) =>
            (int)(screenManager.Font.LineSpacing * FontScale);

        public int GetWidth(ScreenManager screenManager) =>
            (int)(screenManager.Font.MeasureString(Text).X * FontScale);
    }
}