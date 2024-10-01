using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism
{
	public interface IControllable : IMoveable
	{

		void HandleInput(KeyboardState keyboardState, GamePadState gamePadState);
	}
}
