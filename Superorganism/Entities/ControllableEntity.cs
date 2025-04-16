using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Superorganism.Collisions;
using Superorganism.Core.Managers;
using Superorganism.Interfaces;

namespace Superorganism.Entities
{
    public class ControllableEntity : MovableAnimatedDestroyableEntity, IControllable
	{
		public GamePadState GamePadState { get; set; }

		public KeyboardState KeyboardState { get; set; }

		public bool IsControlled { get; set; }

        private float _movementSpeed;
        public float MovementSpeed
        {
            get => _movementSpeed;
            set => _movementSpeed = value;
        }

        public bool IsOnGround { get; set; } = true;

		public bool IsJumping { get; set; }

        public bool IsCenterOnDiagonal { get; set; }

		public float JumpStrength { get; set; } = -14f;

        public KeyboardState PreviousKeyboardState { get; set; }

        public float Friction { get; set; } = 0.9f;

        public float Gravity { get; set; } = 0.5f;

		public SoundEffect MoveSound { get; set; }
		public SoundEffect JumpSound { get; set; }
		private float _soundTimer;
		private const float MoveSoundInterval = 0.25f;
		private const float ShiftMoveSoundInterval = 0.15f;

		private float GetMoveSoundInterval()
		{
			return (KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift))
				? ShiftMoveSoundInterval
				: MoveSoundInterval;
		}

		private void PlayMoveSound(GameTime gameTime)
		{
			_soundTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (_soundTimer >= GetMoveSoundInterval())
			{
				MoveSound?.Play();
				_soundTimer = 0f;
			}
		}

		public override void UpdateAnimation(GameTime gameTime)
		{
			if (!IsSpriteAtlas) return;

			AnimationTimer += gameTime.ElapsedGameTime.TotalSeconds;

			if (AnimationTimer > AnimationSpeed)
			{
				if (HasDirection)
				{
					AnimationFrame++;
					if (AnimationFrame >= TextureInfo.NumOfSpriteCols)
					{
						AnimationFrame = 0;
					}
				}
				else
				{
					if (_isJumping)
					{
						AnimationFrame = 0;
					}
					else
					{
						// For walking animation: use frames 1 and 2 when movings
						if (Math.Abs(_velocity.X) > 0.1f)
						{
							AnimationFrame++;
							if (AnimationFrame < 1 || AnimationFrame > 2)  // Ensure we only use frames 1 and 2
							{
								AnimationFrame = 1;
							}
						}
						else
						{
							AnimationFrame = 0;  // Idle frame
						}
					}
				}
				AnimationTimer -= AnimationSpeed;
			}
		}

        public void HandleInput(KeyboardState keyboardState, GamePadState gamePadState, GameTime gameTime)
        {
            GamePadState = GamePad.GetState(0);
            KeyboardState = Keyboard.GetState();

            // Check for jump before movement utilities to handle the sound
            bool wasJumping = _isJumping;
            bool wasOnGround = _isOnGround;

            // Use MovementUtilities to handle player input and physics
            MovementUtilities.HandlePlayerInput(
                ref _position,
                ref _velocity,
                ref _isOnGround,
                ref _isJumping,
                ref _jumpDiagonalPosY,
                ref _isCenterOnDiagonal,
                ref _soundTimer,
				ref _movementSpeed,
				ref _animationSpeed,
                KeyboardState,
                PreviousKeyboardState,
                GameState.CurrentMap,
                CollisionBounding,
                TextureInfo,
				EntityStatus,
                ref Flipped,
                Friction,
                Gravity,
                JumpStrength,
                PlayMoveSound,
                gameTime);

            // Play jump sound if we just started jumping
            if (!wasJumping && _isJumping && wasOnGround)
            {
                JumpSound?.Play();
            }

            // Update the previous keyboard state
            PreviousKeyboardState = KeyboardState;
        }

        public void LoadSound(ContentManager content)
		{
			MoveSound = content.Load<SoundEffect>("move");
			JumpSound = content.Load<SoundEffect>("Jump");
		}

		public override void Update(GameTime gameTime)
		{
			CollisionBounding ??= TextureInfo.CollisionType;
            if (CollisionBounding is BoundingCircle bc)
            {
                bc.Center = new Vector2(Position.X + (bc.Radius / 2), Position.Y + (bc.Radius / 2));
                CollisionBounding = bc;
            }
            else if (CollisionBounding is BoundingRectangle br)
            {
                br = new BoundingRectangle(Position, br.Width, br.Height);
                CollisionBounding = br;
            }
            HandleInput(KeyboardState, GamePadState, gameTime);
		}
	}
}
