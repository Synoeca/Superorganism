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
        public static TimeSpan GameProgressTime { get; set; }
		public static List<Entity> Entities { get; set; } = [];
		public static Strategy Strategy { get; set; }
		public static int GroundY { get; set; }
        public static DateTime GameStartTime { get; set; }
        public static GameTime GameTime { get; set; }

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
                //double currentTime = (DateTime.Now - GameStartTime).TotalSeconds;
                double currentTime = gameTime.TotalGameTime.TotalSeconds;
                strategyHistory.Add((newStrategy, currentTime, currentTime));
            }
        }

        private static double GetStrategyDuration(List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory, GameTime gameTime)
        {
            if (!strategyHistory.Any()) return 0;
            (Strategy Strategy, double StartTime, double LastActionTime) lastEntry = strategyHistory[^1];
            //return (DateTime.Now - GameStartTime).TotalSeconds - lastEntry.LastActionTime;
            //return (gameTime.TotalGameTime - GameStartTime.TimeOfDay).TotalSeconds - lastEntry.LastActionTime;
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
        ref Vector2 velocity, int screenWidth, int groundHeight, TextureInfo textureInfo, EntityStatus entityStatus, ref bool isOnGround, ref bool isJumping, ref float friction)
        {
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
                            textureInfo.UnitTextureHeight * textureInfo.SizeScale,
                            collisionBounding
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
                        const float movementSpeed = 0.7f;
                        const float gravity = 0.1f;

                        float proposedXVelocity = velocity.X;

                        // Initialize movement if needed
                        if (velocity.X == 0)
                        {
                            proposedXVelocity = movementSpeed;
                        }

                        // Change direction every 3 seconds based on strategy duration
                        if (currentStrategyDuration >= Rand.Next(Rand.Next(3, 21), 21))
                        {
                            proposedXVelocity = -proposedXVelocity; // Reverse direction
                            (Strategy Strategy, double StartTime, double LastActionTime) current = strategyHistory[^1];
                            //strategyHistory[^1] = (current.Strategy, current.StartTime, (DateTime.Now - GameStartTime).TotalSeconds);
                            strategyHistory[^1] = (current.Strategy, current.StartTime, gameTime.TotalGameTime.TotalSeconds);
                        }

                        if (isOnGround)
                        {
                            //proposedXVelocity = velocity.X * friction;
                            if (Math.Abs(proposedXVelocity) < 0.1f)
                            {
                                proposedXVelocity = 0;
                            }
                        }

                        // Handle jumping
                        bool startingJump = isOnGround && false;
                        if (startingJump)
                        {
                            velocity.Y = -3f;
                            isOnGround = false;
                            isJumping = true;
                        }
                        else if (!isJumping) // Only check for falling if we're not in a jump
                        {
                            // Check if there's ground below us
                            //Vector2 groundCheckPos = _position + new Vector2(0, 1.0f);
                            Vector2 groundCheckPos = position + new Vector2(0, 1.0f);
                            bool diagonal = false;
                            bool isCenterOnDiagonal = false;
                            bool hasGroundBelow = CheckCollisionAtPosition(groundCheckPos, GameState.CurrentMap, collisionBounding, ref diagonal, ref isCenterOnDiagonal);

                            if (!hasGroundBelow || diagonal)
                            {
                                isOnGround = false;
                                if (velocity.Y >= 0) // Only apply gravity if we're not moving upward
                                {
                                    velocity.Y += gravity;
                                }
                            }
                        }
                        else
                        {
                            // in the middle of a jump, apply gravity
                            velocity.Y += gravity;
                        }


                        // Try X movement first
                        Vector2 proposedXPosition = position + new Vector2(proposedXVelocity, 0);
                        bool diagonalX = false;
                        bool isCenterOnDiagonalTile = false;
                        bool hasXCollision = CheckCollisionAtPosition(proposedXPosition, GameState.CurrentMap, collisionBounding, ref diagonalX, ref isCenterOnDiagonalTile);

                        // Apply X movement if no collision
                        if (!hasXCollision)
                        {
                            position.X = proposedXPosition.X;
                            velocity.X = proposedXVelocity;
                            if (Math.Abs(velocity.X) > 0.1f && !isJumping)
                            {
                                //PlayMoveSound(gameTime);
                            }
                        }
                        else
                        {
                            float newPosY = 0;
                            BoundingRectangle xTileRec = new();
                            // Check if the collision is with a diagonal tile
                            if (MapHelper.HandleDiagonalCollision(GameState.CurrentMap, position, proposedXPosition, collisionBounding, ref velocity, ref newPosY, ref xTileRec))
                            {
                                position.X = proposedXPosition.X;
                                velocity.X = proposedXVelocity;
                                if (newPosY != 0)
                                {
                                    if (!isJumping)
                                    {
                                        if (velocity.Y == 0)
                                        {
                                            position.Y = newPosY;
                                            isOnGround = true;
                                        }
                                        //_position.Y = newPosY;
                                        //IsOnGround = true;
                                    }
                                    //DiagonalPosY = newPosY;
                                }

                                if (Math.Abs(velocity.X) > 0.1f && !isJumping)
                                {
                                    //PlayMoveSound(gameTime);
                                }
                            }
                            else
                            {
                                // If it's not a diagonal tile, handle as normal collision
                                velocity.X = 0;
                            }
                        }

                        // Then try Y movement
                        if (velocity.Y != 0)
                        {
                            bool isDiagonal = false;
                            bool isCenterOnDiagonal = false;
                            Vector2 proposedYPosition = position + new Vector2(0, velocity.Y);
                            bool hasYCollision = CheckCollisionAtPosition(proposedYPosition, GameState.CurrentMap, collisionBounding, ref isDiagonal, ref isCenterOnDiagonal);
                            if (!hasYCollision && !isDiagonal)
                            {
                                position.Y = proposedYPosition.Y;
                                isOnGround = false;
                            }
                            else
                            {
                                if (Math.Abs(velocity.Y) > 0) // Moving downward
                                {
                                    if (collisionBounding is BoundingCircle bc)
                                    {
                                        bc.Center = new Vector2(position.X + bc.Radius, position.Y + bc.Radius);
                                    }
                                    else if (collisionBounding is BoundingRectangle br)
                                    {
                                        br.X = position.X;
                                        if (Math.Abs(br.Y - position.Y) < 2)
                                        {
                                            br.Y = position.Y;
                                        }

                                        br.Center = new Vector2(br.X + (br.Width / 2), br.Y + (br.Height / 2));
                                    }

                                    if (velocity.Y > 0)
                                    {
                                        // Check ground at both bottom corners
                                        float leftGroundY = MapHelper.GetGroundYPosition(
                                            GameState.CurrentMap,
                                            position.X,
                                            position.Y,
                                            textureInfo.UnitTextureHeight * textureInfo.SizeScale,
                                            collisionBounding
                                        );

                                        float rightGroundY = MapHelper.GetGroundYPosition(
                                            GameState.CurrentMap,
                                            position.X + (textureInfo.UnitTextureWidth * textureInfo.SizeScale),
                                            position.Y,
                                            textureInfo.UnitTextureHeight * textureInfo.SizeScale,
                                            collisionBounding
                                        );

                                        float leftPos = leftGroundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                        float rightPos = rightGroundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale);

                                        if (!isDiagonal)
                                        { }

                                        float groundY = Math.Min(leftGroundY, rightGroundY);
                                        float newGroundY;
                                        if (groundY < position.Y)
                                        {
                                            newGroundY = Math.Max(leftGroundY, rightGroundY) -
                                                         (textureInfo.UnitTextureHeight * textureInfo.SizeScale);

                                            if (Math.Abs(position.Y - newGroundY) < 15)
                                            {
                                                position.Y = newGroundY;
                                                isOnGround = true;
                                                if (isJumping) isJumping = false;
                                            }
                                            else
                                            {
                                                position.Y = proposedYPosition.Y;
                                            }


                                        }
                                        else
                                        {
                                            newGroundY = groundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                            //if (JumpDiagonalPosY == 0)
                                            //{
                                            //    JumpDiagonalPosY = newGroundY;
                                            //}

                                            if (position.Y < newGroundY)
                                            {
                                                position.Y = proposedYPosition.Y;
                                                isOnGround = false;
                                            }
                                            else
                                            {
                                                position.Y = newGroundY;
                                                isOnGround = true;
                                                if (isJumping) isJumping = false;
                                                velocity.Y = 0;
                                                //JumpDiagonalPosY = 0;
                                            }

                                        }

                                    }
                                    else
                                    {
                                        position.Y = proposedYPosition.Y;
                                        isOnGround = false;
                                    }
                                }

                            }

                            //DiagonalPosY = 0;
                        }

                        // Check map bounds
                        mapBounds = MapHelper.GetMapWorldBounds();
                        position.X = MathHelper.Clamp(position.X,
                            (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f,
                            mapBounds.Width - (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f);

                        // Clamp velocity
                        velocity.X = MathHelper.Clamp(velocity.X, -movementSpeed * 2, movementSpeed * 2);

                        Vector2 newPosition = new(position.X + velocity.X, position.Y + velocity.Y);
                        position = newPosition;

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

                        // Find target
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

                        // Check if we should switch to patrol
                        if ((!targetPosition.HasValue || closestDistance > 300) && currentStrategyDuration >= Rand.Next(Rand.Next(3, 21), 21))
                        {
                            targetPosition = GetLastTargetPosition(strategyHistory, gameTime);
                            if (!targetPosition.HasValue)
                            {
                                TransitionToStrategy(ref strategy, Strategy.Patrol, ref strategyHistory, gameTime);
                                return;
                            }
                        }

                        // Calculate chase direction and velocity
                        Vector2 targetPos = targetPosition ?? _lastKnownTargetPosition;
                        Vector2 chaseDirection = Vector2.Normalize(targetPos - position);
                        float proposedXVelocity = chaseDirection.X * chaseSpeed;

                        // Initialize movement if needed
                        if (velocity.X == 0)
                        {
                            proposedXVelocity = chaseSpeed;
                        }

                        // Change direction every 3 seconds based on strategy duration
                        if (currentStrategyDuration >= Rand.Next(Rand.Next(3, 21), 21))
                        {
                            proposedXVelocity = -proposedXVelocity; // Reverse direction
                            (Strategy Strategy, double StartTime, double LastActionTime) current = strategyHistory[^1];
                            //strategyHistory[^1] = (current.Strategy, current.StartTime, (DateTime.Now - GameStartTime).TotalSeconds);
                            strategyHistory[^1] = (current.Strategy, current.StartTime, gameTime.TotalGameTime.TotalSeconds);
                        }

                        if (isOnGround)
                        {
                            //proposedXVelocity = velocity.X * friction;
                            if (Math.Abs(proposedXVelocity) < 0.1f)
                            {
                                proposedXVelocity = 0;
                            }
                        }

                        // Handle jumping
                        bool startingJump = isOnGround && false;
                        if (startingJump)
                        {
                            velocity.Y = -3f;
                            isOnGround = false;
                            isJumping = true;
                        }
                        else if (!isJumping) // Only check for falling if we're not in a jump
                        {
                            // Check if there's ground below us
                            //Vector2 groundCheckPos = _position + new Vector2(0, 1.0f);
                            Vector2 groundCheckPos = position + new Vector2(0, 1.0f);
                            bool diagonal = false;
                            bool isCenterOnDiagonal = false;
                            bool hasGroundBelow = CheckCollisionAtPosition(groundCheckPos, GameState.CurrentMap, collisionBounding, ref diagonal, ref isCenterOnDiagonal);

                            if (!hasGroundBelow || diagonal)
                            {
                                isOnGround = false;
                                if (velocity.Y >= 0) // Only apply gravity if we're not moving upward
                                {
                                    velocity.Y += gravity;
                                }
                            }
                        }
                        else
                        {
                            // in the middle of a jump, apply gravity
                            velocity.Y += gravity;
                        }


                        // Try X movement first
                        Vector2 proposedXPosition = position + new Vector2(proposedXVelocity, 0);
                        bool diagonalX = false;
                        bool isCenterOnDiagonalTile = false;
                        bool hasXCollision = CheckCollisionAtPosition(proposedXPosition, GameState.CurrentMap, collisionBounding, ref diagonalX, ref isCenterOnDiagonalTile);

                        // Apply X movement if no collision
                        if (!hasXCollision)
                        {
                            position.X = proposedXPosition.X;
                            velocity.X = proposedXVelocity;
                            if (Math.Abs(velocity.X) > 0.1f && !isJumping)
                            {
                                //PlayMoveSound(gameTime);
                            }
                        }
                        else
                        {
                            float newPosY = 0;
                            BoundingRectangle xTileRec = new();
                            // Check if the collision is with a diagonal tile
                            if (MapHelper.HandleDiagonalCollision(GameState.CurrentMap, position, proposedXPosition, collisionBounding, ref velocity, ref newPosY, ref xTileRec))
                            {
                                position.X = proposedXPosition.X;
                                velocity.X = proposedXVelocity;
                                if (newPosY != 0)
                                {
                                    if (!isJumping)
                                    {
                                        if (velocity.Y == 0)
                                        {
                                            position.Y = newPosY;
                                            isOnGround = true;
                                        }
                                        //_position.Y = newPosY;
                                        //IsOnGround = true;
                                    }
                                    //DiagonalPosY = newPosY;
                                }

                                if (Math.Abs(velocity.X) > 0.1f && !isJumping)
                                {
                                    //PlayMoveSound(gameTime);
                                }
                            }
                            else
                            {
                                // If it's not a diagonal tile, handle as normal collision
                                velocity.X = 0;
                            }
                        }

                        // Then try Y movement
                        if (velocity.Y != 0)
                        {
                            bool isDiagonal = false;
                            bool isCenterOnDiagonal = false;
                            Vector2 proposedYPosition = position + new Vector2(0, velocity.Y);
                            bool hasYCollision = CheckCollisionAtPosition(proposedYPosition, GameState.CurrentMap, collisionBounding, ref isDiagonal, ref isCenterOnDiagonal);
                            if (!hasYCollision && !isDiagonal)
                            {
                                position.Y = proposedYPosition.Y;
                                isOnGround = false;
                            }
                            else
                            {
                                if (Math.Abs(velocity.Y) > 0) // Moving downward
                                {
                                    if (collisionBounding is BoundingCircle bc)
                                    {
                                        bc.Center = new Vector2(position.X + bc.Radius, position.Y + bc.Radius);
                                    }
                                    else if (collisionBounding is BoundingRectangle br)
                                    {
                                        br.X = position.X;
                                        if (Math.Abs(br.Y - position.Y) < 2)
                                        {
                                            br.Y = position.Y;
                                        }

                                        br.Center = new Vector2(br.X + (br.Width / 2), br.Y + (br.Height / 2));
                                    }

                                    if (velocity.Y > 0)
                                    {
                                        // Check ground at both bottom corners
                                        float leftGroundY = MapHelper.GetGroundYPosition(
                                            GameState.CurrentMap,
                                            position.X,
                                            position.Y,
                                            textureInfo.UnitTextureHeight * textureInfo.SizeScale,
                                            collisionBounding
                                        );

                                        float rightGroundY = MapHelper.GetGroundYPosition(
                                            GameState.CurrentMap,
                                            position.X + (textureInfo.UnitTextureWidth * textureInfo.SizeScale),
                                            position.Y,
                                            textureInfo.UnitTextureHeight * textureInfo.SizeScale,
                                            collisionBounding
                                        );

                                        float leftPos = leftGroundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                        float rightPos = rightGroundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale);

                                        if (!isDiagonal)
                                        { }

                                        float groundY = Math.Min(leftGroundY, rightGroundY);
                                        float newGroundY;
                                        if (groundY < position.Y)
                                        {
                                            newGroundY = Math.Max(leftGroundY, rightGroundY) -
                                                         (textureInfo.UnitTextureHeight * textureInfo.SizeScale);

                                            if (Math.Abs(position.Y - newGroundY) < 15)
                                            {
                                                position.Y = newGroundY;
                                                isOnGround = true;
                                                if (isJumping) isJumping = false;
                                            }
                                            else
                                            {
                                                position.Y = proposedYPosition.Y;
                                            }


                                        }
                                        else
                                        {
                                            newGroundY = groundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                            //if (JumpDiagonalPosY == 0)
                                            //{
                                            //    JumpDiagonalPosY = newGroundY;
                                            //}

                                            if (position.Y < newGroundY)
                                            {
                                                position.Y = proposedYPosition.Y;
                                                isOnGround = false;
                                            }
                                            else
                                            {
                                                position.Y = newGroundY;
                                                isOnGround = true;
                                                if (isJumping) isJumping = false;
                                                velocity.Y = 0;
                                                //JumpDiagonalPosY = 0;
                                            }

                                        }

                                    }
                                    else
                                    {
                                        position.Y = proposedYPosition.Y;
                                        isOnGround = false;
                                    }
                                }

                            }

                            //DiagonalPosY = 0;
                        }

                        // Check map bounds
                        mapBounds = MapHelper.GetMapWorldBounds();
                        position.X = MathHelper.Clamp(position.X,
                            (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f,
                            mapBounds.Width - (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f);

                        // Clamp velocity
                        velocity.X = MathHelper.Clamp(velocity.X, -chaseSpeed * 2, chaseSpeed * 2);

                        //Vector2 newPosition = new(position.X + velocity.X, position.Y + velocity.Y);
                        //position = newPosition;

                        break;
                    }
                case Strategy.Transition:
                    {
                        const float gravity = 0.5f;
                        float proposedXVelocity = 0; // Stay still during transition

                        if (isOnGround)
                        {
                            //proposedXVelocity = velocity.X * friction;
                            if (Math.Abs(proposedXVelocity) < 0.1f)
                            {
                                proposedXVelocity = 0;
                            }
                        }

                        // Handle jumping
                        bool startingJump = isOnGround && false;
                        if (startingJump)
                        {
                            velocity.Y = -3f;
                            isOnGround = false;
                            isJumping = true;
                        }
                        else if (!isJumping) // Only check for falling if we're not in a jump
                        {
                            // Check if there's ground below us
                            //Vector2 groundCheckPos = _position + new Vector2(0, 1.0f);
                            Vector2 groundCheckPos = position + new Vector2(0, 1.0f);
                            bool diagonal = false;
                            bool isCenterOnDiagonal = false;
                            bool hasGroundBelow = CheckCollisionAtPosition(groundCheckPos, GameState.CurrentMap, collisionBounding, ref diagonal, ref isCenterOnDiagonal);

                            if (!hasGroundBelow || diagonal)
                            {
                                isOnGround = false;
                                if (velocity.Y >= 0) // Only apply gravity if we're not moving upward
                                {
                                    velocity.Y += gravity;
                                }
                            }
                        }
                        else
                        {
                            // in the middle of a jump, apply gravity
                            velocity.Y += gravity;
                        }


                        // Try X movement first
                        Vector2 proposedXPosition = position + new Vector2(proposedXVelocity, 0);
                        bool diagonalX = false;
                        bool isCenterOnDiagonalTile = false;
                        bool hasXCollision = CheckCollisionAtPosition(proposedXPosition, GameState.CurrentMap, collisionBounding, ref diagonalX, ref isCenterOnDiagonalTile);

                        // Apply X movement if no collision
                        if (!hasXCollision)
                        {
                            position.X = proposedXPosition.X;
                            velocity.X = proposedXVelocity;
                            if (Math.Abs(velocity.X) > 0.1f && !isJumping)
                            {
                                //PlayMoveSound(gameTime);
                            }
                        }
                        else
                        {
                            float newPosY = 0;
                            BoundingRectangle xTileRec = new();
                            // Check if the collision is with a diagonal tile
                            if (MapHelper.HandleDiagonalCollision(GameState.CurrentMap, position, proposedXPosition, collisionBounding, ref velocity, ref newPosY, ref xTileRec))
                            {
                                position.X = proposedXPosition.X;
                                velocity.X = proposedXVelocity;
                                if (newPosY != 0)
                                {
                                    if (!isJumping)
                                    {
                                        if (velocity.Y == 0)
                                        {
                                            position.Y = newPosY;
                                            isOnGround = true;
                                        }
                                        //_position.Y = newPosY;
                                        //IsOnGround = true;
                                    }
                                    //DiagonalPosY = newPosY;
                                }

                                if (Math.Abs(velocity.X) > 0.1f && !isJumping)
                                {
                                    //PlayMoveSound(gameTime);
                                }
                            }
                            else
                            {
                                // If it's not a diagonal tile, handle as normal collision
                                velocity.X = 0;
                            }
                        }

                        // Then try Y movement
                        if (velocity.Y != 0)
                        {
                            bool isDiagonal = false;
                            bool isCenterOnDiagonal = false;
                            Vector2 proposedYPosition = position + new Vector2(0, velocity.Y);
                            bool hasYCollision = CheckCollisionAtPosition(proposedYPosition, GameState.CurrentMap, collisionBounding, ref isDiagonal, ref isCenterOnDiagonal);
                            if (!hasYCollision && !isDiagonal)
                            {
                                position.Y = proposedYPosition.Y;
                                isOnGround = false;
                            }
                            else
                            {
                                if (Math.Abs(velocity.Y) > 0) // Moving downward
                                {
                                    if (collisionBounding is BoundingCircle bc)
                                    {
                                        bc.Center = new Vector2(position.X + bc.Radius, position.Y + bc.Radius);
                                    }
                                    else if (collisionBounding is BoundingRectangle br)
                                    {
                                        br.X = position.X;
                                        if (Math.Abs(br.Y - position.Y) < 2)
                                        {
                                            br.Y = position.Y;
                                        }

                                        br.Center = new Vector2(br.X + (br.Width / 2), br.Y + (br.Height / 2));
                                    }

                                    if (velocity.Y > 0)
                                    {
                                        // Check ground at both bottom corners
                                        float leftGroundY = MapHelper.GetGroundYPosition(
                                            GameState.CurrentMap,
                                            position.X,
                                            position.Y,
                                            textureInfo.UnitTextureHeight * textureInfo.SizeScale,
                                            collisionBounding
                                        );

                                        float rightGroundY = MapHelper.GetGroundYPosition(
                                            GameState.CurrentMap,
                                            position.X + (textureInfo.UnitTextureWidth * textureInfo.SizeScale),
                                            position.Y,
                                            textureInfo.UnitTextureHeight * textureInfo.SizeScale,
                                            collisionBounding
                                        );

                                        float leftPos = leftGroundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                        float rightPos = rightGroundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale);

                                        if (!isDiagonal)
                                        { }

                                        float groundY = Math.Min(leftGroundY, rightGroundY);
                                        float newGroundY;
                                        if (groundY < position.Y)
                                        {
                                            newGroundY = Math.Max(leftGroundY, rightGroundY) -
                                                         (textureInfo.UnitTextureHeight * textureInfo.SizeScale);

                                            if (Math.Abs(position.Y - newGroundY) < 15)
                                            {
                                                position.Y = newGroundY;
                                                isOnGround = true;
                                                if (isJumping) isJumping = false;
                                            }
                                            else
                                            {
                                                position.Y = proposedYPosition.Y;
                                            }


                                        }
                                        else
                                        {
                                            newGroundY = groundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                            //if (JumpDiagonalPosY == 0)
                                            //{
                                            //    JumpDiagonalPosY = newGroundY;
                                            //}

                                            if (position.Y < newGroundY)
                                            {
                                                position.Y = proposedYPosition.Y;
                                                isOnGround = false;
                                            }
                                            else
                                            {
                                                position.Y = newGroundY;
                                                isOnGround = true;
                                                if (isJumping) isJumping = false;
                                                velocity.Y = 0;
                                                //JumpDiagonalPosY = 0;
                                            }

                                        }

                                    }
                                    else
                                    {
                                        position.Y = proposedYPosition.Y;
                                        isOnGround = false;
                                    }
                                }

                            }

                            //DiagonalPosY = 0;
                        }

                        // Check map bounds
                        mapBounds = MapHelper.GetMapWorldBounds();
                        position.X = MathHelper.Clamp(position.X,
                            (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f,
                            mapBounds.Width - (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f);


                        Vector2 newPosition = new(position.X + velocity.X, position.Y + velocity.Y);
                        position = newPosition;

                        // Check if transition is complete
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

        private static bool CheckCollisionAtPosition(Vector2 position, TiledMap map, ICollisionBounding collisionBounding,
            ref bool isDiagonal, ref bool isCenterOnDiagonal)
        {
            int leftTile = 0;
            int rightTile = 0;
            int topTile = 0;
            int bottomTile = 0;

            // Update collision bounds for test position
            if (collisionBounding is BoundingRectangle br)
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
            else if (collisionBounding is BoundingCircle bc)
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

        private static bool CheckLayerCollision(Layer layer, int leftTile, int rightTile, int topTile, int bottomTile,
            Vector2 position, ICollisionBounding collisionBounding, ref bool isThisDiagonalTile,
            ref bool isCenterOnDiagonal)
        {
            int tilex = (int)(collisionBounding.Center.X / MapHelper.TileSize);
            int tiley = (int)(collisionBounding.Center.Y / MapHelper.TileSize);
            if (tilex == 63 && tiley == 19)
            {

            }

            if (tilex == 63 && tiley == 20)
            {

            }

            if (leftTile < 0 || rightTile < 0) { leftTile = rightTile = 0; }
            if (leftTile >= MapHelper.MapWidth || rightTile >= MapHelper.MapWidth) { leftTile = rightTile = MapHelper.MapWidth - 1; }
            if (topTile < 0 || bottomTile < 0) { topTile = bottomTile = 0; }
            if (topTile >= MapHelper.MapHeight || bottomTile >= MapHelper.MapHeight) { topTile = bottomTile = MapHelper.MapHeight - 1; }

            for (int y = topTile; y <= bottomTile; y++)
            {
                for (int x = leftTile; x <= rightTile; x++)
                {

                    if (x == 63)
                    {

                    }
                    if (x == 63 && y == 19)
                    {

                    }

                    if (x == 63 && y == 20)
                    {

                    }

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
                                if (collisionBounding is BoundingRectangle br)
                                {
                                    isDiagonalTile = true;
                                    
                                }
                                else if (collisionBounding is BoundingCircle bc)
                                {
                                    
                                }
                               
                            }
                        }
                        isThisDiagonalTile = isDiagonalTile;
                        if (x == tilex && y == tiley && !isDiagonalTile)
                        {

                            continue;
                        }

                        // Regular collision check for non-diagonal tiles
                        ICollisionBounding testBounds;
                        if (collisionBounding is BoundingRectangle bra)
                        {
                            testBounds = new BoundingRectangle(position.X, position.Y, bra.Width, bra.Height);
                            collisionBounding = testBounds;
                        }
                        else if (collisionBounding is BoundingCircle bc)
                        {
                            testBounds = new BoundingCircle(new Vector2(position.X, position.Y), bc.Radius);
                            collisionBounding = testBounds;
                        }
                        else
                        {
                            continue;
                        }

                        if (collisionBounding.CollidesWith(tileRect))
                        {
                            if (collisionBounding.Center.X <= tileRect.Right && collisionBounding.Center.X >= tileRect.Left)
                            {
                                isCenterOnDiagonal = true;
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
    }
}