using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Superorganism
{
	public class MovementState
	{
		public Vector2 Position { get; set; }
		public Vector2 Velocity { get; set; } = Vector2.Zero;

		// State Flags
		public bool Flipped { get; set; }
		public bool IsOnGround { get; set; }
		public bool TowardRight { get; set; }

		// Movement Parameters
		public float Gravity { get; set; } = 0.5f;
		public float GroundLevel { get; set; }
		public float MovementSpeed { get; set; }
		public float Friction { get; set; }

		// Chase Parameters
		public float ChaseSpeed { get; set; }
		public float ChaseThreshold { get; set; }

		// Acceleration
		public float Acceleration { get; set; }
	}
}
