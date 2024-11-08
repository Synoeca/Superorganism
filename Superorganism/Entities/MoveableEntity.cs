using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Collisions;
using Superorganism.Enums;

namespace Superorganism.Entities
{
	public class MoveableEntity(Vector2 position) : Entity, IMovable
	{
		public override Texture2D Texture { get; set; }
		public override Color Color { get; set; }
		public virtual Vector2 Velocity { get; set; }
		public virtual Strategy Strategy { get; set; } = Strategy.Idle;

		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Texture, Position, Color);
		}


		public void Move(Vector2 direction)
		{
			Position += Velocity * direction;
			Direction = Math.Abs(direction.X) > Math.Abs(direction.Y)
				? direction.X > 0 ? Direction.Right : Direction.Left
				: direction.Y > 0 ? Direction.Down : Direction.Up;
		}

		public void ApplyGravity(float gravity)
		{
			Velocity += new Vector2(0, gravity);
		}

		private Direction _direction;
		public virtual Direction Direction
		{
			get => _direction;
			set => _direction = value;
		}

		private Vector2 _position = position;
		public override Vector2 Position
		{
			get => _position;
			set => _position = value;
		}

		private double _directionTimer = 0;
		public virtual double DirectionTimer
		{
			get => _directionTimer;
			set => _directionTimer = value;
		}

		private ICollisionBounding _collisionBounding = new BoundingCircle(position + new Vector2(16, 16), 16);
		public virtual ICollisionBounding CollisionBounding
		{
			get => _collisionBounding;
			set => _collisionBounding = value;
		}

		//public virtual bool IsSpriteAtlas { get; set; } = true;
		//public virtual bool HasDirection { get; set; } = true;
		public virtual double DirectionInterval { get; set; }
		//public virtual int NumOfSpriteCols { get; set; } = 4;
		//public virtual int NumOfSpriteRows { get; set; } = 4;
		//public virtual float AnimationSpeed { get; set; } = 0.1f;

		public override void Update(GameTime gameTime)
		{
			DecisionMaker.Action(Strategy, gameTime, ref _direction, ref _position, ref _directionTimer, DirectionInterval, ref _collisionBounding);
		}
	}
}
