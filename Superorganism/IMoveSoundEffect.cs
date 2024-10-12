using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism
{
	public interface IMoveSoundEffect
	{
		void PlayMoveSound(GameTime gameTime, float interval);
		void PlayJumpSound();
	}
}
