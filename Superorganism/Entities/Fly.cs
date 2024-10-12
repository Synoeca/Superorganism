using Superorganism.Collisions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism.Entities
{
	public class Fly : MovableCollidableEntity
	{
		public override ICollisionBounding CollisionBounding { get; set; } = new BoundingRectangle();
	}
}
