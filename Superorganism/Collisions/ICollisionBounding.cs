using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism.Collisions
{
	public interface ICollisionBounding
	{
		bool CollidesWith(ICollisionBounding other);
	}
}
