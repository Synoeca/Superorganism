using Superorganism.Collisions;

namespace Superorganism.Interfaces
{
    public interface ICollidable : IEntity
    {
        ICollisionBounding CollisionBounding { get; }
    }
}
