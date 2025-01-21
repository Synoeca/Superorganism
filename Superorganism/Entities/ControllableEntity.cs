using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Superorganism.Collisions;
using Superorganism.Common;
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

		//public float Friction { get; set; }

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

            // Handle jumping
            bool startingJump = IsOnGround && KeyboardState.IsKeyDown(Keys.Space);
            if (startingJump)
            {
                _velocity.Y = JumpStrength;
                IsOnGround = false;
                IsJumping = true;
                JumpSound?.Play();
            }
            else if (!IsJumping) // Only check for falling if we're not in a jump
            {
                // Check if there's ground below us
                Vector2 groundCheckPos = _position + new Vector2(0, 1.0f);
                bool hasGroundBelow = CheckCollisionAtPosition(groundCheckPos, GameState.CurrentMap, CollisionBounding);

                if (!hasGroundBelow)
                {
                    IsOnGround = false;
                    if (_velocity.Y >= 0) // Only apply gravity if we're not moving upward
                    {
                        _velocity.Y += Gravity;
                    }
                }
            }
            else
            {
                // in the middle of a jump, apply gravity
                _velocity.Y += Gravity;
            }

            // Try X movement first
            Vector2 proposedXPosition = _position + new Vector2(proposedXVelocity, 0);
            bool hasXCollision = CheckCollisionAtPosition(proposedXPosition, GameState.CurrentMap, CollisionBounding);

            // Apply X movement if no collision
            if (!hasXCollision)
            {
                _position.X = proposedXPosition.X;
                _velocity.X = proposedXVelocity;
                if (Math.Abs(_velocity.X) > 0.1f && !IsJumping)
                {
                    PlayMoveSound(gameTime);
                }
            }
            else
            {
                _velocity.X = 0;
            }

            // Then try Y movement
            if (_velocity.Y != 0)
            {
                Vector2 proposedYPosition = _position + new Vector2(0, _velocity.Y);
                bool hasYCollision = CheckCollisionAtPosition(proposedYPosition, GameState.CurrentMap, CollisionBounding);
                if (!hasYCollision)
                {
                    _position.Y = proposedYPosition.Y;
                    IsOnGround = false;
                }
                else
                {
                    if (_velocity.Y > 0) // Moving downward
                    {
                        
                        // Check ground at both bottom corners
                        float leftGroundY = MapHelper.GetGroundYPosition(
                            GameState.CurrentMap,
                            _position.X,
                            _position.Y,
                            TextureInfo.UnitTextureHeight * TextureInfo.SizeScale
                        );

                        float rightGroundY = MapHelper.GetGroundYPosition(
                            GameState.CurrentMap,
                            _position.X + (TextureInfo.UnitTextureWidth * TextureInfo.SizeScale),
                            _position.Y,
                            TextureInfo.UnitTextureHeight * TextureInfo.SizeScale
                        );

                        // Use the highest ground position (lowest Y value)


                        float groundY = Math.Min(leftGroundY, rightGroundY);
                        if (groundY < _position.Y)
                        {
                            _position.Y = Math.Max(leftGroundY, rightGroundY) - (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);
                            IsOnGround = true;
                            if (IsJumping) IsJumping = false;
                        }
                        else
                        {
                            _position.Y = groundY - (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);
                            IsOnGround = true;
                            if (IsJumping) IsJumping = false;
                        }


                    }
                    _velocity.Y = 0;
                }
            }

            // Check map bounds
            Rectangle mapBounds = MapHelper.GetMapWorldBounds();
            _position.X = MathHelper.Clamp(_position.X,
                (TextureInfo.UnitTextureWidth * TextureInfo.SizeScale) / 2f,
                mapBounds.Width - (TextureInfo.UnitTextureWidth * TextureInfo.SizeScale) / 2f);

            // Clamp velocity
            _velocity.X = MathHelper.Clamp(_velocity.X, -MovementSpeed * 2, MovementSpeed * 2);
        }

        private bool CheckCollisionAtPosition(Vector2 position, TiledMap map, ICollisionBounding collisionBounding)
        {
            int leftTile = 0;
            int rightTile = 0;
            int topTile = 0;
            int bottomTile = 0;

            // Update collision bounds for test position
            if (CollisionBounding is BoundingRectangle br)
            {
                BoundingRectangle testBounds = new(
                    position.X,
                    position.Y,
                    br.Width,
                    br.Height
                );

                leftTile = (int)(testBounds.Left / MapHelper.TileSize) - 1;
                rightTile = (int)Math.Ceiling(testBounds.Right / MapHelper.TileSize);
                topTile = (int)(testBounds.Top / MapHelper.TileSize) - 1;
                bottomTile = (int)Math.Ceiling(testBounds.Bottom / MapHelper.TileSize) - 1;
            }
            else if (CollisionBounding is BoundingCircle bc)
            {
                Vector2 testCenter = new(position.X, position.Y);
                leftTile = (int)((testCenter.X - bc.Radius) / MapHelper.TileSize);
                rightTile = (int)Math.Ceiling((testCenter.X + bc.Radius) / MapHelper.TileSize);
                topTile = (int)((testCenter.Y - bc.Radius) / MapHelper.TileSize);
                bottomTile = (int)Math.Ceiling((testCenter.Y + bc.Radius) / MapHelper.TileSize);
            }

            // Check collision with map layers
            foreach (Layer layer in map.Layers.Values)
            {
                if (CheckLayerCollision(layer, leftTile, rightTile, topTile, bottomTile, position, collisionBounding))
                    return true;
            }

            // Check collision with group layers
            foreach (Group group in map.Groups.Values)
            {
                foreach (Layer layer in group.Layers.Values)
                {
                    if (CheckLayerCollision(layer, leftTile, rightTile, topTile, bottomTile, position, collisionBounding))
                        return true;
                }
            }

            return false;
        }

        private bool CheckLayerCollision(Layer layer, int leftTile, int rightTile, int topTile, int bottomTile, Vector2 position, ICollisionBounding collisionBounding)
        {
            int tilex = (int)(collisionBounding.Center.X / MapHelper.TileSize);
            int tiley = (int)(collisionBounding.Center.Y / MapHelper.TileSize);
            //leftTile = tilex - 1;
            //rightTile = tilex + 1;
            //topTile = tiley - 1;
            //bottomTile = tiley + 1;

            for (int y = topTile; y <= bottomTile; y++)
            {
                for (int x = leftTile; x <= rightTile; x++)
                {
                    int tileId = layer.GetTile(x, y);
                    //int tiledId
                    if (x == tilex && y == tiley)
                    {
                        continue;
                    }

                    if (x == 71 && y == 19)
                    {

                    }

                    if (tileId != 0 /*&&
                        tileId != 21 && tileId != 25 && tileId != 26 && tileId != 31 &&
                        tileId != 53 && tileId != 54 && tileId != 57*/)
                    {
                        BoundingRectangle tileRect = new(
                            x * MapHelper.TileSize,
                            y * MapHelper.TileSize,
                            MapHelper.TileSize - 1,
                            MapHelper.TileSize - 1
                        );

                        // Create a test collision bounds at the proposed position
                        ICollisionBounding testBounds;
                        if (CollisionBounding is BoundingRectangle br)
                        {
                            testBounds = new BoundingRectangle(position.X, position.Y, br.Width , br.Height);
                            CollisionBounding = testBounds;
                        }
                        else if (CollisionBounding is BoundingCircle bc)
                        {
                            testBounds = new BoundingCircle(new Vector2(position.X, position.Y), bc.Radius);
                            CollisionBounding = testBounds;
                        }
                        else
                        {
                            continue;
                        }

                        if (CollisionBounding.CollidesWith(tileRect))
                            return true;
                    }
                    else
                    {
                        
                    }
                }
            }
            return false;
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
