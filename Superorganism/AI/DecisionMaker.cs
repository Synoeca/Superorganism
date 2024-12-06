using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Superorganism.Collisions;
using Superorganism.Common;
using Superorganism.Entities;
using Superorganism.Enums;

namespace Superorganism.AI
{
	public static class DecisionMaker
	{
		private static readonly Random Rand = new();
        private static Vector2 _lastKnownTargetPosition;
        private static Strategy _targetStrategy;
        private const float TRANSITION_DURATION = 1.0f; // 1 second pause
        public static GameTime GameTime { get; set; }
		public static List<Entity> Entities { get; set; } = [];
		public static Strategy Strategy { get; set; }
		public static int GroundY { get; set; }

		private static double GetNewDirectionInterval()
		{
			return Rand.NextDouble() * 3.0 + 1.0;
		}

        private static void AddStrategyToHistory(
            ref Strategy strategy,
            Strategy newStrategy,
            ref List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory,
            GameTime gameTime)
        {
            if (strategy != newStrategy)
            {
                strategy = newStrategy;
                double currentTime = gameTime.TotalGameTime.TotalSeconds;
                strategyHistory.Add((newStrategy, currentTime, currentTime));
            }
        }

        private static double GetStrategyDuration(List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory, GameTime gameTime)
        {
            if (!strategyHistory.Any()) return 0;
            (Strategy Strategy, double StartTime, double LastActionTime) lastEntry = strategyHistory[^1];
            return gameTime.TotalGameTime.TotalSeconds - lastEntry.LastActionTime;
        }

        private static Vector2? GetLastTargetPosition(List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory, GameTime gameTime)
        {
            const double minimumDuration = 3.0; // Minimum duration in seconds
            double currentDuration = GetStrategyDuration(strategyHistory, gameTime);

            if (strategyHistory.Any() &&
                strategyHistory[^1].Strategy == Strategy.ChaseEnemy &&
                currentDuration < minimumDuration)
            {
                return _lastKnownTargetPosition;
            }

            return null;
        }

        private static void TransitionToStrategy(
            ref Strategy strategy,
            Strategy targetStrategy,
            ref List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory,
            GameTime gameTime)
        {
            AddStrategyToHistory(ref strategy, Strategy.Transition, ref strategyHistory, gameTime);
            _targetStrategy = targetStrategy;
        }

        public static void Action(ref Strategy strategy,
            ref List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory, GameTime gameTime, ref Direction direction,
            ref Vector2 position,
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

                // Set velocity instead of modifying position directly
                switch (direction)
                {
                    case Direction.Up:
                        velocity = new Vector2(0, -1) * (entityStatus.Agility * 100);
                        break;
                    case Direction.Down:
                        velocity = new Vector2(0, 1) * (entityStatus.Agility * 100);
                        break;
                    case Direction.Left:
                        velocity = new Vector2(-1, 0) * (entityStatus.Agility * 100);
                        break;
                    case Direction.Right:
                        velocity = new Vector2(1, 0) * (entityStatus.Agility * 100);
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

        public static void Action(ref Strategy strategy, ref List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory,
        GameTime gameTime, ref Direction direction, ref Vector2 position,
        ref double directionTimer, ref double directionInterval, ref ICollisionBounding collisionBounding,
        ref Vector2 velocity, int screenWidth, int groundHeight, TextureInfo textureInfo, EntityStatus entityStatus)
        {
            float entityGroundY;
            GameTime = gameTime;
            double currentStrategyDuration = GetStrategyDuration(strategyHistory, gameTime);

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

                // Update velocity and ensure direction matches velocity
                switch (direction)
                {
                    case Direction.Up:
                        velocity = new Vector2(0, -1) * (entityStatus.Agility * 100);
                        break;
                    case Direction.Down:
                        velocity = new Vector2(0, 1) * (entityStatus.Agility * 100);
                        break;
                    case Direction.Left:
                        velocity = new Vector2(-1, 0) * (entityStatus.Agility * 100);
                        break;
                    case Direction.Right:
                        velocity = new Vector2(1, 0) * (entityStatus.Agility * 100);
                        break;
                }

                // Update position using velocity and elapsed time
                position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (strategy == Strategy.Random360FlyingMovement)
            {
                if (velocity == Vector2.Zero)
                {
                    double angle = Rand.NextDouble() * Math.PI * 2;
                    velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) *
                               (entityStatus.Agility * 100);
                    directionInterval = GetNewDirectionInterval();
                }

                directionTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (directionTimer > directionInterval)
                {
                    double angle = Rand.NextDouble() * Math.PI * 2;
                    velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) *
                               (entityStatus.Agility * 100);
                    directionTimer -= directionInterval;
                    directionInterval = GetNewDirectionInterval();
                }

                // Set direction based on highest velocity component
                float absVelX = Math.Abs(velocity.X);
                float absVelY = Math.Abs(velocity.Y);

                if (absVelX > absVelY)
                {
                    direction = velocity.X > 0 ? Direction.Right : Direction.Left;
                }
                else
                {
                    direction = velocity.Y > 0 ? Direction.Down : Direction.Up;
                }

                position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (position.X < 0 || position.X > screenWidth - textureInfo.UnitTextureWidth)
                {
                    velocity.X = -velocity.X;
                    position = new Vector2(
                        Math.Clamp(position.X, 0, screenWidth - textureInfo.UnitTextureWidth),
                        position.Y);
                }

                if (position.Y < 0 || position.Y > groundHeight - textureInfo.UnitTextureHeight)
                {
                    velocity.Y = -velocity.Y;
                    position = new Vector2(position.X,
                        Math.Clamp(position.Y, 0, groundHeight - textureInfo.UnitTextureHeight));
                }
            }
            else if (strategy == Strategy.Patrol)
            {
                const float movementSpeed = 1.0f;
                const float gravity = 0.5f;

                // Apply gravity
                velocity.Y += gravity;

                // Initialize movement if needed
                if (velocity.X == 0)
                {
                    velocity.X = movementSpeed;
                }

                // Change direction every 3 seconds based on strategy duration
                if (currentStrategyDuration >= 3.0)
                {
                    velocity.X = -velocity.X; // Reverse direction
                    (Strategy Strategy, double StartTime, double LastActionTime) current = strategyHistory[^1];
                    strategyHistory[^1] = (current.Strategy, current.StartTime, gameTime.TotalGameTime.TotalSeconds);
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
                if (position.X <= 100)
                {
                    velocity.X = Math.Abs(velocity.X);
                    position.X = 100;
                }
                else if (position.X >= 700)
                {
                    velocity.X = -Math.Abs(velocity.X);
                    position.X = 700;
                }

                // Check for transition to chase
                foreach (Entity entity in Entities)
                {
                    if (entity is ControllableEntity { IsControlled: true } controllableEntity)
                    {
                        float distance = Vector2.Distance(position, controllableEntity.Position);
                        if (distance < 100)
                        {
                            TransitionToStrategy(ref strategy, Strategy.ChaseEnemy, ref strategyHistory, gameTime);
                            _lastKnownTargetPosition = controllableEntity.Position;
                            return; // Exit early during transition
                        }
                    }
                }
            }
            else if (strategy == Strategy.ChaseEnemy)
            {
                const float chaseSpeed = 3.0f;
                const float gravity = 0.5f;
                velocity.Y += gravity;
                Vector2? targetPosition = null;
                float closestDistance = float.MaxValue;

                // Find the closest controlled entity
                foreach (Entity entity in Entities)
                {
                    if (entity is ControllableEntity { IsControlled: true } controllableEntity)
                    {
                        float distance = Vector2.Distance(position, controllableEntity.Position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            targetPosition = controllableEntity.Position;
                            _lastKnownTargetPosition = controllableEntity.Position;
                        }
                    }
                }

                // Check if target is lost and duration threshold is met
                if ((!targetPosition.HasValue || closestDistance > 200) && currentStrategyDuration >= 3.0)
                {
                    targetPosition = GetLastTargetPosition(strategyHistory, gameTime);
                    if (!targetPosition.HasValue)
                    {
                        TransitionToStrategy(ref strategy, Strategy.Patrol, ref strategyHistory, gameTime);
                        return; // Exit early during transition
                    }
                }

                // Chase logic - will continue chasing last known position during minimum duration
                Vector2 targetPos = targetPosition ?? _lastKnownTargetPosition;
                Vector2 chaseDirection = Vector2.Normalize(targetPos - position);
                velocity.X = chaseDirection.X * chaseSpeed;

                // Update position
                position += velocity;

                entityGroundY = GroundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale) + 6.0f;
                if (position.Y >= entityGroundY)
                {
                    position.Y = entityGroundY;
                    velocity.Y = 0;
                }
            }

            else if (strategy == Strategy.Transition)
            {
                // Pause horizontal movement during transition
                velocity.X = 0;
                velocity.Y += 0.5f; // Keep gravity

                // After 1 second, change to target strategy
                if (currentStrategyDuration >= TRANSITION_DURATION)
                {
                    AddStrategyToHistory(ref strategy, _targetStrategy, ref strategyHistory, gameTime);
                    if (_targetStrategy == Strategy.Patrol)
                    {
                        velocity.X = 1.0f; // Initialize patrol movement
                    }
                }
            }

            collisionBounding.Center = position + textureInfo.Center;
        }
    }
}