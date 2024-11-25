using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism.Entities
{
	public sealed class AntEnemy : MovableAnimatedDestroyableEntity
	{
		public AntEnemy()
		{
			((MovableEntity)this).Strategy = global::Superorganism.Strategy.Patrol;
		}

		public override EntityStatus EntityStatus { get; set; } = new()
		{
			Agility = 1
		};
		

		public override bool IsSpriteAtlas { get; set; } = true;
		public override bool HasDirection { get; set; } = false;
		public override double DirectionInterval { get; set; } = 2.0;
		public override float AnimationSpeed { get; set; } = 0.1f;
	}
}
