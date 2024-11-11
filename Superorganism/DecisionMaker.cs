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
		public static List<Entity> Entities { get; set; } = [];
		public static Strategy Strategy { get; set; }
		public static int GroundY { get; set; }

		private static double GetNewDirectionInterval()
		{
			return Rand.NextDouble() * 3.0 + 1.0;
		}

		public static void Action(ref Strategy strategy, GameTime gameTime, ref Direction direction, ref Vector2 position,
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

		public static void Action(ref Strategy strategy, GameTime gameTime, ref Direction direction, ref Vector2 position,
			ref double directionTimer, ref double directionInterval, ref ICollisionBounding collisionBounding,
			ref Vector2 velocity, int screenWidth, int groundHeight, TextureInfo textureInfo, EntityStatus entityStatus)
		{
			float entityGroundY;
			GameTime = gameTime;
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
				const float MOVEMENT_SPEED = 1.0f;
				const float DIRECTION_CHANGE_TIME = 3.0f;
				const float GRAVITY = 0.5f; // Match the gravity from ControllableEntity
				

				// Initialize movement if it hasn't started yet
				if (directionTimer == 0)
				{
					velocity.X = MOVEMENT_SPEED;
					directionTimer = gameTime.TotalGameTime.TotalSeconds;
				}

				double currentGameTime = gameTime.TotalGameTime.TotalSeconds;
				double elapsedTime = currentGameTime - directionTimer;

				// Change direction every 3 seconds
				if (elapsedTime >= DIRECTION_CHANGE_TIME)
				{
					velocity.X = -velocity.X; // Reverse direction
					directionTimer = currentGameTime;
				}

				// Apply gravity
				velocity.Y += GRAVITY;

				// Update position
				position += velocity;

				entityGroundY = GroundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale) + 6.0f;

				if (position.Y >= entityGroundY)
				{
					position.Y = entityGroundY;
					velocity.Y = 0;
				}

				Console.WriteLine("("+position.X+","+position.Y+")");

				// Handle screen bounds
				if (position.X <= 100)
				{
					velocity.X = Math.Abs(velocity.X); // Force right movement
					position.X = 100;
				}
				else if (position.X >= 700)
				{
					velocity.X = -Math.Abs(velocity.X); // Force left movement
					position.X = 700;
				}

				foreach (Entity entity in Entities)
				{
					if (entity is ControlableEntity controlableEntity)
					{
						if (controlableEntity.IsControlled)
						{
							float distance = Vector2.Distance(position, controlableEntity.Position);
							if (distance < 100)
							{
								strategy = Strategy.ChaseEnemy;
							}
						}
					}
				}

			}

			else if (strategy == Strategy.ChaseEnemy)
			{
				const float CHASE_SPEED = 3.0f; // Slightly faster than patrol speed
				const float GRAVITY = 0.5f;
				const float MIN_DIRECTION_CHANGE_TIME = 0.8f;

				double currentGameTime = gameTime.TotalGameTime.TotalSeconds;
				double elapsedTime = currentGameTime - directionTimer;

				// Apply gravity
				velocity.Y += GRAVITY;

				Vector2? targetPosition = null;
				float closestDistance = float.MaxValue;

				// Find the closest controlled entity
				foreach (Entity entity in Entities)
				{
					if (entity is ControlableEntity controlableEntity && controlableEntity.IsControlled)
					{
						float distance = Vector2.Distance(position, controlableEntity.Position);
						if (distance < closestDistance)
						{
							closestDistance = distance;
							targetPosition = controlableEntity.Position;
						}
					}
				}

				// Chase logic
				if (targetPosition.HasValue && elapsedTime >= MIN_DIRECTION_CHANGE_TIME)
				{
					Vector2 chaseDirection = Vector2.Normalize(targetPosition.Value - position);
					velocity.X = chaseDirection.X * CHASE_SPEED;
					directionTimer = currentGameTime;
				}

				// Update position
				position += velocity;

				entityGroundY = GroundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale) + 6.0f;

				if (position.Y >= entityGroundY)
				{
					position.Y = entityGroundY;
					velocity.Y = 0;
				}

				// Handle screen bounds
				if (position.X <= 50)
				{
					//velocity.X = Math.Abs(velocity.X); // Force right movement
					//position.X = 50;
				}
				else if (position.X >= 800)
				{
					//velocity.X = -Math.Abs(velocity.X); // Force left movement
					//position.X = 800;
				}

				// Check if we should switch back to patrol
				if (!targetPosition.HasValue || closestDistance > 400) // Adjust this threshold as needed
				{
					strategy = Strategy.Patrol;
					//directionTimer = currentGameTime;
					directionTimer = 0;
				}

				Console.WriteLine($"Chase: ({position.X},{position.Y})");
			}




			collisionBounding.Center = position + textureInfo.Center;
		}
	}
}