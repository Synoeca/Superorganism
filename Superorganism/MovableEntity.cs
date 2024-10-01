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
	public class MovableEntity : AnimatedEntity, IMoveable, ISoundEffect
	{

		private Vector2 _velocity = Vector2.Zero;
		private bool _flipped;

		private BoundingRectangle _bounds = new(new Vector2(200 - 16, 200 - 16), 32, 32);
		public BoundingRectangle Bounds => _bounds;

		protected bool _isOnGround = true;
		protected bool IsOnGround() => _isOnGround;

		private float _gravity = 0.5f;
		private float _groundLevel = 400;
		private float _jumpStrength = -14f;
		private float _movementSpeed = 3f;
		private float _friction = 0.8f;

		public Color Color { get; set; } = Color.White;

		private GamePadState _gamePadState;
		private KeyboardState _keyboardState;
		protected Texture2D _texture1;
		protected Texture2D _texture2;

		protected SoundEffect _moveSound;
		protected SoundEffect _jumpSound;
		protected float _soundTimer = 0.0f;
		protected float _moveSoundInterval = 0.25f;
		protected float _shiftMoveSoundInterval = 0.15f;
		protected bool _isJumping = false;

		// Velocity property from IMoveable
		public virtual Vector2 Velocity { get; set; }

		// Constructor
		//public MovableEntity(Vector2 initialPosition) : base(initialPosition) { }

		// Implement IMoveable methods
		public virtual void Move(Vector2 direction)
		{
			Velocity += direction;
		}

		public virtual void ApplyGravity(float gravity)
		{
			Velocity = new Vector2(Velocity.X, Velocity.Y + gravity);
		}

		//public virtual void UpdateAnimation(GameTime gameTime, float movementSpeed)
		//{
		//	// Implement animation logic for movable entities
		//	// For example, you could change frames based on movement speed
		//}

		//public virtual void DrawAnimation(SpriteBatch spriteBatch)
		//{
		//	// Draw the current texture with its position
		//	spriteBatch.Draw(_currentTexture, Position, Color.White);
		//}

		// Update the entity's state
		public virtual void Update(GameTime gameTime)
		{
			//// Call the base class update for sound timer management
			//UpdateSoundTimer(gameTime);
			//// Update position based on velocity
			//Position += Velocity;
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
					_soundTimer = 0.0f;
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

		// Load content for the entity
		public virtual void LoadContent(ContentManager content)
		{
			//// Load the textures or other content needed for the entity
			//// Example:
			//_currentTexture = content.Load<Texture2D>("YourTextureName");
			//// Load sound effects if needed
			//_moveSound = content.Load<SoundEffect>("YourMoveSound");
			//_jumpSound = content.Load<SoundEffect>("YourJumpSound");
			_texture1 = content.Load<Texture2D>("ant");
			_texture2 = content.Load<Texture2D>("ant2");
			_currentTexture = _texture1;

			_moveSound = content.Load<SoundEffect>("move");
			_jumpSound = content.Load<SoundEffect>("jump");
		}

		// Draw the entity including animations
		public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			DrawAnimation(spriteBatch);
		}

		public override void UpdateAnimation(GameTime gameTime, float movementSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override void DrawAnimation(SpriteBatch spriteBatch)
		{
			SpriteEffects spriteEffects = (_flipped) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			spriteBatch.Draw(_currentTexture, _position, null, Color, 0, new Vector2(120, 120), 0.25f, spriteEffects, 0);
		}

		private float GetMoveSoundInterval()
		{
			return (_keyboardState.IsKeyDown(Keys.LeftShift) || _keyboardState.IsKeyDown(Keys.RightShift)) ? _shiftMoveSoundInterval : _moveSoundInterval;
		}

		//// Implement sound effect methods
		//public virtual void PlayMoveSound()
		//{
		//	if (_moveSound != null && _soundTimer <= 0)
		//	{
		//		_moveSound.Play();
		//		_soundTimer = _moveSoundInterval; // Reset the sound timer to interval
		//	}
		//}

		public virtual void PlayMoveSound(GameTime gameTime, float interval)
		{
			_soundTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (_soundTimer >= interval)
			{
				_moveSound.Play();
				_soundTimer = 0.0f;
			}
		}

		public virtual void PlayJumpSound()
		{
			if (_jumpSound != null)
			{
				_jumpSound.Play();
			}
		}

		// Update method to manage sound timer
		public void UpdateSoundTimer(GameTime gameTime)
		{
			if (_soundTimer > 0)
			{
				_soundTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
			}
		}

		public void PlayMoveSound()
		{
			throw new NotImplementedException();
		}
	}
}
