﻿using System;
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
		public override EntityStatus EntityStatus { get; set; }
		public override Color Color { get; set; }
		public virtual Strategy Strategy { get; set; } = Strategy.Idle;

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

		private Vector2 _velocity;
		public Vector2 Velocity
		{
			get => _velocity;
			set => _velocity = value;
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

		private double _directionInterval;
		public virtual double DirectionInterval
		{
			get => _directionInterval;
			set => _directionInterval = value;
		}

		private ICollisionBounding _collisionBounding = new BoundingCircle(position + new Vector2(16, 16), 16);
		public virtual ICollisionBounding CollisionBounding
		{
			get => _collisionBounding;
			set => _collisionBounding = value;
		}

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

		public override void Update(GameTime gameTime)
		{
			DecisionMaker.Action(Strategy, gameTime, ref _direction, ref _position, ref _directionTimer, ref _directionInterval, ref _collisionBounding,
				ref _velocity, 800, 600, TextureInfo, EntityStatus);
		}
	}
}
