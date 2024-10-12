using Superorganism.Collisions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism
{
	public interface ICollidable : IEntity
	{
		ICollisionBounding CollisionBounding { get; }
	}
}
