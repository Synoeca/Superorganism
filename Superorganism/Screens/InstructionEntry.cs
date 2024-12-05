using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.StateManagement;

namespace Superorganism.Screens;

public class InstructionEntry
{
    private string _text;
    private Vector2 _position;
    private const float FONT_SCALE = 0.8f;

    public string Text
    {
        get => _text;
        set => _text = value;
    }

    public Vector2 Position
    {
        get => _position;
        set => _position = value;
    }

    public InstructionEntry(string text) => _text = text;

    public void Draw(GameScreen screen, bool isSelected = false)
    {
        SpriteBatch spriteBatch = screen.ScreenManager.SpriteBatch;
        SpriteFont font = screen.ScreenManager.Font;
        const float shadowOffset = 2f;
        Color textColor = isSelected ? Color.Yellow : Color.White;

        spriteBatch.DrawString(font, _text,
            _position + new Vector2(shadowOffset),
            Color.Black * 0.8f * screen.TransitionAlpha,
            0, Vector2.Zero, FONT_SCALE, SpriteEffects.None, 0);

        spriteBatch.DrawString(font, _text,
            _position,
            textColor * screen.TransitionAlpha,
            0, Vector2.Zero, FONT_SCALE, SpriteEffects.None, 0);
    }

    public int GetHeight(ScreenManager screenManager) =>
        (int)(screenManager.Font.LineSpacing * FONT_SCALE);

    public int GetWidth(ScreenManager screenManager) =>
        (int)(screenManager.Font.MeasureString(Text).X * FONT_SCALE);
}