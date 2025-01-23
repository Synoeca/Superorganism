using System;
using System.Collections.Generic;
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

        public bool IsOnGround { get; set; } = true;

		public bool IsJumping { get; set; }

		public float JumpStrength { get; set; } = -14f;

        public float DiagonalPosY { get; set; }

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
                float newPosY = 0;
                // Check if the collision is with a diagonal tile
                if (MapHelper.HandleDiagonalCollision(GameState.CurrentMap, _position, proposedXPosition, CollisionBounding, ref _velocity, ref newPosY))
                {
                    _position.X = proposedXPosition.X;
                    _velocity.X = proposedXVelocity;
                    if (newPosY != 0)
                    {
                        _position.Y = newPosY;
                        DiagonalPosY = newPosY;
                    }

                    if (Math.Abs(_velocity.X) > 0.1f && !IsJumping)
                    {
                        PlayMoveSound(gameTime);
                    }
                }
                else
                {
                    // If it's not a diagonal tile, handle as normal collision
                    _velocity.X = 0;
                    DiagonalPosY = 0;
                }
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
                        if (CollisionBounding is BoundingCircle bc)
                        {
                            bc.Center = new Vector2(_position.X + bc.Radius, _position.Y + bc.Radius);
                        }
                        else if (CollisionBounding is BoundingRectangle br)
                        {
                            br.X = _position.X;
                            br.Y = _position.Y;
                            br.Center = new Vector2(br.X + (br.Width / 2), br.Y + (br.Height / 2));
                        }
                        
                        // Check ground at both bottom corners
                        float leftGroundY = MapHelper.GetGroundYPosition(
                            GameState.CurrentMap,
                            _position.X,
                            _position.Y,
                            TextureInfo.UnitTextureHeight * TextureInfo.SizeScale,
                            CollisionBounding
                        );

                        float rightGroundY = MapHelper.GetGroundYPosition(
                            GameState.CurrentMap,
                            _position.X + (TextureInfo.UnitTextureWidth * TextureInfo.SizeScale),
                            _position.Y,
                            TextureInfo.UnitTextureHeight * TextureInfo.SizeScale,
                            CollisionBounding
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

            if (leftTile < 0 || rightTile < 0) { leftTile = rightTile = 0; }
            if (leftTile >= MapHelper.MapWidth || rightTile >= MapHelper.MapWidth) { leftTile = rightTile = MapHelper.MapWidth - 1; }
            if (topTile < 0 || bottomTile < 0) { topTile = bottomTile = 0; }
            if (topTile >= MapHelper.MapHeight || bottomTile >= MapHelper.MapHeight) { topTile = bottomTile = MapHelper.MapHeight - 1; }

            for (int y = topTile; y <= bottomTile; y++)
            {
                for (int x = leftTile; x <= rightTile; x++)
                {
                    int tileId = layer.GetTile(x, y);


                    if (tileId != 0)
                    {
                        Dictionary<string, string> property = MapHelper.GetTileProperties(tileId);

                        // Skip non-collidable tiles
                        if (property.TryGetValue("isCollidable", out string isCollidable) && isCollidable == "false")
                        {
                            continue;
                        }

                        BoundingRectangle tileRect = new(
                            x * MapHelper.TileSize,
                            y * MapHelper.TileSize,
                            MapHelper.TileSize - 5,
                            MapHelper.TileSize - 5
                        );

                        // Check for diagonal tile
                        if (property.TryGetValue("isDiagonal", out string isDiagonal) && isDiagonal == "true")
                        {
                            // Get slope values
                            if (property.TryGetValue("SlopeLeft", out string slopeLeftStr) &&
                                property.TryGetValue("SlopeRight", out string slopeRightStr) &&
                                int.TryParse(slopeLeftStr, out int slopeLeft) &&
                                int.TryParse(slopeRightStr, out int slopeRight))
                            {
                                if (CollisionBounding is BoundingRectangle br)
                                {
                                    //continue;
                                    //float tileLeft = tileRect.Left;
                                    //float tileRight = tileRect.Right;
                                    //float slope = (slopeRight - slopeLeft) / (float)MapHelper.TileSize;

                                    //// Calculate collision point
                                    //Vector2 collisionPoint = new(br.Center.X, br.Bottom);

                                    //if (collisionPoint.X >= tileLeft && collisionPoint.X <= tileRight)
                                    //{
                                    //    // Calculate the Y position on the slope at this X coordinate
                                    //    float distanceFromLeft = collisionPoint.X - tileLeft;
                                    //    float slopeY = tileRect.Bottom + slopeLeft + (slope * distanceFromLeft);

                                    //    if (collisionPoint.Y >= slopeY - 10)  // 10 pixel tolerance
                                    //    {
                                    //        // Adjust position to place entity on slope
                                    //        float newCenterY = slopeY - br.Height / 2;
                                    //        position.Y = newCenterY - br.Height / 2;  // Convert center Y to top-left position
                                    //        CollisionBounding = new BoundingRectangle(position, br.Width, br.Height);
                                    //        _position.Y = position.Y;
                                    //        IsOnGround = true;
                                    //        return true;
                                    //    }
                                    //}
                                }
                                else if (CollisionBounding is BoundingCircle bc)
                                {
                                    //float tileLeft = tileRect.Left;
                                    //float tileRight = tileRect.Right;
                                    //float slope = (slopeRight - slopeLeft) / (float)MapHelper.TileSize;

                                    //// Calculate collision point (bottom center of circle)
                                    //Vector2 collisionPoint = new(bc.Center.X, bc.Center.Y + bc.Radius);

                                    //if (collisionPoint.X >= tileLeft && collisionPoint.X <= tileRight)
                                    //{
                                    //    // Calculate the Y position on the slope at this X coordinate
                                    //    float distanceFromLeft = collisionPoint.X - tileLeft;
                                    //    float slopeY = tileRect.Bottom + slopeLeft + (slope * distanceFromLeft);

                                    //    if (collisionPoint.Y >= slopeY - 10)  // 10 pixel tolerance
                                    //    {
                                    //        // Adjust position to place entity on slope
                                    //        float newCenterY = slopeY - bc.Radius;
                                    //        position.Y = newCenterY;  // For circle, position is the center
                                    //        CollisionBounding = new BoundingCircle(position, bc.Radius);
                                    //        return true;
                                    //    }
                                    //}
                                }
                                //continue;  // Skip regular collision check for diagonal tiles
                            }
                        }

                        if (x == tilex && y == tiley)
                        {
                            continue;
                        }

                        // Regular collision check for non-diagonal tiles
                        ICollisionBounding testBounds;
                        if (CollisionBounding is BoundingRectangle bra)
                        {
                            testBounds = new BoundingRectangle(position.X, position.Y, bra.Width, bra.Height);
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
