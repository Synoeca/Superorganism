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
	public class Fly(Vector2 position) : MovableAnimatedDestroyableEntity(position)
	{
		public override Strategy Strategy { get; set; } = Strategy.RandomMovement;
		//private Direction _direction;
		//public override Direction Direction
		//{
		//	get => _direction;
		//	set => _direction = value;
		//}

		//private Vector2 _position;
		//public override Vector2 Position
		//{
		//	get => _position;
		//	set => _position = value;
		//}

		//private double _directionTimer = 0;
		//public override double DirectionTimer
		//{
		//	get => _directionTimer;
		//	set => _directionTimer = value;
		//}

		//private ICollisionBounding _collisionBounding = new BoundingCircle(position + new Vector2(16, 16), 16);
		//public override ICollisionBounding CollisionBounding
		//{
		//	get => _collisionBounding;
		//	set => _collisionBounding = value;
		//}

		public override bool IsSpriteAtlas { get; set; } = true;
		public override bool HasDirection { get; set; } = true;
		public override double DirectionInterval { get; set; } = 2.0;
		//public override int NumOfSpriteCols { get; set; } = 4;
		//public override int NumOfSpriteRows { get; set; } = 4;
		public override float AnimationSpeed { get; set; } = 0.1f;

		//public void Update(GameTime gameTime)
		//{
		//	base.Update(gameTime);
		//}

		//public override void Update(GameTime gameTime)
		//{
		//	DecisionMaker.Action(Strategy.RandomMovement, gameTime, ref _direction, ref _position, ref _directionTimer, DirectionInterval, ref _collisionBounding);
		//}
	}
}
