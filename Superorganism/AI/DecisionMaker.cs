using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Superorganism.Collisions;
using Superorganism.Common;
using Superorganism.Core.Managers;
using Superorganism.Entities;
using Superorganism.Enums;
using Superorganism.Tiles;

namespace Superorganism.AI
{
	public static class DecisionMaker
	{
		private static readonly Random Rand = new();
        private static Vector2 _lastKnownTargetPosition;
        private static Strategy _targetStrategy;
        private const float TransitionDuration = 1.0f; // 1 second pause
        public static GameTime GameTime { get; set; }
		public static List<Entity> Entities { get; set; } = [];
		public static Strategy Strategy { get; set; }
		public static int GroundY { get; set; }
        public static DateTime GameStartTime { get; set; }

		private static double GetNewDirectionInterval()
		{
			return Rand.NextDouble() * 3.0 + Rand.Next(3, 21);
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
                double currentTime = (DateTime.Now - GameStartTime).TotalSeconds;
                strategyHistory.Add((newStrategy, currentTime, currentTime));
            }
        }

        private static double GetStrategyDuration(List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory, GameTime gameTime)
        {
            if (!strategyHistory.Any()) return 0;
            (Strategy Strategy, double StartTime, double LastActionTime) lastEntry = strategyHistory[^1];
            return (DateTime.Now - GameStartTime).TotalSeconds - lastEntry.LastActionTime;
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

        private static bool CheckCollisionExcludingDiagonalTiles(Vector2 proposedPosition, TextureInfo textureInfo)
        {
            // Create a slightly smaller hitbox for better feeling collisions
            Vector2 collisionSize = new Vector2(
                textureInfo.UnitTextureWidth * textureInfo.SizeScale * 0.8f,
                textureInfo.UnitTextureHeight * textureInfo.SizeScale * 0.9f
            );

            // Get the tile at the proposed position
            int tileX = (int)(proposedPosition.X / MapHelper.TileSize);
            int tileY = (int)(proposedPosition.Y / MapHelper.TileSize);

            // Check each layer for collision, excluding diagonal tiles
            foreach (Layer layer in GameState.CurrentMap.Layers.Values)
            {
                int tileId = layer.GetTile(tileX, tileY);
                if (tileId != 0 &&
                    tileId != 21 && tileId != 25 && tileId != 26 && tileId != 31 &&
                    tileId != 53 && tileId != 54 && tileId != 57)
                {
                    // Check collision with non-diagonal tiles
                    if (MapHelper.CheckEntityMapCollision(GameState.CurrentMap, proposedPosition, collisionSize))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void Action(ref Strategy strategy,
            ref List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory, 
            GameTime gameTime, ref Direction direction,ref Vector2 position, ref double directionTimer, 
            ref double directionInterval, ref Vector2 velocity, int screenWidth, int groundHeight,
            TextureInfo textureInfo, EntityStatus entityStatus) {}

        public static void Action(ref Strategy strategy, ref List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory,
        GameTime gameTime, ref Direction direction, ref Vector2 position,
        ref double directionTimer, ref double directionInterval, ref ICollisionBounding collisionBounding,
        ref Vector2 velocity, int screenWidth, int groundHeight, TextureInfo textureInfo, EntityStatus entityStatus)
        {
            GameTime = gameTime;
            double currentStrategyDuration = GetStrategyDuration(strategyHistory, gameTime);
            Rectangle mapBounds = MapHelper.GetMapWorldBounds();

            switch (strategy)
            {
                case Strategy.RandomFlyingMovement:
                {
                    directionTimer += gameTime.ElapsedGameTime.TotalSeconds;
                    if (directionTimer > directionInterval)
                    {
                        direction = direction switch
                        {
                            Direction.Up => Direction.Down,
                            Direction.Down => Direction.Right,
                            Direction.Right => Direction.Left,
                            Direction.Left => Direction.Up,
                            _ => direction
                        };
                        directionTimer -= directionInterval;
                        directionInterval = GetNewDirectionInterval(); // Randomize next interval
                    }

                    // Update velocity and ensure direction matches velocity
                    velocity = direction switch
                    {
                        Direction.Up => new Vector2(0, -1) * (entityStatus.Agility * 100),
                        Direction.Down => new Vector2(0, 1) * (entityStatus.Agility * 100),
                        Direction.Left => new Vector2(-1, 0) * (entityStatus.Agility * 100),
                        Direction.Right => new Vector2(1, 0) * (entityStatus.Agility * 100),
                        _ => velocity
                    };

                    // Update position using velocity and elapsed time
                    position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    break;
                }
                case Strategy.Random360FlyingMovement:
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

                        Vector2 proposedXPosition = position + new Vector2(velocity.X * (float)gameTime.ElapsedGameTime.TotalSeconds, 0);
                        if (CheckCollisionExcludingDiagonalTiles(proposedXPosition, textureInfo))
                        {
                            velocity.X = -velocity.X; // Bounce off walls
                        }

                        // Update position
                        Vector2 newPosition = position + velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        // Check ground collision before applying position update
                        float groundY = MapHelper.GetGroundYPosition(
                            GameState.CurrentMap,
                            newPosition.X,
                            position.Y,
                            textureInfo.UnitTextureHeight * textureInfo.SizeScale
                        );

                        if (newPosition.Y > groundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale))
                        {
                            velocity.Y = -velocity.Y;  // Just invert Y velocity on ground collision
                        }

                        // Update bounds checking to use map coordinates
                        if (newPosition.X < 0 || newPosition.X > mapBounds.Width)
                        {
                            velocity.X = -velocity.X;
                            newPosition.X = MathHelper.Clamp(newPosition.X, 0, mapBounds.Width);
                        }
                        if (newPosition.Y < 0)
                        {
                            velocity.Y = -velocity.Y;
                            newPosition.Y = 0;
                        }

                        position = newPosition;
                        break;
                    }
                case Strategy.Patrol:
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
                        if (currentStrategyDuration >= Rand.Next(Rand.Next(3, 21), 21))
                        {
                            velocity.X = -velocity.X; // Reverse direction
                            (Strategy Strategy, double StartTime, double LastActionTime) current = strategyHistory[^1];
                            strategyHistory[^1] = (current.Strategy, current.StartTime, (DateTime.Now - GameStartTime).TotalSeconds);
                        }

                        Vector2 proposedXPosition = position + new Vector2(velocity.X, 0);
                        if (CheckCollisionExcludingDiagonalTiles(proposedXPosition, textureInfo))
                        {
                            velocity.X = -velocity.X; // Reverse direction when hitting a wall
                        }

                        // Update position
                        Vector2 newPosition = position + velocity;

                        // Check map bounds
                        newPosition.X = MathHelper.Clamp(newPosition.X,
                            (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f,
                            mapBounds.Width - (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f);

                        // Get ground level at new position
                        float groundY = MapHelper.GetGroundYPosition(
                            GameState.CurrentMap,
                            newPosition.X,
                            position.Y,
                            textureInfo.UnitTextureHeight * textureInfo.SizeScale
                        );

                        if (newPosition.Y > groundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale))
                        {
                            newPosition.Y = groundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                            velocity.Y = 0;
                        }

                        position = newPosition;

                        // Check for transition to chase
                        foreach (Entity entity in Entities)
                        {
                            switch (entity)
                            {
                                case ControllableEntity { IsControlled: true } controllableEntity:
                                {
                                    float distance = Vector2.Distance(position, controllableEntity.Position);
                                    if (distance < 100)
                                    {
                                        TransitionToStrategy(ref strategy, Strategy.ChaseEnemy, ref strategyHistory, gameTime);
                                        _lastKnownTargetPosition = controllableEntity.Position;
                                        return; // Exit early during transition
                                    }

                                    break;
                                }
                            }
                        }

                        break;
                    }

                case Strategy.ChaseEnemy:
                {
                    const float chaseSpeed = 3.0f;
                    const float gravity = 0.5f;
                    velocity.Y += gravity;
                    Vector2? targetPosition = null;
                    float closestDistance = float.MaxValue;

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

                    if ((!targetPosition.HasValue || closestDistance > 300) && currentStrategyDuration >= Rand.Next(Rand.Next(3, 21), 21))
                    {
                        targetPosition = GetLastTargetPosition(strategyHistory, gameTime);
                        if (!targetPosition.HasValue)
                        {
                            TransitionToStrategy(ref strategy, Strategy.Patrol, ref strategyHistory, gameTime);
                            return;
                        }
                    }

                    Vector2 targetPos = targetPosition ?? _lastKnownTargetPosition;
                    Vector2 chaseDirection = Vector2.Normalize(targetPos - position);
                    velocity.X = chaseDirection.X * chaseSpeed;

                    Vector2 proposedXPosition = position + new Vector2(velocity.X, 0);
                    if (CheckCollisionExcludingDiagonalTiles(proposedXPosition, textureInfo))
                    {
                        velocity.X = 0; // Stop at walls when chasing
                    }

                        // Calculate new position
                        Vector2 newPosition = position + velocity;

                    // Check map bounds
                    newPosition.X = MathHelper.Clamp(newPosition.X,
                        (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f,
                        mapBounds.Width - (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f);

                    // Get ground level at new position
                    float groundY = MapHelper.GetGroundYPosition(
                        GameState.CurrentMap,
                        newPosition.X,
                        position.Y,
                        textureInfo.UnitTextureHeight * textureInfo.SizeScale
                    );

                    // Handle ground collision
                    if (newPosition.Y > groundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale))
                    {
                        newPosition.Y = groundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                        velocity.Y = 0;
                    }

                    position = newPosition;
                    break;
                }
                case Strategy.Transition:
                {
                    velocity.X = 0;
                    velocity.Y += 0.5f;
                    Vector2 proposedXPosition = position + new Vector2(velocity.X, 0);
                    if (CheckCollisionExcludingDiagonalTiles(proposedXPosition, textureInfo))
                    {
                        velocity.X = 0;
                    }
                    Vector2 newPosition = position + velocity;

                    // Check map bounds
                    newPosition.X = MathHelper.Clamp(newPosition.X,
                        (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f,
                        mapBounds.Width - (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f);

                    // Get ground level at new position
                    float groundY = MapHelper.GetGroundYPosition(
                        GameState.CurrentMap,
                        newPosition.X,
                        position.Y,
                        textureInfo.UnitTextureHeight * textureInfo.SizeScale
                    );

                    // Handle ground collision
                    if (newPosition.Y > groundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale))
                    {
                        newPosition.Y = groundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                        velocity.Y = 0;
                    }

                    position = newPosition;

                    if (currentStrategyDuration >= TransitionDuration)
                    {
                        AddStrategyToHistory(ref strategy, _targetStrategy, ref strategyHistory, gameTime);
                        if (_targetStrategy == Strategy.Patrol)
                        {
                            velocity.X = 1.0f;
                        }
                    }
                    break;
                }
                case Strategy.Idle:
                    break;
                case Strategy.AvoidEnemy:
                    break;
                case Strategy.ChargeEnemy:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
            }

            if (collisionBounding is BoundingCircle bc)
            {
                bc.Center = new Vector2(position.X + (bc.Radius / 2), position.Y + (bc.Radius / 2));
                collisionBounding = bc;
            }
            else if (collisionBounding is BoundingRectangle br)
            {
                br = new BoundingRectangle(position, br.Width, br.Height);
                collisionBounding = br;
            }
        }
    }
}