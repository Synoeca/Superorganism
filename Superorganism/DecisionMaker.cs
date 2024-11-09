using Microsoft.Xna.Framework;
using Superorganism.Entities;
using System;
using System.Collections.Generic;
using Superorganism.Enums;
using Superorganism.Collisions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Superorganism
{
	public static class DecisionMaker
	{
		private static readonly Random Rand = new();
		public static GameTime GameTime { get; set; }
		public static List<Entity> Entities { get; set; }
		public static Strategy Strategy { get; set; }

		private static double GetNewDirectionInterval()
		{
			return Rand.NextDouble() * 3.0 + 1.0;
		}

		public static void Action(Strategy strategy, GameTime gameTime, ref Direction direction, ref Vector2 position,
			ref double directionTimer, ref double directionInterval, ref Vector2 velocity, int screenWidth,
			int groundHeight,
			TextureInfo textureInfo, EntityStatus entityStatus)
		{
			if (strategy == Strategy.RandomFlyingMovement)
			{
				// Original 4-direction movement logic
				directionTimer += gameTime.ElapsedGameTime.TotalSeconds;
				if (directionTimer > directionInterval)
				{
					switch (direction)
					{
						case Direction.Up:
							direction = Direction.Down;
							break;
						case Direction.Down:
							direction = Direction.Right;
							break;
						case Direction.Right:
							direction = Direction.Left;
							break;
						case Direction.Left:
							direction = Direction.Up;
							break;
					}

					directionTimer -= directionInterval;
					directionInterval = GetNewDirectionInterval(); // Randomize next interval
				}

				switch (direction)
				{
					case Direction.Up:
						position += new Vector2(0, -1) * (entityStatus.Agility * 100) *
						            (float)gameTime.ElapsedGameTime.TotalSeconds;
						break;
					case Direction.Down:
						position += new Vector2(0, 1) * (entityStatus.Agility * 100) *
						            (float)gameTime.ElapsedGameTime.TotalSeconds;
						break;
					case Direction.Left:
						position += new Vector2(-1, 0) * (entityStatus.Agility * 100) *
						            (float)gameTime.ElapsedGameTime.TotalSeconds;
						break;
					case Direction.Right:
						position += new Vector2(1, 0) * (entityStatus.Agility * 100) *
						            (float)gameTime.ElapsedGameTime.TotalSeconds;
						break;
				}
			}
			else if (strategy == Strategy.Random360FlyingMovement)
			{
				// Initialize velocity if it's zero (first frame)
				if (velocity == Vector2.Zero)
				{
					double angle = Rand.NextDouble() * Math.PI * 2;
					velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) *
					           (entityStatus.Agility * 100);
					directionInterval = GetNewDirectionInterval(); // Initialize with random interval
				}

				// 360-degree movement logic
				directionTimer += gameTime.ElapsedGameTime.TotalSeconds;
				if (directionTimer > directionInterval)
				{
					// Assign new random velocity
					double angle = Rand.NextDouble() * Math.PI * 2;
					velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) *
					           (entityStatus.Agility * 100);
					directionTimer -= directionInterval;
					directionInterval = GetNewDirectionInterval(); // Set new random interval
				}

				// Update position
				position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

				// Handle screen bounds and update direction when bouncing
				if (position.X < 0 || position.X > screenWidth - textureInfo.UnitTextureWidth)
				{
					velocity.X = -velocity.X;
					position = new Vector2(Math.Clamp(position.X, 0, screenWidth - textureInfo.UnitTextureWidth),
						position.Y);
				}

				if (position.Y < 0 || position.Y > groundHeight - textureInfo.UnitTextureHeight)
				{
					velocity.Y = -velocity.Y;
					position = new Vector2(position.X,
						Math.Clamp(position.Y, 0, groundHeight - textureInfo.UnitTextureHeight));
				}
			}
		}

		public static void Action(Strategy strategy, GameTime gameTime, ref Direction direction, ref Vector2 position,
			ref double directionTimer, ref double directionInterval, ref ICollisionBounding collisionBounding,
			ref Vector2 velocity, int screenWidth, int groundHeight, TextureInfo textureInfo, EntityStatus entityStatus)
		{
			if (strategy == Strategy.RandomFlyingMovement)
			{
				// Original 4-direction movement logic
				directionTimer += gameTime.ElapsedGameTime.TotalSeconds;
				if (directionTimer > directionInterval)
				{
					switch (direction)
					{
						case Direction.Up:
							direction = Direction.Down;
							break;
						case Direction.Down:
							direction = Direction.Right;
							break;
						case Direction.Right:
							direction = Direction.Left;
							break;
						case Direction.Left:
							direction = Direction.Up;
							break;
					}

					directionTimer -= directionInterval;
					directionInterval = GetNewDirectionInterval(); // Randomize next interval
				}

				switch (direction)
				{
					case Direction.Up:
						position += new Vector2(0, -1) * (entityStatus.Agility * 100) *
						            (float)gameTime.ElapsedGameTime.TotalSeconds;
						break;
					case Direction.Down:
						position += new Vector2(0, 1) * (entityStatus.Agility * 100) *
						            (float)gameTime.ElapsedGameTime.TotalSeconds;
						break;
					case Direction.Left:
						position += new Vector2(-1, 0) * (entityStatus.Agility * 100) *
						            (float)gameTime.ElapsedGameTime.TotalSeconds;
						break;
					case Direction.Right:
						position += new Vector2(1, 0) * (entityStatus.Agility * 100) *
						            (float)gameTime.ElapsedGameTime.TotalSeconds;
						break;
				}
			}
			else if (strategy == Strategy.Patrol)
			{
				const float ACCELERATION = 0.12f;
				const float MOVEMENT_SPEED = 2.5f;
				const float FRICTION = 0.8f;
				const float GRAVITY = 0.5f;

				// Initialize direction interval and initial movement
				if (directionInterval == 0)
				{
					directionInterval = Rand.NextDouble() * 6.0 + 5.0; // Random interval between 5 and 11 seconds (increased)
					velocity.X = MOVEMENT_SPEED; // Start moving right
				}

				// Add gravity
				velocity.Y += GRAVITY;

				// Handle ground collision using passed in groundHeight
				bool isOnGround = position.Y >= groundHeight;
				if (isOnGround)
				{
					position.Y = groundHeight;
					velocity.Y = 0;
				}

				// Update direction timer
				directionTimer += gameTime.ElapsedGameTime.TotalSeconds;
				if (directionTimer > directionInterval)
				{
					// 5% chance to stop, 95% chance to change direction (increased movement probability)
					if (Rand.NextDouble() < 0.05) // Reduced from 0.1 to 0.05 to increase movement time
					{
						velocity.X = 0; // Stop
					}
					else
					{
						// Randomly choose direction: -1 for left, 1 for right
						velocity.X = (Rand.Next(2) * 2 - 1) * MOVEMENT_SPEED;
					}

					directionTimer = 0;
					directionInterval = Rand.NextDouble() * 1.0 + 2.0; // Longer intervals for movement
				}

				// Apply movement based on current direction
				float targetVelocityX = velocity.X;
				velocity.X = MathHelper.Lerp(velocity.X, targetVelocityX, ACCELERATION);

				// Update position
				position += velocity;

				// Update collision bounds
				collisionBounding.Center = position + new Vector2(16, 16);

				// Apply friction when on ground
				if (isOnGround)
				{
					velocity.X *= FRICTION;
					if (Math.Abs(velocity.X) < 0.1f)
					{
						velocity.X = 0;
					}
				}
			}



			collisionBounding.Center = position + textureInfo.Center;
		}
	}
}