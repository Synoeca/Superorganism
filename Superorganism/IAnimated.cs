using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism
{
	public interface IAnimated
	{
		void UpdateAnimation(GameTime gameTime);
		void DrawAnimation(SpriteBatch spriteBatch);
	}
}
