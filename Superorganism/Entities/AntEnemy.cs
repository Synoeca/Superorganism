using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Superorganism.AI;
using Superorganism.Common;

namespace Superorganism.Entities
{
	public sealed class AntEnemy : MovableAnimatedDestroyableEntity
	{
		public AntEnemy()
		{
			Strategy = Strategy.Patrol;
			StrategyHistory.Add((Strategy.Patrol, 0, 0));
			Color = Color.White;
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
