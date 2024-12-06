using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.ScreenManagement;

namespace Superorganism.Screens
{
    public class InstructionEntry
    {
        private const float FONT_SCALE = 0.8f;

        public string Text { get; set; } = string.Empty;

        public Vector2 Position { get; set; }

        public InstructionEntry(string text)
        {
            Text = text;
        }

        public void Draw(GameScreen screen, bool isSelected = false)
        {
            SpriteBatch spriteBatch = screen.ScreenManager.SpriteBatch;
            SpriteFont font = screen.ScreenManager.Font;
            const float shadowOffset = 2f;
            Color textColor = isSelected ? Color.Yellow : Color.White;

            // Replace spaces with three consecutive spaces
            string adjustedText = Text.Replace(" ", "   ");

            spriteBatch.DrawString(font, adjustedText,
                Position + new Vector2(shadowOffset),
                Color.Black * 0.8f * screen.TransitionAlpha,
                0, Vector2.Zero, FONT_SCALE, SpriteEffects.None, 0);

            spriteBatch.DrawString(font, adjustedText,
                Position,
                textColor * screen.TransitionAlpha,
                0, Vector2.Zero, FONT_SCALE, SpriteEffects.None, 0);
        }

        public int GetHeight(ScreenManager screenManager) =>
            (int)(screenManager.Font.LineSpacing * FONT_SCALE);

        public int GetWidth(ScreenManager screenManager) =>
            (int)(screenManager.Font.MeasureString(Text).X * FONT_SCALE);
    }
}