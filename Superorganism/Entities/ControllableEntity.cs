using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Superorganism.Collisions;
using Superorganism.Core.Managers;
using Superorganism.Interfaces;
using Superorganism.Tiles;

namespace Superorganism.Entities
{
    public class ControllableEntity : MovableAnimatedDestroyableEntity, IControllable
	{
		public GamePadState GamePadState { get; set; }

		public KeyboardState KeyboardState { get; set; }

		public bool IsControlled { get; set; }

		public float MovementSpeed { get; set; }

		public bool IsOnGround { get; set; }

		public bool IsJumping { get; set; }

		public float JumpStrength { get; set; } = -14f;

		public float Friction { get; set; }

		public float Gravity { get; set; } = 0.5f;

		//public float GroundLevel { get; set; } = 400f;

		//public float? EntityGroundY { get; set; }

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
					if (IsJumping)
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

        // Modify ControllableEntity.HandleInput
        public void HandleInput(KeyboardState keyboardState, GamePadState gamePadState, GameTime gameTime)
        {
            GamePadState = GamePad.GetState(0);
            KeyboardState = Keyboard.GetState();

            // Update movement speed based on shift key
            if (KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift))
            {
                MovementSpeed = 4.5f;
                AnimationSpeed = 0.1f;
            }
            else
            {
                MovementSpeed = 1.0f;
                AnimationSpeed = 0.15f;
            }

            // Handle jumping
            if (IsOnGround && KeyboardState.IsKeyDown(Keys.Space))
            {
                _velocity.Y = JumpStrength;
                IsOnGround = false;
                IsJumping = true;
                JumpSound?.Play();
            }

            // Handle horizontal movement
            if (KeyboardState.IsKeyDown(Keys.Left) || KeyboardState.IsKeyDown(Keys.A))
            {
                _velocity.X = -MovementSpeed;
                _flipped = true;
                if (!IsJumping) PlayMoveSound(gameTime);
            }
            else if (KeyboardState.IsKeyDown(Keys.Right) || KeyboardState.IsKeyDown(Keys.D))
            {
                _velocity.X = MovementSpeed;
                _flipped = false;
                if (!IsJumping) PlayMoveSound(gameTime);
            }
            else if (IsOnGround)
            {
                _velocity.X *= Friction;
                if (Math.Abs(_velocity.X) < 0.1f)
                {
                    _velocity.X = 0;
                    _soundTimer = 0f;
                }
            }

            // Apply gravity
            _velocity.Y += Gravity;

            // Calculate new position
            Vector2 newPosition = _position + _velocity;

            // Check map bounds
            Rectangle mapBounds = MapHelper.GetMapWorldBounds();
            newPosition.X = MathHelper.Clamp(newPosition.X,
                TextureInfo.UnitTextureWidth / 2f,
                mapBounds.Width - TextureInfo.UnitTextureWidth / 2f);

            // Get ground level at new position
            float groundY = MapHelper.GetGroundYPosition(
                GameState.CurrentMap,
                newPosition.X,
                TextureInfo.UnitTextureWidth * TextureInfo.SizeScale
            );

            // Update position and handle ground collision
            if (newPosition.Y >= groundY - TextureInfo.UnitTextureHeight / 2f)
            {
                newPosition.Y = groundY - TextureInfo.UnitTextureHeight / 2f;
                _velocity.Y = 0;
                IsOnGround = true;
                if (IsJumping) IsJumping = false;
            }

            _position = newPosition;

            // Update collision bounds
            if (CollisionBounding is BoundingRectangle boundingRectangle)
            {
                boundingRectangle.X = _position.X + (boundingRectangle.Width / 2f);
                boundingRectangle.Y = _position.Y + (boundingRectangle.Height / 2f);
                CollisionBounding = boundingRectangle;
            }

            _velocity.X = MathHelper.Clamp(_velocity.X, -MovementSpeed * 2, MovementSpeed * 2);
        }

        public void LoadSound(ContentManager content)
		{
			MoveSound = content.Load<SoundEffect>("move");
			JumpSound = content.Load<SoundEffect>("Jump");
		}

		public override void Update(GameTime gameTime)
		{
			CollisionBounding ??= TextureInfo.CollisionType;
			//EntityGroundY ??= GroundLevel - (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale) + 6.0f;
			HandleInput(KeyboardState, GamePadState, gameTime);
		}
	}
}
