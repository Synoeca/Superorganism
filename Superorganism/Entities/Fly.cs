using Superorganism.Collisions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Superorganism.Enums;

namespace Superorganism.Entities
{
	public class Fly : MovableAnimatedDestroyableEntity
	{
		public override EntityStatus EntityStatus { get; set; } = new ()
		{
			Agility = 1
		};
		public override Strategy Strategy { get; set; } = Strategy.RandomFlyingMovement;

		public override bool IsSpriteAtlas { get; set; } = true;
		public override bool HasDirection { get; set; } = true;
		public override double DirectionInterval { get; set; } = 2.0;
		public override float AnimationSpeed { get; set; } = 0.1f;
	}
}
