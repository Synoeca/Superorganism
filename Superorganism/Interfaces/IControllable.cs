using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Superorganism.Interfaces
{
    public interface IControllable : IMovable
    {
        bool IsControlled { get; }

        void HandleInput(KeyboardState keyboardState, GamePadState gamePadState, GameTime gameTime);
    }
}
