using System;
using System.Collections.Generic;
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

        public bool IsOnGround { get; set; } = true;

		public bool IsJumping { get; set; }

        public bool IsCenterOnDiagonal { get; set; }

		public float JumpStrength { get; set; } = -14f;

        public float JumpDiagonalPosY { get; set; } = 0;

        public KeyboardState PreviousKeyboardState { get; set; }

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


            //bool flipped = Flipped;
            //bool isOnGround = IsOnGround;
            //bool isJumping = IsJumping;
            //bool isCenterOnDiagonal = IsCenterOnDiagonal;

            //float animationSpeed = AnimationSpeed;
            //float movementSpeed = MovementSpeed;

            //Vector2 position = Position;
            //ICollisionBounding collisionBounding = CollisionBounding;
            //float jumpDiagonalPosY = JumpDiagonalPosY;


            //GamePhysicsHelper.HandleMovement(keyboardState, gamePadState, gameTime, ref movementSpeed, ref animationSpeed, ref flipped, ref isOnGround,
            //    ref _velocity, Friction, ref _soundTimer, JumpStrength, ref isJumping, MoveSound, JumpSound, MoveSoundInterval, ShiftMoveSoundInterval, 
            //    ref position, ref collisionBounding, Gravity, TextureInfo, ref jumpDiagonalPosY, ref isCenterOnDiagonal);

            //Flipped = flipped;
            //IsOnGround = isOnGround;
            //IsJumping = isJumping;
            //IsCenterOnDiagonal = isCenterOnDiagonal;
            //AnimationSpeed = animationSpeed;
            //MovementSpeed = movementSpeed;

            //JumpDiagonalPosY = jumpDiagonalPosY;
            //CollisionBounding = collisionBounding;
            //Position = position;

            if (PreviousKeyboardState.GetPressedKeys().Length == 0)
            {
                //PreviousKeyboardState.
            }

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

            if (PreviousKeyboardState.IsKeyDown(Keys.F) && KeyboardState.IsKeyUp(Keys.F))
            {
                if (proposedXVelocity > 0)
                {
                    MapModifier.ModifyTileBelowPlayer(GameState.CurrentMap, new Vector2(Position.X + (TextureInfo.UnitTextureWidth * TextureInfo.SizeScale) + 5, Position.Y), false);
                }
                else if (proposedXVelocity < 0)
                {
                    MapModifier.ModifyTileBelowPlayer(GameState.CurrentMap, Position, false);
                }
                else
                {
                    MapModifier.ModifyTileBelowPlayer(GameState.CurrentMap, new Vector2(Position.X + (TextureInfo.UnitTextureWidth * TextureInfo.SizeScale)/2, Position.Y), true);
                }

                //if (Flipped)
                //{
                //    MapModifier.ModifyTileBelowPlayer(GameState.CurrentMap, Position);
                //}
                //else
                //{
                //    MapModifier.ModifyTileBelowPlayer(GameState.CurrentMap, new Vector2(Position.X + TextureInfo.UnitTextureWidth * TextureInfo.SizeScale, Position.Y));
                //}
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
                bool diagonal = false;
                bool isCenterOnDiagonal = false;
                bool hasGroundBelow = CheckCollisionAtPosition(_position, GameState.CurrentMap, CollisionBounding, ref diagonal, ref isCenterOnDiagonal);
                IsCenterOnDiagonal = isCenterOnDiagonal;
                if (!hasGroundBelow || diagonal)
                {
                    IsOnGround = false;
                    if (_velocity.Y >= 0) // Only apply gravity if we're not moving upward
                    {
                        //_velocity.Y += Gravity;
                    }
                }
                _velocity.Y += Gravity;
            }
            else
            {
                // in the middle of a jump, apply gravity
                _velocity.Y += Gravity;
            }

            // Try X movement first
            Vector2 proposedXPosition = _position + new Vector2(proposedXVelocity, 0);
            bool diagonalX = false;
            bool isCenterOnDiagonalTile = false;
            bool hasXCollision = CheckCollisionAtPosition(proposedXPosition, GameState.CurrentMap, CollisionBounding, ref diagonalX, ref isCenterOnDiagonalTile);

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
                bool hasLeftDiagonal = false;
                bool hasRightDiagonal = false;
                BoundingRectangle xTileRec = new();
                // Check if the collision is with a diagonal tile
                if (MapHelper.HandleDiagonalCollision(GameState.CurrentMap, _position, proposedXPosition,
                        CollisionBounding, ref _velocity, ref newPosY, ref xTileRec, ref hasLeftDiagonal, ref hasRightDiagonal))
                {
                    _position.X = proposedXPosition.X;
                    _velocity.X = proposedXVelocity;
                    if (newPosY != 0)
                    {
                        if (!IsJumping)
                        {
                            if (_velocity.Y == 0)
                            {
                                IsOnGround = true;
                            }
                        }
                    }

                    if (Math.Abs(_velocity.X) > 0.1f && !IsJumping)
                    {
                        PlayMoveSound(gameTime);
                    }
                }
                else
                {
                    // If it's not a diagonal tile, handle as normal collision
                    if (!isCenterOnDiagonalTile)
                    {
                        //_velocity.X = 0;
                    }
                    _velocity.X = 0;

                }
            }

            // Then try Y movement
            if (_velocity.Y != 0)
            {
                bool isDiagonal = false;
                bool isCenterOnDiagonal = false;
                Vector2 proposedYPosition = _position + new Vector2(0, _velocity.Y);
                bool hasYCollision = CheckCollisionAtPosition(proposedYPosition, GameState.CurrentMap, CollisionBounding, ref isDiagonal, ref isCenterOnDiagonal);
                if (!hasYCollision && !isDiagonal)
                {
                    _position.Y = proposedYPosition.Y;
                    IsOnGround = false;
                }
                else
                {
                    if (Math.Abs(_velocity.Y) > 0) // Moving downward
                    {
                        if (_velocity.Y < 0 && !isDiagonal) // Moving upward
                        {
                            // Hit ceiling, stop upward movement
                            _velocity.Y = 0;
                            IsJumping = false;
                        }

                        else if (_velocity.Y > 0)
                        {
                            bool leftHitsDiagonal = false;
                            bool rightHitsDiagonal = false;
                            float leftSlope = 0;
                            float rightSlope = 0;
                            // Check ground at both bottom corners
                           float leftGroundY = MapHelper.GetGroundYPosition(
                                GameState.CurrentMap,
                                _position.X,
                                _position.Y,
                                TextureInfo.UnitTextureHeight * TextureInfo.SizeScale,
                                CollisionBounding,
                                ref leftHitsDiagonal,
                                ref leftSlope
                            );

                            float rightGroundY = MapHelper.GetGroundYPosition(
                                GameState.CurrentMap,
                                _position.X + (TextureInfo.UnitTextureWidth * TextureInfo.SizeScale),
                                _position.Y,
                                TextureInfo.UnitTextureHeight * TextureInfo.SizeScale,
                                CollisionBounding,
                                ref rightHitsDiagonal,
                                ref rightSlope
                            );

                            float leftPos = leftGroundY - (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);
                            float rightPos = rightGroundY - (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);

                            if (!isDiagonal)
                            { }

                            float groundY = 0;

                            if (!leftHitsDiagonal && rightHitsDiagonal)
                            {

                            }

                            if (IsCenterOnDiagonal)
                            {
                                if (leftHitsDiagonal && rightHitsDiagonal)
                                {
                                    groundY = Math.Min(leftGroundY, rightGroundY);
                                }
                                else if (leftHitsDiagonal)
                                {
                                    groundY = leftGroundY;  // Use the diagonal Y
                                }
                                else if (rightHitsDiagonal)
                                {
                                    groundY = rightGroundY;  // Use the diagonal Y
                                }

                            }
                            else
                            {
                                groundY = Math.Min(leftGroundY, rightGroundY);
                            }

                            float newGroundY;
                            float bottom = 0;
                            if (CollisionBounding is BoundingRectangle br)
                            {
                                bottom = br.Bottom;
                            }
                            else if (CollisionBounding is BoundingCircle bcc)
                            {
                                bottom = bcc.Center.Y - bcc.Radius;
                            }
                            if (groundY < _position.Y)
                            {
                                newGroundY = Math.Max(leftGroundY, rightGroundY) -
                                             (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);

                                float distanceToNewGround = Math.Abs(_position.Y - newGroundY);
                                if (newGroundY - _position.Y < 5)
                                {
                                    if (newGroundY >= 1254 && newGroundY < 1255.5)
                                    {

                                    }
                                    _position.Y = newGroundY;
                                    IsOnGround = true;
                                    _velocity.Y = 0;
                                    if (IsJumping) IsJumping = false;
                                }
                                else
                                {
                                     _position.Y = proposedYPosition.Y;
                                }


                            }
                            else
                            {
                                newGroundY = groundY - (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);

                                // For landing on mixed diagonal/flat tiles
                                if (leftHitsDiagonal || rightHitsDiagonal)
                                {
                                    if (leftHitsDiagonal && rightHitsDiagonal)
                                    {
                                        newGroundY = Math.Min(leftGroundY, rightGroundY) -
                                                     (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);

                                    }
                                    else if (leftHitsDiagonal)
                                    {
                                        if (leftSlope < 0)  // Downward slope (\)
                                        {
                                            if (leftGroundY > rightGroundY && leftGroundY - rightGroundY < 64)
                                            {
                                                newGroundY = rightGroundY -
                                                             (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);
                                            }
                                            else
                                            {
                                                newGroundY = leftGroundY -
                                                             (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);
                                            }
                                        }
                                        else  // Upward slope (/)
                                        {
                                            if (leftGroundY < rightGroundY && rightGroundY - leftGroundY < 64)
                                            {
                                                if (rightGroundY - leftGroundY < 2)
                                                {
                                                    newGroundY = rightGroundY -
                                                                 (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);
                                                }
                                                else
                                                {
                                                    newGroundY = leftGroundY -
                                                                 (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);
                                                }

                                            }
                                            else
                                            {
                                                newGroundY = leftGroundY -
                                                             (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);
                                            }
                                        }
                                    }
                                    else if (rightHitsDiagonal)
                                    {
                                        if (rightSlope < 0)  // Downward slope (\)
                                        {
                                            if (leftGroundY < rightGroundY && rightGroundY - leftGroundY < 64)
                                            {
                                                newGroundY = rightGroundY -
                                                             (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);
                                            }
                                            else
                                            {
                                                if (leftGroundY < _position.Y)
                                                {
                                                    newGroundY = rightGroundY -
                                                                 (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);
                                                }
                                                else
                                                {
                                                    if (leftGroundY - _position.Y < 2)
                                                    {
                                                        newGroundY = leftGroundY -
                                                                     (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);
                                                    }
                                                    else
                                                    {
                                                        newGroundY = rightGroundY -
                                                                     (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);
                                                    }

                                                }

                                            }
                                        }
                                        else  // Upward slope (/)
                                        {
                                            if (leftGroundY < rightGroundY && (rightGroundY - leftGroundY) < 64)
                                            {
                                                newGroundY = leftGroundY -
                                                             (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);
                                            }
                                            else
                                            {
                                                newGroundY = rightGroundY -
                                                             (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);
                                            }
                                        }
                                    }
                                }

                                // Rest remains the same
                                if (JumpDiagonalPosY == 0 ||
                                    (leftHitsDiagonal || rightHitsDiagonal) ||
                                    newGroundY < JumpDiagonalPosY)
                                {
                                    JumpDiagonalPosY = newGroundY;
                                }

                                if (_position.Y < JumpDiagonalPosY)
                                {
                                    _position.Y = proposedYPosition.Y;
                                    IsOnGround = false;
                                }


                                else
                                {
                                    if (newGroundY < 1255)
                                    {

                                    }

                                    _position.Y = newGroundY;
                                    IsOnGround = true;
                                    if (IsJumping) IsJumping = false;
                                    _velocity.Y = 0;
                                    JumpDiagonalPosY = 0;
                                }
                            }
                        }
                        else
                        {
                            _position.Y = proposedYPosition.Y;
                            IsOnGround = false;
                        }
                    }

                }
            }

            // Check map bounds
            Rectangle mapBounds = MapHelper.GetMapWorldBounds();
            _position.X = MathHelper.Clamp(_position.X,
                (TextureInfo.UnitTextureWidth * TextureInfo.SizeScale) / 2f,
                mapBounds.Width - (TextureInfo.UnitTextureWidth * TextureInfo.SizeScale) / 2f);

            // Clamp velocity
            _velocity.X = MathHelper.Clamp(_velocity.X, -MovementSpeed * 2, MovementSpeed * 2);
            IsCenterOnDiagonal = false;

            PreviousKeyboardState = KeyboardState;
        }

        private bool CheckCollisionAtPosition(Vector2 position, TiledMap map, ICollisionBounding collisionBounding,
            ref bool isDiagonal, ref bool isCenterOnDiagonal)
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
                topTile = (int)(testBounds.Top / MapHelper.TileSize) - 2;
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
                if (CheckLayerCollision(layer, leftTile, rightTile, topTile, bottomTile, position, collisionBounding, ref isDiagonal, ref isCenterOnDiagonal))
                    return true;
            }

            // Check collision with group layers
            foreach (Group group in map.Groups.Values)
            {
                foreach (Layer layer in group.Layers.Values)
                {
                    if (CheckLayerCollision(layer, leftTile, rightTile, topTile, bottomTile, position, collisionBounding, ref isDiagonal, ref isCenterOnDiagonal))
                        return true;
                }
            }

            return false;
        }

        private bool CheckLayerCollision(Layer layer, int leftTile, int rightTile, int topTile, int bottomTile,
            Vector2 position, ICollisionBounding collisionBounding, ref bool isThisDiagonalTile,
            ref bool isCenterOnDiagonal)
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
                    //int tileId = layer.GetTile(x, y);
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

                        bool isDiagonalTile = false;

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
                                    float tileLeft = x * MapHelper.TileSize;
                                    float slope = (slopeRight - slopeLeft) / (float)MapHelper.TileSize;
                                    float distanceFromLeft = CollisionBounding.Center.X - tileLeft;
                                    float tileBottom = (y + 1) * MapHelper.TileSize;

                                    float slopeY;
                                    if (slope > 0)
                                    {
                                        slopeY = (tileBottom - slopeLeft) - (slope * distanceFromLeft);
                                    }
                                    else
                                    {
                                        slopeY = (tileBottom - slopeRight) + (slope * (MapHelper.TileSize - Math.Abs(distanceFromLeft)));
                                    }

                                    if (position.Y < slopeY)
                                    {
                                        isDiagonalTile = true;
                                    }

                                }
                                else if (CollisionBounding is BoundingCircle bc)
                                {
                                    
                                }
                               
                            }
                        }
                        isThisDiagonalTile = isDiagonalTile;
                        if (x == tilex && y == 0 && !isDiagonalTile)
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
                        {
                            if (CollisionBounding.Center.X <= tileRect.Right && CollisionBounding.Center.X >= tileRect.Left)
                            {
                                if (isDiagonalTile)
                                {
                                    isCenterOnDiagonal = true;
                                }

                            }
                            else
                            {
                                isCenterOnDiagonal = false;
                            }
                            return true;
                        }
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
