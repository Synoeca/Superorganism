using System;
using Microsoft.Xna.Framework;

namespace Superorganism.Screens
{
    // Helper class represents a single entry in a MenuScreen. By default, this
    // just draws the entry text string, but it can be customized to display menu
    // entries in different ways. This also provides an event that will be raised
    // when the menu entry is selected.
    public class MenuEntry(string text)
    {
        private float _selectionFade;    // Entries transition out of the selection effect when they are deselected

        public string Text { get; set; } = text;

        public Vector2 Position { get; set; }

        public event EventHandler<PlayerIndexEventArgs> AdjustValue;
        protected internal virtual void OnAdjustValue(int direction, PlayerIndex playerIndex)
        {
            AdjustValue?.Invoke(this, new PlayerIndexEventArgs(direction, playerIndex));
        }

        public event EventHandler<PlayerIndexEventArgs> Selected;
        protected internal virtual void OnSelectEntry(PlayerIndex playerIndex)
        {
            Selected?.Invoke(this, new PlayerIndexEventArgs(playerIndex));
        }

        public virtual void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
        {
            // When the menu selection changes, entries gradually fade between
            // their selected and deselected appearance, rather than instantly
            // popping to the new state.
            float fadeSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 4;

            _selectionFade = isSelected ? 
                Math.Min(_selectionFade + fadeSpeed, 1) : Math.Max(_selectionFade - fadeSpeed, 0);
        }

        public virtual int GetHeight(MenuScreen screen)
        {
            return screen.ScreenManager.Font.LineSpacing;
        }

        public virtual int GetWidth(MenuScreen screen)
        {
            return (int)screen.ScreenManager.Font.MeasureString(Text).X;
        }
    }
}
