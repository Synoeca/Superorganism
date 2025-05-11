using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Superorganism.AI;
using Superorganism.Collisions;
using Superorganism.Common;
using Superorganism.Entities;
using Superorganism.Tiles;

namespace Superorganism.Core.Managers;

/// <summary>
/// 
/// </summary>
public static class MovementUtilities
{
    private static readonly Random Rand = new();

    /// <summary>
    /// Handles player input for movement
    /// </summary>
    public static void HandlePlayerInput(
        ref Vector2 position,
        ref Vector2 velocity,
        ref bool isOnGround,
        ref bool isJumping,
        ref float jumpDiagonalPosY,
        ref bool isCenterOnDiagonal,
        ref float soundTimer,
        ref float movementSpeed,
        ref float animationSpeed,
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState,
        TiledMap currentMap,
        ICollisionBounding collisionBounding,
        TextureInfo textureInfo,
        EntityStatus entityStatus,
        ref bool flipped,
        float friction,
        float gravity,
        float jumpStrength,
        Action<GameTime> playMoveSound,
        GameTime gameTime)
    {
        //movementSpeed = 1.0f;
        movementSpeed = entityStatus.Agility * 1.0f;
        animationSpeed = 0.15f;

        // Update movement speed based on shift key
        if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
        {
            //movementSpeed = 4.5f;
            movementSpeed = entityStatus.Agility * 2.0f;
            animationSpeed = 0.1f;
        }

        float proposedXVelocity = 0;

        // Calculate proposed horizontal movement
        if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
        {
            proposedXVelocity = -movementSpeed;
            flipped = true;
        }
        else if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
        {
             proposedXVelocity = movementSpeed;
            flipped = false;
        }
        else if (isOnGround)
        {
            proposedXVelocity = velocity.X * friction;
            if (Math.Abs(proposedXVelocity) < 0.1f)
            {
                proposedXVelocity = 0;
                soundTimer = 0f;
            }
        }

        // Handle map modifications with F key
        if (previousKeyboardState.IsKeyDown(Keys.F) && keyboardState.IsKeyUp(Keys.F))
        {
            HandleMapModification(position, proposedXVelocity, keyboardState, currentMap, textureInfo);
        }

        // Handle jumping and physics movement
        bool startingJump = isOnGround && keyboardState.IsKeyDown(Keys.Space);

        HandleMovementPhysics(
            ref position,
            ref velocity,
            ref isOnGround,
            ref isJumping,
            ref jumpDiagonalPosY,
            ref isCenterOnDiagonal,
            proposedXVelocity,
            gravity,
            startingJump,
            jumpStrength,
            textureInfo,
            collisionBounding,
            currentMap,
            flipped,
            movementSpeed,
            gameTime,
            playMoveSound);
    }

    /// <summary>
    /// Handles F key map modification
    /// </summary>
    private static void HandleMapModification(
        Vector2 position,
        float proposedXVelocity,
        KeyboardState keyboardState,
        TiledMap currentMap,
        TextureInfo textureInfo)
    {
        if (proposedXVelocity > 0)
        {
            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
            {
                MapModifier.ModifyTileBelowPlayer(currentMap,
                    new Vector2(position.X + (textureInfo.UnitTextureWidth * textureInfo.SizeScale) + 5,
                    position.Y - (textureInfo.UnitTextureWidth * textureInfo.SizeScale) - 5),
                    false);
            }
            else
            {
                MapModifier.ModifyTileBelowPlayer(currentMap,
                    new Vector2(position.X + (textureInfo.UnitTextureWidth * textureInfo.SizeScale) + 5,
                    position.Y),
                    false);
            }
        }
        else if (proposedXVelocity < 0)
        {
            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
            {
                MapModifier.ModifyTileBelowPlayer(currentMap,
                    new Vector2(position.X,
                    position.Y - (textureInfo.UnitTextureWidth * textureInfo.SizeScale) - 5),
                    false);
            }
            else
            {
                MapModifier.ModifyTileBelowPlayer(currentMap, position, false);
            }
        }
        else
        {
            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
            {
                MapModifier.ModifyTileBelowPlayer(currentMap,
                    new Vector2(position.X + (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2,
                    position.Y - (textureInfo.UnitTextureWidth * textureInfo.SizeScale) - 5),
                    false);
            }
            else
            {
                MapModifier.ModifyTileBelowPlayer(currentMap,
                    new Vector2(position.X + (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2,
                    position.Y),
                    true);
            }
        }
    }

    /// <summary>
    /// Handles AI movement
    /// </summary>
    public static void HandleAIMovement(
        ref Vector2 position,
        ref Vector2 velocity,
        ref bool isOnGround,
        ref bool isJumping,
        ref float jumpDiagonalPosY,
        ref bool isCenterOnDiagonalSlope,
        TiledMap currentMap,
        ICollisionBounding collisionBounding,
        TextureInfo textureInfo,
        EntityStatus entityStatus,
        ref bool flipped,
        Strategy strategy,
        List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory,
        ref Vector2 lastKnownTargetPosition,
        List<Entity> entities,
        ref Strategy targetStrategy,
        double currentStrategyDuration,
        GameTime gameTime,
        bool canJump = false)
    {
        switch (strategy)
        {
            case Strategy.Patrol:
                HandlePatrolStrategy(
                    ref position,
                    ref velocity,
                    ref isOnGround,
                    ref isJumping,
                    ref jumpDiagonalPosY,
                    ref isCenterOnDiagonalSlope,
                    currentMap,
                    collisionBounding,
                    textureInfo,
                    entityStatus,
                    ref flipped,
                    ref strategy,
                    ref strategyHistory,
                    entities,
                    currentStrategyDuration,
                    ref lastKnownTargetPosition,
                    gameTime);
                break;

            case Strategy.ChaseEnemy:
                HandleChaseStrategy(
                    ref position,
                    ref velocity,
                    ref isOnGround,
                    ref isJumping,
                    ref jumpDiagonalPosY,
                    ref isCenterOnDiagonalSlope,
                    currentMap,
                    collisionBounding,
                    textureInfo,
                    entityStatus,
                    ref flipped,
                    ref strategy,
                    strategyHistory,
                    entities,
                    currentStrategyDuration,
                    ref lastKnownTargetPosition,
                    gameTime);
                break;

            case Strategy.Transition:
                HandleTransitionStrategy(
                    ref position,
                    ref velocity,
                    ref isOnGround,
                    ref isJumping,
                    ref jumpDiagonalPosY,
                    ref isCenterOnDiagonalSlope,
                    currentMap,
                    collisionBounding,
                    textureInfo,
                    entityStatus,
                    ref flipped,
                    ref strategy,
                    ref strategyHistory,
                    ref targetStrategy,
                    currentStrategyDuration,
                    gameTime);
                break;
        }
    }

    /// <summary>
    /// Handles player input for movement with stamina restrictions
    /// </summary>
    public static void HandlePlayerInputWithStaminaRestrictions(
        ref Vector2 position,
        ref Vector2 velocity,
        ref bool isOnGround,
        ref bool isJumping,
        ref float jumpDiagonalPosY,
        ref bool isCenterOnDiagonal,
        ref float soundTimer,
        ref float movementSpeed,
        ref float animationSpeed,
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState,
        TiledMap currentMap,
        ICollisionBounding collisionBounding,
        TextureInfo textureInfo,
        EntityStatus entityStatus,
        ref bool flipped,
        float friction,
        float gravity,
        float jumpStrength,
        Action<GameTime> playMoveSound,
        bool canSprint,
        bool canJump,
        GameTime gameTime)
    {
        //movementSpeed = 1.0f;
        movementSpeed = entityStatus.Agility * 1.0f;
        animationSpeed = 0.15f;

        // Update movement speed based on shift key AND stamina
        bool isAttemptingToSprint = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
        bool isSprinting = isAttemptingToSprint && canSprint;

        if (isSprinting)
        {
            //movementSpeed = 4.5f;
            movementSpeed = entityStatus.Agility * 2.0f;
            animationSpeed = 0.1f;
        }

        float proposedXVelocity = 0;

        // Calculate proposed horizontal movement
        if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
        {
            proposedXVelocity = -movementSpeed;
            flipped = true;
        }
        else if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
        {
            proposedXVelocity = movementSpeed;
            flipped = false;
        }
        else if (isOnGround)
        {
            proposedXVelocity = velocity.X * friction;
            if (Math.Abs(proposedXVelocity) < 0.1f)
            {
                proposedXVelocity = 0;
                soundTimer = 0f;
            }
        }

        // Handle map modifications with F key
        if (previousKeyboardState.IsKeyDown(Keys.F) && keyboardState.IsKeyUp(Keys.F))
        {
            HandleMapModification(position, proposedXVelocity, keyboardState, currentMap, textureInfo);
        }

        // Handle jumping and physics movement - only allow jumping if enough stamina
        bool isAttemptingToJump = isOnGround && keyboardState.IsKeyDown(Keys.Space);
        bool startingJump = isAttemptingToJump && canJump;
        if (proposedXVelocity > 0)
        {

        }

        HandleMovementPhysics(
            ref position,
            ref velocity,
            ref isOnGround,
            ref isJumping,
            ref jumpDiagonalPosY,
            ref isCenterOnDiagonal,
            proposedXVelocity,
            gravity,
            startingJump,
            jumpStrength,
            textureInfo,
            collisionBounding,
            currentMap,
            flipped,
            movementSpeed,
            gameTime,
            playMoveSound);
    }

    /// <summary>
    /// Handles AI patrol strategy movement
    /// </summary>
    public static void HandlePatrolStrategy(ref Vector2 position,
        ref Vector2 velocity,
        ref bool isOnGround,
        ref bool isJumping,
        ref float jumpDiagonalPosY,
        ref bool isCenterOnDiagonalSlope,
        TiledMap currentMap,
        ICollisionBounding collisionBounding,
        TextureInfo textureInfo,
        EntityStatus entityStatus,
        ref bool flipped,
        ref Strategy strategy,
        ref List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory,
        List<Entity> entities,
        double currentStrategyDuration,
        ref Vector2 lastKnownTargetPosition,
        GameTime gameTime)
    {
        //const float movementSpeed = 1.0f;
        float movementSpeed = entityStatus.Agility;
        const float gravity = 0.2f;

        float proposedXVelocity = velocity.X;
        if (velocity.X == 0)
        {
            proposedXVelocity = movementSpeed;
        }

        // Change direction every 3 seconds based on strategy duration
        if (currentStrategyDuration >= DecisionMaker.Rand.Next(DecisionMaker.Rand.Next(3, 21), 21))
        {
            proposedXVelocity = -proposedXVelocity; // Reverse direction
            (Strategy Strategy, double StartTime, double LastActionTime) current = strategyHistory[^1];
            strategyHistory[^1] = (current.Strategy, current.StartTime, gameTime.TotalGameTime.TotalSeconds);
        }

        // Handle ground friction
        HandleGroundFriction(ref proposedXVelocity, isOnGround);

        // Handle physics movement
        HandleMovementPhysics(
            ref position,
            ref velocity,
            ref isOnGround,
            ref isJumping,
            ref jumpDiagonalPosY,
            ref isCenterOnDiagonalSlope,
            proposedXVelocity,
            gravity,
            false, // No jumping in patrol
            -3f,   // Jump strength if jumping was enabled
            textureInfo,
            collisionBounding,
            currentMap,
            flipped,
            movementSpeed,
            gameTime);

        // Check for player to transition to chase
        foreach (Entity entity in entities)
        {
            if (entity is ControllableEntity { IsControlled: true } controllableEntity)
            {
                float distance = Vector2.Distance(position, controllableEntity.Position);
                if (distance < 100)
                {
                    TransitionToStrategy(ref strategy, Strategy.ChaseEnemy, ref strategyHistory, gameTime);
                    lastKnownTargetPosition = controllableEntity.Position;
                    return; // Exit early during transition
                }
            }
        }
    }

    /// <summary>
    /// Handles AI chase strategy movement
    /// </summary>
    public static void HandleChaseStrategy(ref Vector2 position,
        ref Vector2 velocity,
        ref bool isOnGround,
        ref bool isJumping,
        ref float jumpDiagonalPosY,
        ref bool isCenterOnDiagonalSlope,
        TiledMap currentMap,
        ICollisionBounding collisionBounding,
        TextureInfo textureInfo,
        EntityStatus entityStatus,
        ref bool flipped,
        ref Strategy strategy,
        List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory,
        List<Entity> entities,
        double currentStrategyDuration,
        ref Vector2 lastKnownTargetPosition,
        GameTime gameTime)
    {
        //const float chaseSpeed = 3.0f;
        float chaseSpeed = entityStatus.Agility * 2.0f;
        const float gravity = 0.2f;

        // Find target
        Vector2? targetPosition = null;
        float closestDistance = float.MaxValue;
        foreach (Entity entity in entities)
        {
            if (entity is ControllableEntity { IsControlled: true } controllableEntity)
            {
                float distance = Vector2.Distance(position, controllableEntity.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    targetPosition = controllableEntity.Position;
                    lastKnownTargetPosition = controllableEntity.Position;
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
        Vector2 targetPos = targetPosition ?? lastKnownTargetPosition;
        Vector2 chaseDirection = Vector2.Normalize(targetPos - position);
        float proposedXVelocity = chaseDirection.X * chaseSpeed;

        // Initialize movement if needed
        if (velocity.X == 0 && proposedXVelocity == 0)
        {
            proposedXVelocity = chaseSpeed;
        }

        // Handle physics
        HandleMovementPhysics(
            ref position,
            ref velocity,
            ref isOnGround,
            ref isJumping,
            ref jumpDiagonalPosY,
            ref isCenterOnDiagonalSlope,
            proposedXVelocity,
            gravity,
            false, // No jumping in chase
            -3f,   // Jump strength if jumping was enabled
            textureInfo,
            collisionBounding,
            currentMap,
            flipped,
            chaseSpeed,
            gameTime);
    }

    /// <summary>
    /// Handles AI transition strategy movement
    /// </summary>
    public static void HandleTransitionStrategy(ref Vector2 position,
        ref Vector2 velocity,
        ref bool isOnGround,
        ref bool isJumping,
        ref float jumpDiagonalPosY,
        ref bool isCenterOnDiagonalSlope,
        TiledMap currentMap,
        ICollisionBounding collisionBounding,
        TextureInfo textureInfo,
        EntityStatus entityStatus,
        ref bool flipped,
        ref Strategy strategy,
        ref List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory,
        ref Strategy targetStrategy,
        double currentStrategyDuration,
        GameTime gameTime)
    {
        const float gravity = 0.2f;
        float proposedXVelocity = 0; // Stay still during transition

        // Handle ground friction
        if (isOnGround)
        {
            if (Math.Abs(proposedXVelocity) < 0.1f)
            {
                proposedXVelocity = 0;
            }
        }

        // Handle physics movement
        HandleMovementPhysics(
            ref position,
            ref velocity,
            ref isOnGround,
            ref isJumping,
            ref jumpDiagonalPosY,
            ref isCenterOnDiagonalSlope,
            proposedXVelocity,
            gravity,
            false, // No jumping in transition
            -3f,   // Jump strength if jumping was enabled
            textureInfo,
            collisionBounding,
            currentMap,
            flipped,
            1.0f,  // Base movement speed
            gameTime);

        // Check if transition is complete
        if (currentStrategyDuration >= TransitionDuration)
        {
            AddStrategyToHistory(ref strategy, targetStrategy, ref strategyHistory, gameTime);
        }
    }

    /// <summary>
    /// Handles the physics movement for an entity
    /// </summary>
    public static void HandleMovementPhysics(
        ref Vector2 position,
        ref Vector2 velocity,
        ref bool isOnGround,
        ref bool isJumping,
        ref float jumpDiagonalPosY,
        ref bool isCenterOnDiagonalSlope,
        float proposedXVelocity,
        float gravity,
        bool startingJump,
        float jumpStrength,
        TextureInfo textureInfo,
        ICollisionBounding collisionBounding,
        TiledMap currentMap,
        bool flipped,
        float movementSpeed,
        GameTime gameTime,
        Action<GameTime> playMoveSound = null)
    {
        // Handle jumping
        if (startingJump)
        {
            velocity.Y = jumpStrength;
            isOnGround = false;
            isJumping = true;
            // Jump sound would be played here if passed in
        }
        else if (!isJumping) // Only check for falling if we're not in a jump
        {
            // Check if there's ground below us
            bool diagonal = false;
            bool isCenterOnDiagonal = false;
            bool yCollisionFromAbove = false;
            bool hasGroundBelow = CheckCollisionAtPosition(position, currentMap, collisionBounding, velocity.Y, ref diagonal, ref isCenterOnDiagonal, ref yCollisionFromAbove);

            if (!hasGroundBelow || diagonal)
            {
                isOnGround = false;
            }
            velocity.Y += gravity;
        }
        else
        {
            // In the middle of a jump, apply gravity
            velocity.Y += gravity;
        }

        // Try X movement first
        Vector2 proposedXPosition = position + new Vector2(proposedXVelocity, 0);
        bool diagonalX = false;
        bool isCenterOnDiagonalTile = false;
        bool isYCollisionFromAbove = false;
        bool hasXCollision = CheckCollisionAtPosition(proposedXPosition, currentMap, collisionBounding, velocity.Y, ref diagonalX, ref isCenterOnDiagonalTile, ref isYCollisionFromAbove);
        bool xMovementBlocked = false;

        // Apply X movement if no collision
        if (!hasXCollision)
        {
            position.X = proposedXPosition.X;
            velocity.X = proposedXVelocity;
            if (Math.Abs(velocity.X) > 0.1f && !isJumping)
            {
                playMoveSound?.Invoke(gameTime);
            }
        }
        else
        {
            float newPosY = 0;
            bool hasLeftDiagonal = false;
            bool hasRightDiagonal = false;
            BoundingRectangle xTileRec = new();

            // Check if the collision is with a diagonal tile
            if (MapHelper.HandleDiagonalCollision(currentMap, position, proposedXPosition, collisionBounding,
                    ref velocity, ref newPosY, ref xTileRec, ref hasLeftDiagonal, ref hasRightDiagonal))
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
                    }
                }

                if (Math.Abs(velocity.X) > 0.1f && !isJumping)
                {
                    playMoveSound?.Invoke(gameTime);
                }
            }
            else
            {
                // If it's not a diagonal tile, handle as normal collision
                velocity.X = 0;
                xMovementBlocked = true;
            }
        }

        // Then try Y movement
        if (velocity.Y != 0)
        {
            bool isDiagonal = false;
            bool isCenterOnDiagonal = false;
            bool yCollisionFromAbove = false;

            Vector2 proposedYPosition = position + new Vector2(0, velocity.Y);
            if (xMovementBlocked)
            {
                if (flipped)
                {
                    proposedYPosition.X += movementSpeed;
                }
                else
                {
                    proposedYPosition.X -= movementSpeed;
                }
            }

            if (isJumping)
            {

            }
            bool hasYCollision = CheckCollisionAtPosition(proposedYPosition, currentMap, collisionBounding, velocity.Y, ref isDiagonal, ref isCenterOnDiagonal, ref yCollisionFromAbove);

            if (!hasYCollision && !isDiagonal)
            {
                position.Y = proposedYPosition.Y;
                isOnGround = false;
            }
            else
            {
                if (Math.Abs(velocity.Y) > 0) // Moving downward or upward
                {
                    if (velocity.Y < 0 && !isDiagonal) // Moving upward
                    {
                        // Hit ceiling, stop upward movement
                        if (!xMovementBlocked && yCollisionFromAbove)
                        {
                            velocity.Y = 0;
                            isJumping = false;
                        }
                        else
                        {
                            if (flipped)
                            {
                                position.X += movementSpeed;
                            }
                            else
                            {
                                position.X -= movementSpeed;
                            }
                        }
                    }
                    else if (velocity.Y > 0) // Moving downward
                    {
                        HandleGroundCollision(
                            ref position,
                            ref velocity,
                            ref isOnGround,
                            ref isJumping,
                            ref jumpDiagonalPosY,
                            proposedYPosition,
                            xMovementBlocked,
                            movementSpeed,
                            textureInfo,
                            collisionBounding,
                            currentMap,
                            isCenterOnDiagonal,
                            flipped);
                    }
                    else
                    {
                        position.Y = proposedYPosition.Y;
                        isOnGround = false;
                    }
                }
            }
        }

        // Check map bounds
        Rectangle mapBounds = MapHelper.GetMapWorldBounds();
        position.X = MathHelper.Clamp(position.X,
            (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f,
            mapBounds.Width - (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f);

        // Clamp velocity
        velocity.X = MathHelper.Clamp(velocity.X, -movementSpeed * 2, movementSpeed * 2);
        isCenterOnDiagonalSlope = false;
    }

    /// <summary>
    /// Handles ground friction for movement
    /// </summary>
    private static void HandleGroundFriction(ref float proposedXVelocity, bool isOnGround)
    {
        if (isOnGround && Math.Abs(proposedXVelocity) < 0.1f)
        {
            proposedXVelocity = 0;
        }
    }


    /// <summary>
    /// Handles ground collision and diagonal slope movement
    /// </summary>
    private static void HandleGroundCollision(
        ref Vector2 position,
        ref Vector2 velocity,
        ref bool isOnGround,
        ref bool isJumping,
        ref float jumpDiagonalPosY,
        Vector2 proposedYPosition,
        bool xMovementBlocked,
        float movementSpeed,
        TextureInfo textureInfo,
        ICollisionBounding collisionBounding,
        TiledMap currentMap,
        bool isCenterOnDiagonal,
        bool flipped)
    {
        bool leftHitsDiagonal = false;
        bool rightHitsDiagonal = false;
        float leftSlope = 0;
        float rightSlope = 0;

        // Check ground at both bottom corners
        float leftGroundY = MapHelper.GetGroundYPosition(
            currentMap,
            position.X,
            position.Y,
            textureInfo.UnitTextureHeight * textureInfo.SizeScale,
            collisionBounding,
            ref leftHitsDiagonal,
            ref leftSlope
        );

        float rightGroundY = MapHelper.GetGroundYPosition(
            currentMap,
            position.X + (textureInfo.UnitTextureWidth * textureInfo.SizeScale),
            position.Y,
            textureInfo.UnitTextureHeight * textureInfo.SizeScale,
            collisionBounding,
            ref rightHitsDiagonal,
            ref rightSlope
        );

        float groundY = 0;

        if (isCenterOnDiagonal)
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
        if (groundY < position.Y)
        {
            newGroundY = Math.Max(leftGroundY, rightGroundY) -
                         (textureInfo.UnitTextureHeight * textureInfo.SizeScale);

            if (newGroundY - position.Y < 5)
            {
                position.Y = newGroundY;
                isOnGround = true;
                velocity.Y = 0;
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

            // For landing on mixed diagonal/flat tiles
            if (leftHitsDiagonal || rightHitsDiagonal)
            {
                if (leftHitsDiagonal && rightHitsDiagonal)
                {
                    newGroundY = Math.Min(leftGroundY, rightGroundY) -
                                 (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                }
                else if (leftHitsDiagonal)
                {
                    HandleLeftDiagonalSlope(
                        ref position,
                        ref newGroundY,
                        leftGroundY,
                        rightGroundY,
                        leftSlope,
                        xMovementBlocked,
                        movementSpeed,
                        textureInfo);
                }
                else if (rightHitsDiagonal)
                {
                    HandleRightDiagonalSlope(
                        ref position,
                        ref newGroundY,
                        leftGroundY,
                        rightGroundY,
                        rightSlope,
                        xMovementBlocked,
                        movementSpeed,
                        textureInfo);
                }
            }

            if (jumpDiagonalPosY == 0 ||
                (leftHitsDiagonal || rightHitsDiagonal) ||
                newGroundY < jumpDiagonalPosY || position.Y >= jumpDiagonalPosY)
            {
                jumpDiagonalPosY = newGroundY;
            }

            if (position.Y < jumpDiagonalPosY)
            {
                if (position.Y - proposedYPosition.Y > 20)
                {

                }
                position.Y = proposedYPosition.Y;
                isOnGround = false;
            }
            else
            {
                if (position.Y -  newGroundY > 16)
                {

                }
                position.Y = newGroundY;
                isOnGround = true;
                if (isJumping) isJumping = false;
                velocity.Y = 0;
                jumpDiagonalPosY = 0;
            }
        }
    }

    /// <summary>
    /// Handles left diagonal slope collision
    /// </summary>
    /// <summary>
    /// Helper constant for strategy transitions
    /// </summary>
    private const float TransitionDuration = 1.0f; // Duration in seconds

    /// <summary>
    /// Gets the last known target position from strategy history
    /// </summary>
    private static Vector2? GetLastTargetPosition(
        List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory,
        GameTime gameTime)
    {
        // Implementation would depend on how the game tracks target positions
        // This is a placeholder that would need to be implemented
        return null;
    }

    /// <summary>
    /// Transitions to a new strategy
    /// </summary>
    private static void TransitionToStrategy(
        ref Strategy currentStrategy,
        Strategy targetStrategy,
        ref List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory,
        GameTime gameTime)
    {
        AddStrategyToHistory(ref currentStrategy, Strategy.Transition, ref strategyHistory, gameTime);
        DecisionMaker._targetStrategy = targetStrategy;
    }

    /// <summary>
    /// Adds a strategy to history
    /// </summary>
    private static void AddStrategyToHistory(
        ref Strategy currentStrategy,
        Strategy newStrategy,
        ref List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory,
        GameTime gameTime)
    {
        currentStrategy = newStrategy;
        double currentTime = gameTime.TotalGameTime.TotalSeconds;
        strategyHistory.Add((newStrategy, currentTime, currentTime));
    }

    /// <summary>
    /// Handles left diagonal slope collision
    /// </summary>
    private static void HandleLeftDiagonalSlope(
        ref Vector2 position,
        ref float newGroundY,
        float leftGroundY,
        float rightGroundY,
        float leftSlope,
        bool xMovementBlocked,
        float movementSpeed,
        TextureInfo textureInfo)
    {
        if (leftSlope < 0)  // Downward slope (\)
        {
            if (leftGroundY > rightGroundY && leftGroundY - rightGroundY < 64 && position.Y < rightGroundY)
            {
                newGroundY = rightGroundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
            }
            else
            {
                float prevNewGrounY = newGroundY;
                if (prevNewGrounY > 959)
                {

                }
                if (xMovementBlocked)
                {
                    //position.X += movementSpeed;
                    newGroundY = leftGroundY - (textureInfo.ScaledHeight);
                }
                if (newGroundY - position.Y > 64)
                {
                    newGroundY = rightGroundY - (textureInfo.ScaledHeight);
                }
                else
                {
                    newGroundY = leftGroundY - (textureInfo.ScaledHeight);
                    if (position.Y - newGroundY > 15)
                    {
                        newGroundY = rightGroundY - (textureInfo.ScaledHeight);
                    }
                }
            }
        }
        else  // Upward slope (/)
        {
            if (leftGroundY < rightGroundY && rightGroundY - leftGroundY < 64)
            {
                if (rightGroundY - leftGroundY < 2)
                {
                    newGroundY = rightGroundY - (textureInfo.ScaledHeight);
                }
                else
                {
                    newGroundY = leftGroundY - (textureInfo.ScaledHeight);
                }
            }
            else
            {
                if (leftGroundY - rightGroundY < 64)
                {
                    newGroundY = leftGroundY - (textureInfo.ScaledHeight);
                }
                else
                {
                    newGroundY = rightGroundY - (textureInfo.ScaledHeight);
                }
            }
        }
    }

    /// <summary>
    /// Handles right diagonal slope collision
    /// </summary>
    private static void HandleRightDiagonalSlope(
        ref Vector2 position,
        ref float newGroundY,
        float leftGroundY,
        float rightGroundY,
        float rightSlope,
        bool xMovementBlocked,
        float movementSpeed,
        TextureInfo textureInfo)
    {
        if (rightSlope < 0)  // Downward slope (\)
        {
            if (leftGroundY < rightGroundY && rightGroundY - leftGroundY < 64)
            {
                newGroundY = rightGroundY - (textureInfo.ScaledHeight);
            }
            else
            {
                if (leftGroundY < position.Y)
                {
                    newGroundY = rightGroundY - (textureInfo.ScaledHeight);
                }
                else
                {
                    if (leftGroundY < rightGroundY)
                    {
                        newGroundY = leftGroundY - (textureInfo.ScaledHeight);
                    }
                    else
                    {
                        newGroundY = rightGroundY - (textureInfo.ScaledHeight);
                    }
                }
            }
        }
        else  // Upward slope (/)
        {
            if (leftGroundY < rightGroundY)
            {
                if (rightGroundY - leftGroundY < 64)
                {
                    if (leftGroundY > position.Y)
                    {
                        newGroundY = leftGroundY - (textureInfo.ScaledHeight);
                    }
                    else
                    {
                        newGroundY = rightGroundY - (textureInfo.ScaledHeight);
                    }
                }
                else
                {
                    newGroundY = rightGroundY - (textureInfo.ScaledHeight);
                    if (newGroundY - position.Y > 64)
                    {
                        newGroundY = leftGroundY - (textureInfo.ScaledHeight);
                    }
                }
            }
            else
            {
                if (xMovementBlocked)
                {
                    //position.X -= movementSpeed;
                    newGroundY = leftGroundY - (textureInfo.ScaledHeight);
                }
                else
                {
                    newGroundY = rightGroundY - (textureInfo.ScaledHeight);
                    if (position.Y - newGroundY > 15)
                    {
                        newGroundY = leftGroundY - (textureInfo.ScaledHeight);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks for collision at a position
    /// </summary>
    public static bool CheckCollisionAtPosition(Vector2 position,
        TiledMap map,
        ICollisionBounding collisionBounding,
        float velocityY,
        ref bool isDiagonal,
        ref bool isCenterOnDiagonal,
        ref bool yCollisionFromAbove)
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
            if (CheckLayerCollision(layer, leftTile, rightTile, topTile, bottomTile, position, collisionBounding, velocityY, ref isDiagonal, ref isCenterOnDiagonal, ref yCollisionFromAbove))
                return true;
        }

        // Check collision with group layers
        foreach (Group group in map.Groups.Values)
        {
            foreach (Layer layer in group.Layers.Values)
            {
                if (CheckLayerCollision(layer, leftTile, rightTile, topTile, bottomTile, position, collisionBounding, velocityY, ref isDiagonal, ref isCenterOnDiagonal, ref yCollisionFromAbove))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks for collision with a specific layer
    /// </summary>
    private static bool CheckLayerCollision(Layer layer,
        int leftTile,
        int rightTile,
        int topTile,
        int bottomTile,
        Vector2 position,
        ICollisionBounding collisionBounding,
        float velocityY,
        ref bool isThisDiagonalTile,
        ref bool isCenterOnDiagonal, 
        ref bool yCollisionFromAbove)
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
                                float tileLeft = x * MapHelper.TileSize;
                                float slope = (slopeRight - slopeLeft) / (float)MapHelper.TileSize;
                                float distanceFromLeft = collisionBounding.Center.X - tileLeft;
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

                                if (position.Y < slopeY && velocityY < 0)
                                {
                                    isDiagonalTile = true;
                                    yCollisionFromAbove = true;
                                }
                            }
                            else if (collisionBounding is BoundingCircle)
                            {
                                // Add implementation for BoundingCircle if needed
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
                        if (x == 102 && y == 13)
                        {

                        }
                        if (collisionBounding.Center.X <= tileRect.Right && collisionBounding.Center.X >= tileRect.Left)
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

                        if (velocityY < 0)
                        {
                            yCollisionFromAbove = true;
                            if (collisionBounding is BoundingRectangle brag)
                            {
                                if (tileRect.Left < brag.Left)
                                {
                                    if (tileRect.Right > brag.Left)
                                    {
                                        if (tileRect.Right - 5 < brag.Left)
                                        {
                                            yCollisionFromAbove = false;
                                        }
                                    }
                                }
                                if (tileRect.Right > brag.Right)
                                {
                                    if (tileRect.Left < brag.Right)
                                    {
                                        if (tileRect.Left + 5 > brag.Right)
                                        {
                                            yCollisionFromAbove = false;
                                        }
                                    }
                                }

                                if (velocityY < 0 && yCollisionFromAbove)
                                {

                                }
                            }
                        }
                        return true;
                    }
                }
            }
        }
        return false;
    }
}