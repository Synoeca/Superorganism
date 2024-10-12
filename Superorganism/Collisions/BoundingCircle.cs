using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Superorganism.Collisions
{
	/// <summary>
	/// A struct representing circular bounds
	/// </summary>
	public struct BoundingCircle : ICollisionBounding
	{
		/// <summary>
		/// The center of the BoundingCircle
		/// </summary>
		public Vector2 Center { get; set; }

		/// <summary>
		/// The radius of the BoundingCircle
		/// </summary>
		public float Radius;

		/// <summary>
		/// Constructs a new Bounding Circle
		/// </summary>
		/// <param name="center">The center</param>
		/// <param name="radius">The radius</param>
		public BoundingCircle(Vector2 center, float radius)
		{
			Center = center;
			Radius = radius;
		}

		/// <summary>
		/// Tests for a collision between this and another bounding circle
		/// </summary>
		/// <param name="other">The other bounding circle</param>
		/// <returns>true for collision, false otherwise</returns>
		public bool CollidesWith(ICollisionBounding other)
		{
			if (other is BoundingCircle otherCircle)
			{
				return CollisionHelper.Collides(this, otherCircle);
			}
			else if (other is BoundingRectangle otherRectangle)
			{
				return CollisionHelper.Collides(this, otherRectangle);
			}
			return false;
		}
	}
}
