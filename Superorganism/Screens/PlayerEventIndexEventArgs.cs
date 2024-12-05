using System;
using Microsoft.Xna.Framework;

namespace Superorganism.Screens
{
    // Custom event argument which includes the index of the player who
    // triggered the event. This is used by the MenuEntry.Selected event.
    public class PlayerIndexEventArgs : EventArgs
    {
        public PlayerIndex PlayerIndex { get; }
        public int Direction { get; }

        public PlayerIndexEventArgs(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
            Direction = 0;
        }

        public PlayerIndexEventArgs(int direction, PlayerIndex playerIndex)
        {
            Direction = direction;
            PlayerIndex = playerIndex;
        }
    }
}
