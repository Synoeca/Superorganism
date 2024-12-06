using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.ScreenManagement;

namespace Superorganism.Screens;

public class InstructionEntry(string text)
{
    private const float FONT_SCALE = 0.8f;

    public string Text { get; set; } = text;

    public Vector2 Position { get; set; }

    public void Draw(GameScreen screen, bool isSelected = false)
    {
        SpriteBatch spriteBatch = screen.ScreenManager.SpriteBatch;
        SpriteFont font = screen.ScreenManager.Font;
        const float shadowOffset = 2f;
        Color textColor = isSelected ? Color.Yellow : Color.White;

        spriteBatch.DrawString(font, Text,
            Position + new Vector2(shadowOffset),
            Color.Black * 0.8f * screen.TransitionAlpha,
            0, Vector2.Zero, FONT_SCALE, SpriteEffects.None, 0);

        spriteBatch.DrawString(font, Text,
            Position,
            textColor * screen.TransitionAlpha,
            0, Vector2.Zero, FONT_SCALE, SpriteEffects.None, 0);
    }

    public int GetHeight(ScreenManager screenManager) =>
        (int)(screenManager.Font.LineSpacing * FONT_SCALE);

    public int GetWidth(ScreenManager screenManager) =>
        (int)(screenManager.Font.MeasureString(Text).X * FONT_SCALE);
}