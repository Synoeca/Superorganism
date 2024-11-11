using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Superorganism.Interfaces
{
    public interface IControllable : IMovable
    {
        bool IsControlled { get; }

        void HandleInput(KeyboardState keyboardState, GamePadState gamePadState, GameTime gameTime);
    }
}
