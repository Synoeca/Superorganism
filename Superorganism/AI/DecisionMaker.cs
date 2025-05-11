using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Superorganism.Collisions;
using Superorganism.Common;
using Superorganism.Core.Managers;
using Superorganism.Core.Timing;
using Superorganism.Entities;
using Superorganism.Enums;
using Superorganism.Tiles;

namespace Superorganism.AI
{
	/// <summary>
	/// Make decisions for the AI entities
	/// </summary>
	public static class DecisionMaker
	{
        /// <summary>
        /// Random number generator used for making stochastic decisions in AI behavior.
        /// Thread-safe instance used across all entity decision-making.
        /// </summary>
		public static readonly Random Rand = new();

        /// <summary>
        /// Stores the last known position of a target entity during chase sequences.
        /// Used for predictive pathing when the target is temporarily out of sight.
        /// </summary>
        //private static Vector2 _lastKnownTargetPosition;

        /// <summary>
        /// The target strategy that entities are transitioning towards.
        /// Used during strategy transition phases to determine the final behavior.
        /// </summary>
        public static Strategy TargetStrategy;

        /// <summary>
        /// The target strategy that entities are transitioning towards.
        /// Used during strategy transition phases to determine the final behavior.
        /// </summary>
        public static List<Entity> Entities { get; set; } = [];

        /// <summary>
        /// Generates a random duration for how long an entity should move in a specific direction.
        /// Used by flying movement strategies to create natural, unpredictable movement patterns.
        /// </summary>
        /// <returns>A double value between 3 and 23 seconds representing the direction change interval.</returns>
        private static double GetNewDirectionInterval()
		{
			return Rand.NextDouble() * 3.0 + Rand.Next(3, 21);
		}

        /// <summary>
        /// Calculates how long the current strategy has been active.
        /// Used to determine when to transition between different AI behaviors.
        /// </summary>
        /// <param name="strategyHistory">List of strategies with their start and last action times.</param>
        /// <param name="gameTime">Current game timing information.</param>
        /// <returns>Duration in seconds that the current strategy has been active.</returns>
        private static double GetStrategyDuration(List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory, GameTime gameTime)
        {
            if (!strategyHistory.Any()) return 0;
            (Strategy Strategy, double StartTime, double LastActionTime) lastEntry = strategyHistory[^1];
            // Use GameTimer instead of gameTime.TotalGameTime.TotalSeconds
            return GameTimer.TotalGameplayTime - lastEntry.LastActionTime;
        }

        /// <summary>
        /// Checks if a proposed position would result in collision with the game world,
        /// excluding diagonal tiles that entities can pass through.
        /// </summary>
        /// <param name="proposedPosition">The position to check for collisions.</param>
        /// <param name="textureInfo">Information about the entity's texture and size.</param>
        /// <returns>True if collision would occur, false if position is valid.</returns>
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

        /// <summary>
        /// Simplified Action method for flying entities that don't need ground collision detection.
        /// Used primarily for airborne entities like flying creatures.
        /// </summary>
        /// <param name="strategy">Current AI strategy being executed.</param>
        /// <param name="strategyHistory">History of strategies for tracking behavior patterns.</param>
        /// <param name="gameTime">Game timing information.</param>
        /// <param name="direction">Current movement direction.</param>
        /// <param name="position">Current position of the entity.</param>
        /// <param name="directionTimer">Timer for tracking direction changes.</param>
        /// <param name="directionInterval">Interval between direction changes.</param>
        /// <param name="velocity">Current movement velocity.</param>
        /// <param name="screenWidth">Width of the game screen.</param>
        /// <param name="groundHeight">Height of the ground level.</param>
        /// <param name="textureInfo">Information about the entity's texture and size.</param>
        /// <param name="entityStatus">Current stats and status of the entity.</param>
        public static void Action(ref Strategy strategy,
            ref List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory, 
            GameTime gameTime, ref Direction direction,ref Vector2 position, ref double directionTimer, 
            ref double directionInterval, ref Vector2 velocity, int screenWidth, int groundHeight,
            TextureInfo textureInfo, EntityStatus entityStatus) {}

        /// <summary>
        /// Comprehensive Action method that executes AI behavior for all entity types.
        /// Handles ground-based movement, collision detection, jumping, and all strategy implementations.
        /// </summary>
        /// <param name="strategy">Current AI strategy being executed.</param>
        /// <param name="strategyHistory">History of strategies for tracking behavior patterns.</param>
        /// <param name="gameTime">Game timing information.</param>
        /// <param name="direction">Current movement direction.</param>
        /// <param name="position">Current position of the entity.</param>
        /// <param name="directionTimer">Timer for tracking direction changes.</param>
        /// <param name="directionInterval">Interval between direction changes.</param>
        /// <param name="collisionBounding">Collision boundary for the entity.</param>
        /// <param name="lastKnownTargetPosition"></param>
        /// <param name="velocity">Current movement velocity.</param>
        /// <param name="screenWidth">Width of the game screen.</param>
        /// <param name="groundHeight">Height of the ground level.</param>
        /// <param name="textureInfo">Information about the entity's texture and size.</param>
        /// <param name="entityStatus">Current stats and status of the entity.</param>
        /// <param name="isOnGround">Whether the entity is currently on the ground.</param>
        /// <param name="isJumping">Whether the entity is currently jumping.</param>
        /// <param name="friction">Ground friction affecting movement.</param>
        /// <param name="isCenterOnDiagonalSlope">Whether the entity is centered on a diagonal slope.</param>
        /// <param name="jumpDiagonalPosY">Y position used for diagonal slope jumping calculations.</param>
        /// <param name="flipped">Whether the entity sprite is horizontally flipped.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported strategy is provided.</exception>
        public static void Action(ref Strategy strategy,
            ref List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory,
            GameTime gameTime, ref Direction direction, ref Vector2 position,
            ref double directionTimer, ref double directionInterval, ref ICollisionBounding collisionBounding,
            ref Vector2 lastKnownTargetPosition,
            ref Vector2 velocity, int screenWidth, int groundHeight, TextureInfo textureInfo, EntityStatus entityStatus,
            ref bool isOnGround, ref bool isJumping, ref float friction, ref bool isCenterOnDiagonalSlope,
            ref float jumpDiagonalPosY, ref bool flipped)
        {
            double currentStrategyDuration = GetStrategyDuration(strategyHistory, gameTime);
            Rectangle mapBounds = MapHelper.GetMapWorldBounds();

            switch (strategy)
            {
                case Strategy.RandomFlyingMovement:
                {
                    // Use GameTimer.GetElapsedGameplayTime to ensure proper timing during active gameplay
                    directionTimer += GameTimer.GetElapsedGameplayTime(gameTime);
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
                    position += velocity * (float)GameTimer.GetElapsedGameplayTime(gameTime);
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

                        // Use GameTimer.GetElapsedGameplayTime
                        directionTimer += GameTimer.GetElapsedGameplayTime(gameTime);
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

                        Vector2 proposedXPosition = position + new Vector2(velocity.X * (float)GameTimer.GetElapsedGameplayTime(gameTime), 0);
                        if (CheckCollisionExcludingDiagonalTiles(proposedXPosition, textureInfo))
                        {
                            velocity.X = -velocity.X; // Bounce off walls
                        }

                        // Update position using elapsed gameplay time
                        Vector2 newPosition = position + velocity * (float)GameTimer.GetElapsedGameplayTime(gameTime);

                        bool hitsDiagonal = false;
                        float slope = 0;

                        // Check ground collision before applying position update
                        float groundY = MapHelper.GetGroundYPosition(
                            GameState.CurrentMap,
                            newPosition.X,
                            position.Y,
                            textureInfo.UnitTextureHeight * textureInfo.SizeScale,
                            collisionBounding,
                            ref hitsDiagonal,
                            ref slope
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
                    MovementUtilities.HandlePatrolStrategy(
                        ref position,
                        ref velocity,
                        ref isOnGround,
                        ref isJumping,
                        ref jumpDiagonalPosY,
                        ref isCenterOnDiagonalSlope,
                        GameState.CurrentMap,
                        collisionBounding,
                        textureInfo,
                        entityStatus,
                        ref flipped,
                        ref strategy,
                        ref strategyHistory,
                        Entities,
                        currentStrategyDuration,
                        ref lastKnownTargetPosition,
                        gameTime);

                    break;
                }

                case Strategy.ChaseEnemy:
                {
                    MovementUtilities.HandleChaseStrategy(
                        ref position,
                        ref velocity,
                        ref isOnGround,
                        ref isJumping,
                        ref jumpDiagonalPosY,
                        ref isCenterOnDiagonalSlope,
                        GameState.CurrentMap,
                        collisionBounding,
                        textureInfo,
                        entityStatus,
                        ref flipped,
                        ref strategy,
                        strategyHistory,
                        Entities,
                        currentStrategyDuration,
                        ref lastKnownTargetPosition,
                        gameTime);

                    break;
                }
                case Strategy.Transition:
                {
                    MovementUtilities.HandleTransitionStrategy(
                        ref position,
                        ref velocity,
                        ref isOnGround,
                        ref isJumping,
                        ref jumpDiagonalPosY,
                        ref isCenterOnDiagonalSlope,
                        GameState.CurrentMap,
                        collisionBounding,
                        textureInfo,
                        entityStatus,
                        ref flipped,
                        ref strategy,
                        ref strategyHistory,
                        ref TargetStrategy,
                        currentStrategyDuration,
                        gameTime);

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

            if (collisionBounding is BoundingCircle bca)
            {
                bca.Center = new Vector2(position.X + (bca.Radius / 2), position.Y + (bca.Radius / 2));
                collisionBounding = bca;
            }
            else if (collisionBounding is BoundingRectangle br)
            {
                br = new BoundingRectangle(position, br.Width, br.Height);
                collisionBounding = br;
            }
        }
    }
}