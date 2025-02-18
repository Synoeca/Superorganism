using Superorganism.Collisions;
using Superorganism.AI;
using Superorganism.Common;

namespace Superorganism.Entities
{
	public sealed class Fly : MovableAnimatedDestroyableEntity
	{
		public Fly()
		{
			Strategy = Strategy.Random360FlyingMovement;
            StrategyHistory.Add((Strategy.Random360FlyingMovement, 0, 0));
            if (CollisionBounding is BoundingCircle bc)
            {
                bc.Radius *= 0.8f;
            }
            UseRotation = true;
        }
		public override EntityStatus EntityStatus { get; set; } = new ()
		{
			Agility = 1
		};

		public override bool IsSpriteAtlas { get; set; } = true;
		public override bool HasDirection { get; set; } = true;
		public override double DirectionInterval { get; set; } = 2.0;
		public override float AnimationSpeed { get; set; } = 0.1f;
	}
}
