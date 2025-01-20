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

            // Handle X-axis collision first
            Vector2 proposedXPosition = _position + new Vector2(proposedXVelocity, 0);
            bool hasXCollision = CheckCollisionAtPosition(proposedXPosition, GameState.CurrentMap);

            // Apply horizontal movement
            if (!hasXCollision)
            {
                _velocity.X = proposedXVelocity;
                if (Math.Abs(_velocity.X) > 0.1f && !IsJumping)
                {
                    PlayMoveSound(gameTime);
                }
            }
            else
            {
                _velocity.X = 0;  // Only zero out X velocity on X collision
            }

            // Apply gravity and calculate Y movement
            _velocity.Y += Gravity;
            Vector2 proposedYPosition = _position + new Vector2(0, _velocity.Y);
            bool hasYCollision = CheckCollisionAtPosition(proposedYPosition, GameState.CurrentMap);

            // Handle Y movement and ground detection
            if (hasYCollision)
            {
                if (_velocity.Y > 0) // Moving downward
                {
                    IsOnGround = true;
                    if (IsJumping) IsJumping = false;
                }
                _velocity.Y = 0;
            }
            else
            {
                IsOnGround = false;
            }

            // Calculate final position using both axes
            Vector2 newPosition = _position + _velocity;

            // Check map bounds
            Rectangle mapBounds = MapHelper.GetMapWorldBounds();
            newPosition.X = MathHelper.Clamp(newPosition.X,
                (TextureInfo.UnitTextureWidth * TextureInfo.SizeScale) / 2f,
                mapBounds.Width - (TextureInfo.UnitTextureWidth * TextureInfo.SizeScale) / 2f);

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

        private bool CheckCollisionAtPosition(Vector2 position, TiledMap map)
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

                leftTile = (int)(testBounds.Left / MapHelper.TileSize);
                rightTile = (int)Math.Ceiling(testBounds.Right / MapHelper.TileSize);
                topTile = (int)(testBounds.Top / MapHelper.TileSize);
                bottomTile = (int)Math.Ceiling(testBounds.Bottom / MapHelper.TileSize);
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
                if (CheckLayerCollision(layer, leftTile, rightTile, topTile, bottomTile, position))
                    return true;
            }

            // Check collision with group layers
            foreach (Group group in map.Groups.Values)
            {
                foreach (Layer layer in group.Layers.Values)
                {
                    if (CheckLayerCollision(layer, leftTile, rightTile, topTile, bottomTile, position))
                        return true;
                }
            }

            return false;
        }

        private bool CheckLayerCollision(Layer layer, int leftTile, int rightTile, int topTile, int bottomTile, Vector2 position)
        {
            for (int y = topTile; y <= bottomTile; y++)
            {
                for (int x = leftTile; x <= rightTile; x++)
                {
                    int tileId = layer.GetTile(x, y);

                    if (tileId != 0 &&
                        tileId != 21 && tileId != 25 && tileId != 26 && tileId != 31 &&
                        tileId != 53 && tileId != 54 && tileId != 57)
                    {
                        BoundingRectangle tileRect = new(
                            x * MapHelper.TileSize + 5,
                            y * MapHelper.TileSize,
                            (float)(MapHelper.TileSize * 0.8),
                            MapHelper.TileSize
                        );

                        // Create a test collision bounds at the proposed position
                        ICollisionBounding testBounds;
                        if (CollisionBounding is BoundingRectangle br)
                        {
                            testBounds = new BoundingRectangle(position.X, position.Y, br.Width, br.Height);
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
