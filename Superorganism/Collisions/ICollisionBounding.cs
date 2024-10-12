using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Superorganism.Collisions
{
	public interface ICollisionBounding
	{
		Vector2 Center { get; set; }

		bool CollidesWith(ICollisionBounding other);
	}
}
