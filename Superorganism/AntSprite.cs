using CollisionExample.Collisions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Superorganism
{
	public class AntSprite
	{
		private GamePadState _gamePadState;
		private KeyboardState _keyboardState;

		private float _animationTimer = 0f;
		private float _animationInterval = 0.15f;
		private Texture2D _currentTexture;

		private Texture2D _texture1;
		private Texture2D _texture2;

		private Vector2 _position = new(200, 200);
		public Vector2 Position() => _position;

		private Vector2 _velocity = Vector2.Zero;
		private bool _flipped;

		private BoundingRectangle _bounds = new BoundingRectangle(new Vector2(200 - 16, 200 - 16), 32, 32);
		public BoundingRectangle Bounds => _bounds;

		private bool _isOnGround = true;
		public bool IsOnGround() => _isOnGround;

		private float _gravity = 0.5f;
		private float _groundLevel = 400;
		private float _jumpStrength = -14f;
		private float _movementSpeed = 3f;
		private float _friction = 0.8f;

		public Color Color { get; set; } = Color.White;

		private SoundEffect _moveSound;
		private SoundEffect _jumpSound;
		private float _moveSoundInterval = 0.25f;
		private float _shiftMoveSoundInterval = 0.15f;
		private double _soundTimer = 0.0;
		private bool _isJumping = false;

		public int HitPoint { get; set; } = 100;
		public int MaxHitPoint { get; private set; } = 100; // Maximum health

		public void LoadContent(ContentManager content)
		{
			_texture1 = content.Load<Texture2D>("ant");
			_texture2 = content.Load<Texture2D>("ant2");
			_currentTexture = _texture1;

			_moveSound = content.Load<SoundEffect>("move");
			_jumpSound = content.Load<SoundEffect>("jump");
		}

		public void Update(GameTime gameTime)
		{
			_gamePadState = GamePad.GetState(0);
			_keyboardState = Keyboard.GetState();

			_movementSpeed = (_keyboardState.IsKeyDown(Keys.LeftShift) || _keyboardState.IsKeyDown(Keys.RightShift)) ? 2.5f : 1f;

			if (_isOnGround && _keyboardState.IsKeyDown(Keys.Space))
			{
				_velocity.Y = _jumpStrength;
				_isOnGround = false;
				_isJumping = true;
				_jumpSound.Play();
			}

			if (_keyboardState.IsKeyDown(Keys.Left) || _keyboardState.IsKeyDown(Keys.A))
			{
				_velocity.X = -_movementSpeed;
				_flipped = true;
				if (!_isJumping)
				{
					PlayMoveSound(gameTime, GetMoveSoundInterval());
				}
			}
			else if (_keyboardState.IsKeyDown(Keys.Right) || _keyboardState.IsKeyDown(Keys.D))
			{
				_velocity.X = _movementSpeed;
				_flipped = false;
				if (!_isJumping)
				{
					PlayMoveSound(gameTime, GetMoveSoundInterval());
				}
			}
			else
			{
				if (_isOnGround)
				{
					_velocity.X *= _friction;
					if (Math.Abs(_velocity.X) < 0.1f)
					{
						_velocity.X = 0;
					}
				}

				if (_soundTimer > 0 && _velocity.X == 0)
				{
					_soundTimer = 0.0;
				}
			}

			_velocity.Y += _gravity;

			_position += _velocity;

			if (_position.Y >= _groundLevel)
			{
				_position.Y = _groundLevel;
				_velocity.Y = 0;
				_isOnGround = true;

				if (_isJumping)
				{
					_isJumping = false;
				}
			}

			_bounds.X = _position.X - 16;
			_bounds.Y = _position.Y - 16;

			_velocity.X = MathHelper.Clamp(_velocity.X, -_movementSpeed * 2, _movementSpeed * 2);

			if (_isOnGround && Math.Abs(_velocity.X) > 0)
			{
				_animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
				_animationInterval = 0.15f / Math.Abs(_velocity.X);

				if (_animationTimer >= _animationInterval)
				{
					_currentTexture = (_currentTexture == _texture1) ? _texture2 : _texture1;
					_animationTimer = 0f;
				}
			}
			else if (!_isOnGround)
			{
				_currentTexture = _texture1;
			}
		}

		private void PlayMoveSound(GameTime gameTime, float interval)
		{
			_soundTimer += gameTime.ElapsedGameTime.TotalSeconds;
			if (_soundTimer >= interval)
			{
				_moveSound.Play();
				_soundTimer = 0.0;
			}
		}

		private float GetMoveSoundInterval()
		{
			return (_keyboardState.IsKeyDown(Keys.LeftShift) || _keyboardState.IsKeyDown(Keys.RightShift)) ? _shiftMoveSoundInterval : _moveSoundInterval;
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			SpriteEffects spriteEffects = (_flipped) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			spriteBatch.Draw(_currentTexture, _position, null, Color, 0, new Vector2(120, 120), 0.25f, spriteEffects, 0);
		}
	}
}
