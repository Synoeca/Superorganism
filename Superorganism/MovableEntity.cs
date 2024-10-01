using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Superorganism;
using Superorganism.Collisions;
using System;

namespace Superorganism
{
	public class MovableEntity : AnimatedEntity, IMovable
	{
		private new float _animationTimer = 0f;
		private new float _animationInterval = 0.15f;
		protected Texture2D _currentTexture;

		private Texture2D _texture1;
		private Texture2D _texture2;
		private Texture2D _texture3;
		private new Vector2 _position = new(550, 400);
		private Vector2 _velocity = Vector2.Zero;
		public Vector2 Velocity { get; set; }

		private bool _flipped;
		private bool _isOnGround = true;

		private float _gravity = 0.5f;
		private float _groundLevel = 400;
		private float _movementSpeed = 2.5f;
		private float _friction = 0.8f;
		private float _chaseThreshold = 300f;
		private float _acceleration = 0.12f;

		private BoundingRectangle _bounds = new BoundingRectangle(new Vector2(200 - 16, 200 - 16), 32, 32);
		public new BoundingRectangle Bounds => _bounds;

		private bool _movingRight = true;

		public Color Color { get; set; } = Color.White;

		public MovableEntity(Vector2 position) : base(position)
		{
			this._position = position;
		}

		public override void LoadContent(ContentManager content)
		{
			_texture1 = content.Load<Texture2D>("antEnemy");
			_texture2 = content.Load<Texture2D>("antEnemy2");
			_texture3 = content.Load<Texture2D>("antEnemy3");
			_currentTexture = _texture1;
		}

		public void Update(GameTime gameTime, Vector2 playerPosition)
		{
			float distanceToPlayerX = Math.Abs(_position.X - playerPosition.X);

			if (distanceToPlayerX < _chaseThreshold)
			{
				float targetVelocityX = playerPosition.X > _position.X ? _movementSpeed : -_movementSpeed;
				_velocity.X = MathHelper.Lerp(_velocity.X, targetVelocityX, _acceleration);
				_flipped = _velocity.X < 0;
			}
			else
			{
				float targetVelocityX = _movingRight ? _movementSpeed : -_movementSpeed;
				_velocity.X = MathHelper.Lerp(_velocity.X, targetVelocityX, _acceleration);

				if (_position.X <= 100) _movingRight = true;
				else if (_position.X >= 700) _movingRight = false;

				_flipped = _velocity.X < 0;
			}

			_velocity.Y += _gravity;

			_position += _velocity;

			if (_position.Y >= _groundLevel)
			{
				_position.Y = _groundLevel;
				_velocity.Y = 0;
				_isOnGround = true;
			}
			else
			{
				_isOnGround = false;
			}

			_bounds.X = _position.X - 16;
			_bounds.Y = _position.Y - 16;

			if (_isOnGround && Math.Abs(_velocity.X) > 0)
			{
				_animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
				_animationInterval = 0.15f / Math.Abs(_velocity.X);

				if (_animationTimer >= _animationInterval)
				{
					_currentTexture = (_currentTexture == _texture2) ? _texture3 : _texture2;
					_animationTimer = 0f;
				}
			}
			else if (!_isOnGround)
			{
				_currentTexture = _texture1;
			}
			else if (_isOnGround && Math.Abs(_velocity.X) == 0)
			{
				_currentTexture = _texture1;
			}

			if (_isOnGround && distanceToPlayerX >= _chaseThreshold)
			{
				_velocity.X *= _friction;
				if (Math.Abs(_velocity.X) < 0.1f)
				{
					_velocity.X = 0;
				}
			}
		}

		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			SpriteEffects spriteEffects = (_flipped) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			spriteBatch.Draw(_currentTexture, _position, null, Color, 0, new Vector2(120, 120), 0.25f, spriteEffects, 0);
		}

		public void Move(Vector2 direction)
		{
			throw new NotImplementedException();
		}

		public void ApplyGravity(float gravity)
		{
			throw new NotImplementedException();
		}
	}
}
