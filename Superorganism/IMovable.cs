using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Superorganism.Enums;

namespace Superorganism
{
	public interface IMovable : IEntity
	{
		Vector2 Velocity { get; }
		Direction Direction { get; }

		void Move(Vector2 direction);
		void ApplyGravity(float gravity);
	}
}
