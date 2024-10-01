using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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
			if (other is BoundingRectangle otherRectangle)
			{
				return CollisionHelper.Collides(this, otherRectangle);
			}
			else if (other is BoundingCircle otherCircle)
			{
				return CollisionHelper.Collides(otherCircle, this);
			}
			return false;
		}
	}
}
