using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Superorganism.AI;
using Superorganism.Collisions;
using Superorganism.Common;
using Superorganism.Entities;
using Superorganism.Tiles;
using Superorganism.Core.Timing;

namespace Superorganism.Core.Managers;

/// <summary>
/// Static utility class that provides comprehensive movement and physics functionality
/// for both player and AI entities. Handles player input, physics calculations,
/// collision detection, and various AI movement strategies.
/// </summary>
public static class MovementUtilities
{
    /// <summary>
    /// Random number generator used for AI movement patterns and decision-making.
    /// </summary>
    private static readonly Random Rand = new();

    /// <summary>
    /// Handles player input for movement including walking, running, and jumping.
    /// Processes keyboard input and applies appropriate movement physics.
    /// </summary>
    /// <param name="position">Current position of the player entity.</param>
    /// <param name="velocity">Current velocity of the player entity.</param>
    /// <param name="isOnGround">Whether the player is currently on the ground.</param>
    /// <param name="isJumping">Whether the player is currently in a jumping state.</param>
    /// <param name="jumpDiagonalPosY">Y position for diagonal slope jumping calculations.</param>
    /// <param name="isCenterOnDiagonal">Whether the player is centered on a diagonal slope.</param>
    /// <param name="soundTimer">Timer for controlling movement sound playback.</param>
    /// <param name="movementSpeed">Current movement speed of the player.</param>
    /// <param name="animationSpeed">Speed of the walking animation.</param>
    /// <param name="keyboardState">Current frame's keyboard state.</param>
    /// <param name="previousKeyboardState">Previous frame's keyboard state for edge detection.</param>
    /// <param name="currentMap">The current tiled map for collision detection.</param>
    /// <param name="collisionBounding">Collision boundary for the player entity.</param>
    /// <param name="textureInfo">Information about the player's texture and size.</param>
    /// <param name="entityStatus">Current stats and status of the player entity.</param>
    /// <param name="flipped">Whether the player sprite is horizontally flipped.</param>
    /// <param name="friction">Ground friction coefficient affecting movement.</param>
    /// <param name="gravity">Gravity acceleration applied to the player.</param>
    /// <param name="jumpStrength">Strength/force of the player's jump.</param>
    /// <param name="playMoveSound">Action callback for playing movement sounds.</param>
    /// <param name="gameTime">Game timing information.</param>
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
        if (keyboardState.IsKeyDown(Keys.A))
        {
            proposedXVelocity = -movementSpeed;
            flipped = true;
        }
        else if (keyboardState.IsKeyDown(Keys.D))
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
    /// Handles map modification based on F key input and directional keys.
    /// Allows players to remove or place tiles in the game world.
    /// </summary>
    /// <param name="position">Current position of the player.</param>
    /// <param name="proposedXVelocity">The intended horizontal velocity.</param>
    /// <param name="keyboardState">Current keyboard state for directional inputs.</param>
    /// <param name="currentMap">The map to modify.</param>
    /// <param name="textureInfo">Player texture information for calculating modification position.</param>
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
    /// Handles AI movement based on the current strategy. Routes to specific strategy handlers
    /// and manages overall AI behavior patterns.
    /// </summary>
    /// <param name="position">Current position of the AI entity.</param>
    /// <param name="velocity">Current velocity of the AI entity.</param>
    /// <param name="isOnGround">Whether the AI is currently on the ground.</param>
    /// <param name="isJumping">Whether the AI is currently jumping.</param>
    /// <param name="jumpDiagonalPosY">Y position for diagonal jumping calculations.</param>
    /// <param name="isCenterOnDiagonalSlope">Whether the AI is centered on a diagonal slope.</param>
    /// <param name="currentMap">The current tiled map for collision detection.</param>
    /// <param name="collisionBounding">Collision boundary for the AI entity.</param>
    /// <param name="textureInfo">Information about the AI's texture and size.</param>
    /// <param name="entityStatus">Current stats and status of the AI entity.</param>
    /// <param name="flipped">Whether the AI sprite is horizontally flipped.</param>
    /// <param name="strategy">Current AI strategy being executed.</param>
    /// <param name="strategyHistory">History of strategies for tracking behavior patterns.</param>
    /// <param name="lastKnownTargetPosition">Last known position of the target for chase strategies.</param>
    /// <param name="entities">List of all active entities for proximity checks.</param>
    /// <param name="targetStrategy">Target strategy during transitions.</param>
    /// <param name="currentStrategyDuration">How long the current strategy has been active.</param>
    /// <param name="gameTime">Game timing information.</param>
    /// <param name="canJump">Whether the AI is allowed to jump (default: false).</param>
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
    /// Handles player input with stamina restrictions on sprinting and jumping.
    /// Similar to HandlePlayerInput but includes stamina checks before allowing
    /// energy-intensive actions.
    /// </summary>
    /// <param name="position">Current position of the player entity.</param>
    /// <param name="velocity">Current velocity of the player entity.</param>
    /// <param name="isOnGround">Whether the player is currently on the ground.</param>
    /// <param name="isJumping">Whether the player is currently in a jumping state.</param>
    /// <param name="jumpDiagonalPosY">Y position for diagonal slope jumping calculations.</param>
    /// <param name="isCenterOnDiagonal">Whether the player is centered on a diagonal slope.</param>
    /// <param name="soundTimer">Timer for controlling movement sound playback.</param>
    /// <param name="movementSpeed">Current movement speed of the player.</param>
    /// <param name="animationSpeed">Speed of the walking animation.</param>
    /// <param name="keyboardState">Current frame's keyboard state.</param>
    /// <param name="previousKeyboardState">Previous frame's keyboard state for edge detection.</param>
    /// <param name="currentMap">The current tiled map for collision detection.</param>
    /// <param name="collisionBounding">Collision boundary for the player entity.</param>
    /// <param name="textureInfo">Information about the player's texture and size.</param>
    /// <param name="entityStatus">Current stats and status of the player entity.</param>
    /// <param name="flipped">Whether the player sprite is horizontally flipped.</param>
    /// <param name="friction">Ground friction coefficient affecting movement.</param>
    /// <param name="gravity">Gravity acceleration applied to the player.</param>
    /// <param name="jumpStrength">Strength/force of the player's jump.</param>
    /// <param name="playMoveSound">Action callback for playing movement sounds.</param>
    /// <param name="canSprint">Whether the player has enough stamina to sprint.</param>
    /// <param name="canJump">Whether the player has enough stamina to jump.</param>
    /// <param name="gameTime">Game timing information.</param>
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
        if (keyboardState.IsKeyDown(Keys.A))
        {
            proposedXVelocity = -movementSpeed;
            flipped = true;
        }
        else if (keyboardState.IsKeyDown(Keys.D))
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
    /// Handles the AI patrol strategy where entities move in a pattern,
    /// typically back and forth, until they detect a target to chase.
    /// </summary>
    /// <param name="position">Current position of the AI entity.</param>
    /// <param name="velocity">Current velocity of the AI entity.</param>
    /// <param name="isOnGround">Whether the AI is currently on the ground.</param>
    /// <param name="isJumping">Whether the AI is currently jumping.</param>
    /// <param name="jumpDiagonalPosY">Y position for diagonal jumping calculations.</param>
    /// <param name="isCenterOnDiagonalSlope">Whether the AI is centered on a diagonal slope.</param>
    /// <param name="currentMap">The current tiled map for collision detection.</param>
    /// <param name="collisionBounding">Collision boundary for the AI entity.</param>
    /// <param name="textureInfo">Information about the AI's texture and size.</param>
    /// <param name="entityStatus">Current stats and status of the AI entity.</param>
    /// <param name="flipped">Whether the AI sprite is horizontally flipped.</param>
    /// <param name="strategy">Current AI strategy reference.</param>
    /// <param name="strategyHistory">History of strategies for tracking behavior patterns.</param>
    /// <param name="entities">List of all active entities for proximity checks.</param>
    /// <param name="currentStrategyDuration">How long the current strategy has been active.</param>
    /// <param name="lastKnownTargetPosition">Last known position of any target (for persistence).</param>
    /// <param name="gameTime">Game timing information.</param>
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
            // Use GameTimer instead of gameTime.TotalGameTime.TotalSeconds
            strategyHistory[^1] = (current.Strategy, current.StartTime, GameTimer.TotalGameplayTime);
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
    /// Handles the AI chase strategy where entities pursue a target aggressively.
    /// Entities will use their last known target position if the target goes out of range.
    /// </summary>
    /// <param name="position">Current position of the AI entity.</param>
    /// <param name="velocity">Current velocity of the AI entity.</param>
    /// <param name="isOnGround">Whether the AI is currently on the ground.</param>
    /// <param name="isJumping">Whether the AI is currently jumping.</param>
    /// <param name="jumpDiagonalPosY">Y position for diagonal jumping calculations.</param>
    /// <param name="isCenterOnDiagonalSlope">Whether the AI is centered on a diagonal slope.</param>
    /// <param name="currentMap">The current tiled map for collision detection.</param>
    /// <param name="collisionBounding">Collision boundary for the AI entity.</param>
    /// <param name="textureInfo">Information about the AI's texture and size.</param>
    /// <param name="entityStatus">Current stats and status of the AI entity.</param>
    /// <param name="flipped">Whether the AI sprite is horizontally flipped.</param>
    /// <param name="strategy">Current AI strategy reference.</param>
    /// <param name="strategyHistory">History of strategies for tracking behavior patterns.</param>
    /// <param name="entities">List of all active entities for finding targets.</param>
    /// <param name="currentStrategyDuration">How long the current strategy has been active.</param>
    /// <param name="lastKnownTargetPosition">Last known position of the target.</param>
    /// <param name="gameTime">Game timing information.</param>
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
    /// Handles the AI transition strategy which creates a pause between strategy changes.
    /// Entities remain stationary during transitions to create more natural behavior changes.
    /// </summary>
    /// <param name="position">Current position of the AI entity.</param>
    /// <param name="velocity">Current velocity of the AI entity.</param>
    /// <param name="isOnGround">Whether the AI is currently on the ground.</param>
    /// <param name="isJumping">Whether the AI is currently jumping.</param>
    /// <param name="jumpDiagonalPosY">Y position for diagonal jumping calculations.</param>
    /// <param name="isCenterOnDiagonalSlope">Whether the AI is centered on a diagonal slope.</param>
    /// <param name="currentMap">The current tiled map for collision detection.</param>
    /// <param name="collisionBounding">Collision boundary for the AI entity.</param>
    /// <param name="textureInfo">Information about the AI's texture and size.</param>
    /// <param name="entityStatus">Current stats and status of the AI entity.</param>
    /// <param name="flipped">Whether the AI sprite is horizontally flipped.</param>
    /// <param name="strategy">Current AI strategy reference.</param>
    /// <param name="strategyHistory">History of strategies for tracking behavior patterns.</param>
    /// <param name="targetStrategy">The strategy to transition to after the transition completes.</param>
    /// <param name="currentStrategyDuration">How long the current strategy has been active.</param>
    /// <param name="gameTime">Game timing information.</param>
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
    /// Handles the core physics movement for any entity including collision detection,
    /// jumping, gravity, and diagonal slope interactions.
    /// </summary>
    /// <param name="position">Current position of the entity.</param>
    /// <param name="velocity">Current velocity of the entity.</param>
    /// <param name="isOnGround">Whether the entity is currently on the ground.</param>
    /// <param name="isJumping">Whether the entity is currently jumping.</param>
    /// <param name="jumpDiagonalPosY">Y position for diagonal jumping calculations.</param>
    /// <param name="isCenterOnDiagonalSlope">Whether the entity is centered on a diagonal slope.</param>
    /// <param name="proposedXVelocity">The intended horizontal velocity for this frame.</param>
    /// <param name="gravity">Gravity acceleration to apply.</param>
    /// <param name="startingJump">Whether a jump is being initiated this frame.</param>
    /// <param name="jumpStrength">Strength/force of the jump (typically negative).</param>
    /// <param name="textureInfo">Information about the entity's texture and size.</param>
    /// <param name="collisionBounding">Collision boundary for the entity.</param>
    /// <param name="currentMap">The current tiled map for collision detection.</param>
    /// <param name="flipped">Whether the entity sprite is horizontally flipped.</param>
    /// <param name="movementSpeed">Base movement speed for the entity.</param>
    /// <param name="gameTime">Game timing information.</param>
    /// <param name="playMoveSound">Optional callback for playing movement sounds.</param>
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
            if (TilePhysicsInspector.HandleDiagonalCollision(currentMap, position, proposedXPosition, collisionBounding,
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
        Rectangle mapBounds = TilePhysicsInspector.GetMapWorldBounds();
        position.X = MathHelper.Clamp(position.X,
            (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f,
            mapBounds.Width - (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f);

        // Clamp velocity
        velocity.X = MathHelper.Clamp(velocity.X, -movementSpeed * 2, movementSpeed * 2);
        isCenterOnDiagonalSlope = false;
    }

    /// <summary>
    /// Handles ground friction for movement, reducing velocity when the entity is on the ground
    /// and not actively moving. Prevents sliding on surfaces.
    /// </summary>
    /// <param name="proposedXVelocity">Current proposed horizontal velocity.</param>
    /// <param name="isOnGround">Whether the entity is currently on the ground.</param>
    private static void HandleGroundFriction(ref float proposedXVelocity, bool isOnGround)
    {
        if (isOnGround && Math.Abs(proposedXVelocity) < 0.1f)
        {
            proposedXVelocity = 0;
        }
    }


    /// <summary>
    /// Handles ground collision detection and adjusts the entity's position when landing on surfaces,
    /// including special handling for diagonal slopes and mixed terrain types.
    /// </summary>
    /// <param name="position">Current position of the entity.</param>
    /// <param name="velocity">Current velocity of the entity.</param>
    /// <param name="isOnGround">Whether the entity is currently on the ground.</param>
    /// <param name="isJumping">Whether the entity is currently jumping.</param>
    /// <param name="jumpDiagonalPosY">Y position for diagonal jumping calculations.</param>
    /// <param name="proposedYPosition">The intended Y position after movement.</param>
    /// <param name="xMovementBlocked">Whether horizontal movement was blocked by collision.</param>
    /// <param name="movementSpeed">Base movement speed for position adjustments.</param>
    /// <param name="textureInfo">Information about the entity's texture and size.</param>
    /// <param name="collisionBounding">Collision boundary for the entity.</param>
    /// <param name="currentMap">The current tiled map for collision detection.</param>
    /// <param name="isCenterOnDiagonal">Whether the entity is centered on a diagonal slope.</param>
    /// <param name="flipped">Whether the entity sprite is horizontally flipped.</param>
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
        float leftGroundY = TilePhysicsInspector.GetGroundYPosition(
            currentMap,
            position.X,
            position.Y,
            textureInfo.UnitTextureHeight * textureInfo.SizeScale,
            collisionBounding,
            ref leftHitsDiagonal,
            ref leftSlope
        );

        float rightGroundY = TilePhysicsInspector.GetGroundYPosition(
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
    /// Duration in seconds for strategy transitions. Provides a pause between
    /// different AI behaviors for more natural transitions.
    /// </summary>
    private const float TransitionDuration = 1.0f;

    /// <summary>
    /// Gets the last known target position from the strategy history.
    /// Used when an AI loses sight of its target during a chase.
    /// </summary>
    /// <param name="strategyHistory">History of strategies for tracking behavior patterns.</param>
    /// <param name="gameTime">Game timing information.</param>
    /// <returns>The last known target position, or null if no target has been tracked.</returns>
    private static Vector2? GetLastTargetPosition(
        List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory,
        GameTime gameTime)
    {
        // Implementation would depend on how the game tracks target positions
        // This is a placeholder that would need to be implemented
        return null;
    }

    /// <summary>
    /// Initiates a transition to a new strategy. First switches to a transition state
    /// before switching to the target strategy.
    /// </summary>
    /// <param name="currentStrategy">Current AI strategy reference.</param>
    /// <param name="targetStrategy">Strategy to transition to.</param>
    /// <param name="strategyHistory">History of strategies for tracking behavior patterns.</param>
    /// <param name="gameTime">Game timing information.</param>
    private static void TransitionToStrategy(
        ref Strategy currentStrategy,
        Strategy targetStrategy,
        ref List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory,
        GameTime gameTime)
    {
        AddStrategyToHistory(ref currentStrategy, Strategy.Transition, ref strategyHistory, gameTime);
        DecisionMaker.TargetStrategy = targetStrategy;
    }

    /// <summary>
    /// Adds a new strategy to the AI's strategy history and updates the current strategy.
    /// </summary>
    /// <param name="currentStrategy">Current AI strategy reference.</param>
    /// <param name="newStrategy">New strategy to add to history and set as current.</param>
    /// <param name="strategyHistory">History of strategies for tracking behavior patterns.</param>
    /// <param name="gameTime">Game timing information.</param>
    private static void AddStrategyToHistory(
        ref Strategy currentStrategy,
        Strategy newStrategy,
        ref List<(Strategy Strategy, double StartTime, double LastActionTime)> strategyHistory,
        GameTime gameTime)
    {
        currentStrategy = newStrategy;
        // Use GameTimer instead of gameTime.TotalGameTime.TotalSeconds
        double currentTime = GameTimer.TotalGameplayTime;
        strategyHistory.Add((newStrategy, currentTime, currentTime));
    }

    /// <summary>
    /// Handles specific collision behavior for left diagonal slopes.
    /// Adjusts entity position based on slope direction and collision context.
    /// </summary>
    /// <param name="position">Current position of the entity.</param>
    /// <param name="newGroundY">The calculated ground Y position to be adjusted.</param>
    /// <param name="leftGroundY">Ground Y position on the left side of the entity.</param>
    /// <param name="rightGroundY">Ground Y position on the right side of the entity.</param>
    /// <param name="leftSlope">Slope value of the left diagonal tile.</param>
    /// <param name="xMovementBlocked">Whether horizontal movement was blocked.</param>
    /// <param name="movementSpeed">Base movement speed for position adjustments.</param>
    /// <param name="textureInfo">Information about the entity's texture and size.</param>
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
                if (xMovementBlocked)
                {
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
    /// Handles specific collision behavior for right diagonal slopes.
    /// Adjusts entity position based on slope direction and collision context.
    /// </summary>
    /// <param name="position">Current position of the entity.</param>
    /// <param name="newGroundY">The calculated ground Y position to be adjusted.</param>
    /// <param name="leftGroundY">Ground Y position on the left side of the entity.</param>
    /// <param name="rightGroundY">Ground Y position on the right side of the entity.</param>
    /// <param name="rightSlope">Slope value of the right diagonal tile.</param>
    /// <param name="xMovementBlocked">Whether horizontal movement was blocked.</param>
    /// <param name="movementSpeed">Base movement speed for position adjustments.</param>
    /// <param name="textureInfo">Information about the entity's texture and size.</param>
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
    /// Checks for collision at a specific position by testing against all map layers.
    /// Returns collision information including diagonal status for specialized handling.
    /// </summary>
    /// <param name="position">Position to check for collision.</param>
    /// <param name="map">The tiled map to check against.</param>
    /// <param name="collisionBounding">Collision boundary for the entity.</param>
    /// <param name="velocityY">Vertical velocity for detecting fall-through conditions.</param>
    /// <param name="isDiagonal">Outputs whether collision is with a diagonal tile.</param>
    /// <param name="isCenterOnDiagonal">Outputs whether the entity center is on a diagonal.</param>
    /// <param name="yCollisionFromAbove">Outputs whether collision is from above (ceiling hit).</param>
    /// <returns>True if collision is detected, false otherwise.</returns>
    /// <returns></returns>
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

            leftTile = (int)(testBounds.Left / TilePhysicsInspector.TileSize) - 1;
            rightTile = (int)Math.Ceiling(testBounds.Right / TilePhysicsInspector.TileSize);
            topTile = (int)(testBounds.Top / TilePhysicsInspector.TileSize) - 1;
            bottomTile = (int)Math.Ceiling(testBounds.Bottom / TilePhysicsInspector.TileSize) - 1;
        }
        else if (collisionBounding is BoundingCircle bc)
        {
            Vector2 testCenter = new(position.X, position.Y);
            leftTile = (int)((testCenter.X - bc.Radius) / TilePhysicsInspector.TileSize);
            rightTile = (int)Math.Ceiling((testCenter.X + bc.Radius) / TilePhysicsInspector.TileSize);
            topTile = (int)((testCenter.Y - bc.Radius) / TilePhysicsInspector.TileSize);
            bottomTile = (int)Math.Ceiling((testCenter.Y + bc.Radius) / TilePhysicsInspector.TileSize);
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
    /// Checks for collision with a specific layer within the given tile bounds.
    /// Handles both regular and diagonal tile collisions.
    /// </summary>
    /// <param name="layer">Map layer to check.</param>
    /// <param name="leftTile">Left boundary tile index.</param>
    /// <param name="rightTile">Right boundary tile index.</param>
    /// <param name="topTile">Top boundary tile index.</param>
    /// <param name="bottomTile">Bottom boundary tile index.</param>
    /// <param name="position">Entity position to check.</param>
    /// <param name="collisionBounding">Collision boundary for the entity.</param>
    /// <param name="velocityY">Vertical velocity for detecting movement direction.</param>
    /// <param name="isThisDiagonalTile">Outputs whether a diagonal tile was hit.</param>
    /// <param name="isCenterOnDiagonal">Outputs whether entity center is on a diagonal.</param>
    /// <param name="yCollisionFromAbove">Outputs whether collision is from above.</param>
    /// <returns>True if collision is detected, false otherwise.</returns>
    /// <returns></returns>
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
        int tilex = (int)(collisionBounding.Center.X / TilePhysicsInspector.TileSize);
        int tiley = (int)(collisionBounding.Center.Y / TilePhysicsInspector.TileSize);

        if (leftTile < 0 || rightTile < 0) { leftTile = rightTile = 0; }
        if (leftTile >= TilePhysicsInspector.MapWidth || rightTile >= TilePhysicsInspector.MapWidth) { leftTile = rightTile = TilePhysicsInspector.MapWidth - 1; }
        if (topTile < 0 || bottomTile < 0) { topTile = bottomTile = 0; }
        if (topTile >= TilePhysicsInspector.MapHeight || bottomTile >= TilePhysicsInspector.MapHeight) { topTile = bottomTile = TilePhysicsInspector.MapHeight - 1; }

        for (int y = topTile; y <= bottomTile; y++)
        {
            for (int x = leftTile; x <= rightTile; x++)
            {
                int tileId = layer.GetTile(x, y);

                if (tileId != 0)
                {
                    Dictionary<string, string> property = TilePhysicsInspector.GetTileProperties(tileId);

                    // Skip non-collidable tiles
                    if (property.TryGetValue("isCollidable", out string isCollidable) && isCollidable == "false")
                    {
                        continue;
                    }

                    BoundingRectangle tileRect = new(
                        x * TilePhysicsInspector.TileSize,
                        y * TilePhysicsInspector.TileSize,
                        TilePhysicsInspector.TileSize - 5,
                        TilePhysicsInspector.TileSize - 5
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
                                float tileLeft = x * TilePhysicsInspector.TileSize;
                                float slope = (slopeRight - slopeLeft) / (float)TilePhysicsInspector.TileSize;
                                float distanceFromLeft = collisionBounding.Center.X - tileLeft;
                                float tileBottom = (y + 1) * TilePhysicsInspector.TileSize;

                                float slopeY;
                                if (slope > 0)
                                {
                                    slopeY = (tileBottom - slopeLeft) - (slope * distanceFromLeft);
                                }
                                else
                                {
                                    slopeY = (tileBottom - slopeRight) + (slope * (TilePhysicsInspector.TileSize - Math.Abs(distanceFromLeft)));
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