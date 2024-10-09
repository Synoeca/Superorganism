using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Collisions;

namespace Superorganism;

public class MovableEntity : AnimatedEntity, IMovable
{
	private readonly float _acceleration = 0.12f;
	private float _animationInterval = 0.15f;
	private float _animationTimer;

	private BoundingRectangle _bounds = new(new Vector2(200 - 16, 200 - 16), 32, 32);
	private readonly float _chaseThreshold = 300f;

	private bool _flipped;
	private readonly float _friction = 0.8f;

	private readonly float _gravity = 0.5f;
	private readonly float _groundLevel = 400;
	private bool _isOnGround = true;
	private readonly float _movementSpeed = 2.5f;

	private bool _movingRight = true;
	private new Vector2 _position = new(550, 400);

	private Texture2D _texture1;
	private Texture2D _texture2;
	private Texture2D _texture3;
	private Vector2 _velocity = Vector2.Zero;
	protected Texture2D CurrentTexture;

	public MovableEntity(Vector2 position) : base(position)
	{
		_position = position;
	}

	public new BoundingRectangle Bounds => _bounds;

	public Color Color { get; set; } = Color.White;
	public Vector2 Velocity { get; set; }

	public override void LoadContent(ContentManager content)
	{
		_texture1 = content.Load<Texture2D>("antEnemy");
		_texture2 = content.Load<Texture2D>("antEnemy2");
		_texture3 = content.Load<Texture2D>("antEnemy3");
		CurrentTexture = _texture1;
	}

	public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
	{
		SpriteEffects spriteEffects = _flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
		spriteBatch.Draw(CurrentTexture, _position, null, Color, 0, new Vector2(120, 120), 0.25f, spriteEffects, 0);
	}

	public void Move(Vector2 direction)
	{
		throw new NotImplementedException();
	}

	public void ApplyGravity(float gravity)
	{
		throw new NotImplementedException();
	}

	public void Update(GameTime gameTime, Vector2 playerPosition)
	{
		float distanceToPlayerX = Math.Abs(_position.X - playerPosition.X);

		if (distanceToPlayerX < _chaseThreshold)
		{
			float targetVelocityX = playerPosition.X > _position.X ? _movementSpeed : -_movementSpeed;
			_velocity.X = MathHelper.Lerp(_velocity.X, targetVelocityX, _acceleration);
		}
		else
		{
			float targetVelocityX = _movingRight ? _movementSpeed : -_movementSpeed;
			_velocity.X = MathHelper.Lerp(_velocity.X, targetVelocityX, _acceleration);

			_movingRight = _position.X switch
			{
				<= 100 => true,
				>= 700 => false,
				_ => _movingRight
			};
		}

		_flipped = _velocity.X < 0;

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

		switch (_isOnGround)
		{
			case true when Math.Abs(_velocity.X) > 0:
			{
				_animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
				_animationInterval = 0.15f / Math.Abs(_velocity.X);

				if (_animationTimer >= _animationInterval)
				{
					CurrentTexture = CurrentTexture == _texture2 ? _texture3 : _texture2;
					_animationTimer = 0f;
				}

				break;
			}
			case false:
			case true when Math.Abs(_velocity.X) == 0:
				CurrentTexture = _texture1;
				break;
		}

		if (!_isOnGround || !(distanceToPlayerX >= _chaseThreshold)) return;
		_velocity.X *= _friction;
		if (Math.Abs(_velocity.X) < 0.1f) _velocity.X = 0;
	}
}