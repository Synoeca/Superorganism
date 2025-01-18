using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Interfaces;

namespace Superorganism.Entities
{
    public class MovableAnimatedDestroyableEntity : MovableAnimatedEntity, ICollidable
	{
		public bool Destroyed { get; set; } = false;

		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			if (Destroyed)
			{
				return;
			}
			UpdateAnimation(gameTime);
			DrawAnimation(spriteBatch);
		}
	}
}
