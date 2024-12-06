using System;
using Microsoft.Xna.Framework;

namespace Superorganism.Screens
{
    // Custom event argument which includes the index of the player who
    // triggered the event. This is used by the MenuEntry.Selected event.
    public class PlayerIndexEventArgs(int direction, PlayerIndex playerIndex) : EventArgs
    {
        public PlayerIndex PlayerIndex { get; } = playerIndex;
        public int Direction { get; } = direction;

        public PlayerIndexEventArgs(PlayerIndex playerIndex) : this(0, playerIndex) {}
    }
}
