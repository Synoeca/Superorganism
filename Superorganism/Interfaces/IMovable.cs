using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
