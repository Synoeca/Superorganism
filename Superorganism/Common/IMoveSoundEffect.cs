using Microsoft.Xna.Framework;

namespace Superorganism.Common
{
	public interface IMoveSoundEffect
	{
		void PlayMoveSound(GameTime gameTime, float interval);
		void PlayJumpSound();
	}
}
