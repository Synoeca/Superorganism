using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Superorganism
{
	public interface IControllable : IMovable
	{
		void HandleInput(KeyboardState keyboardState, GamePadState gamePadState, GameTime gameTime);
	}
}
