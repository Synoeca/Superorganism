using Microsoft.Xna.Framework;
using Superorganism.Entities;
using System;
using System.Collections.Generic;
using Superorganism.Enums;
using Superorganism.Collisions;

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
						position += new Vector2(0, -1) * (entityStatus.Agility * 100) * (float)gameTime.ElapsedGameTime.TotalSeconds;
						break;
					case Direction.Down:
						position += new Vector2(0, 1) * (entityStatus.Agility * 100) * (float)gameTime.ElapsedGameTime.TotalSeconds;
						break;
					case Direction.Left:
						position += new Vector2(-1, 0) * (entityStatus.Agility * 100) * (float)gameTime.ElapsedGameTime.TotalSeconds;
						break;
					case Direction.Right:
						position += new Vector2(1, 0) * (entityStatus.Agility * 100) * (float)gameTime.ElapsedGameTime.TotalSeconds;
						break;
				}
			}
			else if (strategy == Strategy.Random360FlyingMovement)
			{
				// Initialize velocity if it's zero (first frame)
				if (velocity == Vector2.Zero)
				{
					double angle = Rand.NextDouble() * Math.PI * 2;
					velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (entityStatus.Agility * 100);
					directionInterval = GetNewDirectionInterval(); // Initialize with random interval
				}

				// 360-degree movement logic
				directionTimer += gameTime.ElapsedGameTime.TotalSeconds;
				if (directionTimer > directionInterval)
				{
					// Assign new random velocity
					double angle = Rand.NextDouble() * Math.PI * 2;
					velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (entityStatus.Agility * 100);
					directionTimer -= directionInterval;
					directionInterval = GetNewDirectionInterval(); // Set new random interval
				}

				// Update position
				position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

				// Handle screen bounds and update direction when bouncing
				if (position.X < 0 || position.X > screenWidth - 32)
				{
					velocity.X = -velocity.X;
					position = new Vector2(Math.Clamp(position.X, 0, screenWidth - 32), position.Y);
				}
				if (position.Y < 0 || position.Y > groundHeight - 32)
				{
					velocity.Y = -velocity.Y;
					position = new Vector2(position.X, Math.Clamp(position.Y, 0, groundHeight - 32));
				}
			}

			// Update collision bounds
			collisionBounding.Center = position + textureInfo.Center;
		}
	}
}