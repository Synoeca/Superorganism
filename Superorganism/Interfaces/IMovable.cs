using Microsoft.Xna.Framework;
using Superorganism.Enums;

namespace Superorganism.Interfaces
{
    public interface IMovable : IEntity
    {
        Vector2 Velocity { get; }
        Direction Direction { get; }

        void Move(Vector2 direction);
        void ApplyGravity(float gravity);
    }
}
