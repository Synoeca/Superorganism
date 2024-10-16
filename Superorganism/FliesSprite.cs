﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using Superorganism.Collisions;
using Superorganism.Enums;

namespace Superorganism
{

    public class FliesSprite
	{
		public event Action<Vector2> OnDestroyed;

		private Texture2D _texture;
		private double _directionTimer;
		private double _animationTimer;
		private short _animationFrame = 1;
		private double _directionChangeInterval;
		private Vector2 _velocity;
		private static Random _rand = new();

		private BoundingCircle _bounds;

		public bool Destroyed { get; set; } = false;

		private Vector2 _position;
		public Vector2 Position
		{
			get => _position;
			set
			{
				_position = value;
				_bounds.Center = _position + new Vector2(16, 16); 
			}
		}

		public BoundingCircle Bounds => _bounds;

		public Direction Direction { get; set; }

		public FliesSprite()
		{
			_directionChangeInterval = _rand.NextDouble() * 3.0 + 1.0;
			AssignRandomVelocity();
			_position = Vector2.Zero; // Initialize position
			_bounds = new BoundingCircle(_position + new Vector2(16, 16), 16); // Adjust radius as needed
		}

		public void LoadContent(ContentManager content)
		{
			_texture = content.Load<Texture2D>("flies");
		}

		private void AssignRandomVelocity()
		{
			double angle = _rand.NextDouble() * Math.PI * 2;
			_velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 100;
		}

		public void Update(GameTime gameTime, int screenWidth, int groundHeight)
		{
			if (Destroyed) return; // Skip update if destroyed

			_directionTimer += gameTime.ElapsedGameTime.TotalSeconds;

			if (_directionTimer > _directionChangeInterval)
			{
				AssignRandomVelocity();
				_directionTimer -= _directionChangeInterval;
				_directionChangeInterval = _rand.NextDouble() * 3.0 + 1.0;
			}

			Position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

			if (Position.X < 0 || Position.X > screenWidth - 32)
			{
				_velocity.X = -_velocity.X;
				Position = new Vector2(Math.Clamp(Position.X, 0, screenWidth - 32), Position.Y);
			}

			if (Position.Y < 0 || Position.Y > groundHeight - 32)
			{
				_velocity.Y = -_velocity.Y;
				Position = new Vector2(Position.X, Math.Clamp(Position.Y, 0, groundHeight - 32));
			}

			Direction = Math.Abs(_velocity.X) > Math.Abs(_velocity.Y)
				? _velocity.X > 0 ? Direction.Right : Direction.Left
				: _velocity.Y > 0 ? Direction.Down : Direction.Up;
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			if (Destroyed) { return; }

			_animationTimer += gameTime.ElapsedGameTime.TotalSeconds;

			if (_animationTimer > 0.04)
			{
				_animationFrame++;
				if (_animationFrame > 3)
				{
					_animationFrame = 1;
				}
				_animationTimer -= 0.04;
			}

			float rotation = (float)Math.Atan2(_velocity.Y, _velocity.X);
			int directionIndex = Direction switch
			{
				Direction.Up => (int)Direction.Down,
				Direction.Down => (int)Direction.Up,
				_ => (int)Direction
			};

			Rectangle source = new Rectangle(_animationFrame * 32, directionIndex * 32, 32, 32);
			Rectangle destination = new Rectangle((int)Position.X, (int)Position.Y, 32, 32);
			spriteBatch.Draw(_texture, destination, source, Color.White, rotation, new Vector2(16, 16), SpriteEffects.None, 0f);

		}

		public bool CollidesWith(BoundingCircle other)
		{
			return _bounds.CollidesWith(other);
		}
	}
}
