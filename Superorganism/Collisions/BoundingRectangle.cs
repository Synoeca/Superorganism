using Microsoft.Xna.Framework;

namespace Superorganism.Collisions
{
	/// <summary>
	/// A bounding rectangle for collision detection
	/// </summary>
	public struct BoundingRectangle : ICollisionBounding
	{
		public float X;

		public float Y;

		public float Width;

		public float Height;

		public float Left => X;

		public float Right => X + Width;

		public float Top => Y;

		public float Bottom => Y + Height;

		public Vector2 Center { get; set; }

		public BoundingRectangle(float x, float y, float width, float height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public BoundingRectangle(Vector2 position, float width, float height)
		{
			X = position.X;
			Y = position.Y;
			Width = width;
			Height = height;
		}

		public bool CollidesWith(ICollisionBounding other)
        {
            return other switch
            {
                BoundingRectangle otherRectangle => CollisionHelper.Collides(this, otherRectangle),
                BoundingCircle otherCircle => CollisionHelper.Collides(otherCircle, this),
                _ => false
            };
        }
	}
}
