using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Superorganism.Collisions;

namespace Superorganism;

public class ControllableEntity : MovableEntity, IControllable, IMoveSoundEffect
{
	private BoundingRectangle _bounds = new(new Vector2(200 - 16, 200 - 16), 32, 32);
	private bool _flipped;
	private readonly float _friction = 0.8f;

	private GamePadState _gamePadState;

	private readonly float _gravity = 0.5f;
	private readonly float _groundLevel = 400;

	private bool _isOnGround = true;
	private readonly float _jumpStrength = -14f;
	private KeyboardState _keyboardState;
	private float _movementSpeed = 3f;
	private Vector2 _velocity = Vector2.Zero;
	protected bool IsJumping;
	protected SoundEffect JumpSound;

	protected SoundEffect MoveSound;
	protected float MoveSoundInterval = 0.25f;
	protected float ShiftMoveSoundInterval = 0.15f;
	protected float SoundTimer;
	protected Texture2D Texture1;
	protected Texture2D Texture2;

	public ControllableEntity(Vector2 position) : base(position)
	{
		_position = position;
	}

	public new BoundingRectangle Bounds => _bounds;

	public new Color Color { get; set; } = Color.White;

	// Velocity property from IMoveable
	public new virtual Vector2 Velocity { get; set; }

	public virtual void PlayMoveSound(GameTime gameTime, float interval)
	{
		SoundTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
		if (!(SoundTimer >= interval)) return;
		MoveSound.Play();
		SoundTimer = 0.0f;
	}

	public virtual void PlayJumpSound()
	{
		JumpSound?.Play();
	}

	protected bool IsOnGround()
	{
		return _isOnGround;
	}

	// Constructor
	//public MovableEntity(Vector2 initialPosition) : base(initialPosition) { }

	// Implement IMoveable methods
	public new virtual void Move(Vector2 direction)
	{
		Velocity += direction;
	}

	public new virtual void ApplyGravity(float gravity)
	{
		Velocity = new Vector2(Velocity.X, Velocity.Y + gravity);
	}

	public void HandleInput(KeyboardState keyboardState, GamePadState gamePadState, GameTime gameTime)
	{
		_gamePadState = GamePad.GetState(0);
		_keyboardState = Keyboard.GetState();
		_movementSpeed = _keyboardState.IsKeyDown(Keys.LeftShift) || _keyboardState.IsKeyDown(Keys.RightShift)
			? 2.5f
			: 1f;

		if (_isOnGround && _keyboardState.IsKeyDown(Keys.Space))
		{
			_velocity.Y = _jumpStrength;
			_isOnGround = false;
			IsJumping = true;
			//JumpSound.Play();
		}

		if (_keyboardState.IsKeyDown(Keys.Left) || _keyboardState.IsKeyDown(Keys.A))
		{
			_velocity.X = -_movementSpeed;
			_flipped = true;
			//if (!IsJumping) PlayMoveSound(gameTime, GetMoveSoundInterval());
		}
		else if (_keyboardState.IsKeyDown(Keys.Right) || _keyboardState.IsKeyDown(Keys.D))
		{
			_velocity.X = _movementSpeed;
			_flipped = false;
			//if (!IsJumping) PlayMoveSound(gameTime, GetMoveSoundInterval());
		}
		else
		{
			if (_isOnGround)
			{
				_velocity.X *= _friction;
				if (Math.Abs(_velocity.X) < 0.1f) _velocity.X = 0;
			}

			//if (SoundTimer > 0 && _velocity.X == 0) SoundTimer = 0.0f;
		}

		_velocity.Y += _gravity;

		_position += _velocity;

		if (_position.Y >= _groundLevel)
		{
			_position.Y = _groundLevel;
			_velocity.Y = 0;
			_isOnGround = true;

			if (IsJumping) IsJumping = false;
		}

		_bounds.X = _position.X - 16;
		_bounds.Y = _position.Y - 16;

		_velocity.X = MathHelper.Clamp(_velocity.X, -_movementSpeed * 2, _movementSpeed * 2);

		switch (_isOnGround)
		{
			case true when Math.Abs(_velocity.X) > 0:
			{
				AnimationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
				AnimationInterval = 0.15f / Math.Abs(_velocity.X);

				if (AnimationTimer >= AnimationInterval)
				{
					CurrentTexture = CurrentTexture == Texture1 ? Texture2 : Texture1;
					AnimationTimer = 0f;
				}

				break;
			}
			case false:
				CurrentTexture = Texture1;
				break;
		}
	}


	// Update the entity's state
	public virtual void Update(GameTime gameTime)
	{
		HandleInput(_keyboardState, _gamePadState, gameTime);
	}

	// Load content for the entity
	public override void LoadContent(ContentManager content)
	{
		Texture1 = content.Load<Texture2D>("ant");
		Texture2 = content.Load<Texture2D>("ant2");
		CurrentTexture = Texture1;

		MoveSound = content.Load<SoundEffect>("move");
		JumpSound = content.Load<SoundEffect>("jump");
	}

	// Draw the entity including animations
	public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
	{
		DrawAnimation(spriteBatch);
	}

	public override void UpdateAnimation(GameTime gameTime)
	{
		throw new NotImplementedException();
	}

	public override void DrawAnimation(SpriteBatch spriteBatch)
	{
		SpriteEffects spriteEffects = _flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
		spriteBatch.Draw(CurrentTexture, _position, null, Color, 0, new Vector2(120, 120), 0.25f, spriteEffects, 0);
	}

	private float GetMoveSoundInterval()
	{
		return _keyboardState.IsKeyDown(Keys.LeftShift) || _keyboardState.IsKeyDown(Keys.RightShift)
			? ShiftMoveSoundInterval
			: MoveSoundInterval;
	}

	// Update method to manage sound timer
	public void UpdateSoundTimer(GameTime gameTime)
	{
		if (SoundTimer > 0) SoundTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
	}
}