using Microsoft.Xna.Framework;

namespace Superorganism.Collisions
{
	public interface ICollisionBounding
	{
		Vector2 Center { get; set; }

		bool CollidesWith(ICollisionBounding other);
	}
}
