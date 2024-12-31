using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Superorganism.Collisions;

namespace Superorganism.Interfaces
{
    public interface ICollidable : IEntity
    {
        ICollisionBounding CollisionBounding { get; }
    }
}
