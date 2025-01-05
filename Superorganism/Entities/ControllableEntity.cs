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

            // Store the original position for collision checking
            Vector2 originalPosition = _position;
            float proposedXVelocity = 0;

            // Calculate proposed horizontal movement
            if (KeyboardState.IsKeyDown(Keys.Left) || KeyboardState.IsKeyDown(Keys.A))
            {
                proposedXVelocity = -MovementSpeed;
                Flipped = true;
            }
            else if (KeyboardState.IsKeyDown(Keys.Right) || KeyboardState.IsKeyDown(Keys.D))
            {
                proposedXVelocity = MovementSpeed;
                Flipped = false;
            }
            else if (IsOnGround)
            {
                proposedXVelocity = _velocity.X * Friction;
                if (Math.Abs(proposedXVelocity) < 0.1f)
                {
                    proposedXVelocity = 0;
                    _soundTimer = 0f;
                }
            }

            // Check horizontal collision before applying movement
            Vector2 proposedPosition = _position + new Vector2(proposedXVelocity, 0);

            // Create a slightly smaller hitbox for better feeling collisions
            Vector2 collisionSize = new Vector2(
                TextureInfo.UnitTextureWidth * TextureInfo.SizeScale * 0.8f,
                TextureInfo.UnitTextureHeight * TextureInfo.SizeScale * 0.9f
            );

            // Get the tile at the proposed position
            int tileX = (int)(proposedPosition.X / MapHelper.TileSize);
            int tileY = (int)(proposedPosition.Y / MapHelper.TileSize);
            bool hasCollision = false;

            // Check each layer for collision, excluding diagonal tiles
            foreach (BasicLayerMTLG layer in GameState.CurrentMap.Layers.Values)
            {
                int tileId = layer.GetTile(tileX, tileY);
                // Skip collision check for diagonal tiles (20, 24, 25, 30, 52, 53, 56)
                if (tileId != 0 &&
                    tileId != 21 && tileId != 25 && tileId != 26 && tileId != 31 &&
                    tileId != 53 && tileId != 54 && tileId != 57)
                {
                    // Check collision with non-diagonal tiles
                    if (MapHelper.CheckEntityMapCollision(GameState.CurrentMap, proposedPosition, collisionSize))
                    {
                        hasCollision = true;
                        break;
                    }
                }
            }

            // Only apply horizontal movement if there's no collision with non-diagonal tiles
            if (!hasCollision)
            {
                _velocity.X = proposedXVelocity;
                if (Math.Abs(_velocity.X) > 0.1f && !IsJumping)
                {
                    PlayMoveSound(gameTime);
                }
            }
            else
            {
                // If there's a collision, stop horizontal movement
                _velocity.X = 0;
            }

            // Apply gravity
            _velocity.Y += Gravity;

            // Calculate new position
            Vector2 newPosition = _position + _velocity;

            // Check map bounds
            Rectangle mapBounds = MapHelper.GetMapWorldBounds();
            newPosition.X = MathHelper.Clamp(newPosition.X,
                (TextureInfo.UnitTextureWidth * TextureInfo.SizeScale) / 2f,
                mapBounds.Width - (TextureInfo.UnitTextureWidth * TextureInfo.SizeScale) / 2f);

            // Get ground level at new position
            float groundY = MapHelper.GetGroundYPosition(
                GameState.CurrentMap,
                newPosition.X,
                _position.Y,
                TextureInfo.UnitTextureHeight * TextureInfo.SizeScale
            );

            // Update position and handle ground collision
            if (newPosition.Y > groundY - (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale))
            {
                newPosition.Y = groundY - (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);
                _velocity.Y = 0;
                IsOnGround = true;
                if (IsJumping) IsJumping = false;
            }

            _position = newPosition;

            // Update collision bounds
            if (CollisionBounding is BoundingRectangle boundingRectangle)
            {
                boundingRectangle.X = _position.X;
                boundingRectangle.Y = _position.Y;
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
			HandleInput(KeyboardState, GamePadState, gameTime);
		}
	}
}
