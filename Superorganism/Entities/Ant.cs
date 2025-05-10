using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using Superorganism.Common;
using Superorganism.Core.Managers;
namespace Superorganism.Entities
{
    /// <summary>
    /// Represents a controllable ant entity in the game with resource management
    /// </summary>
    public sealed class Ant : ControllableEntity
    {
        // Timers for resource regeneration
        private float _staminaRegenTimer = 0f;
        private float _hungerTimer = 0f;
        private const float StaminaRegenDelay = 3.0f; // Seconds before stamina starts regenerating
        private const float StaminaRegenRate = 5.0f; // Amount regenerated per second when resting

        // Hunger decrease timers (in seconds)
        private const float IdleHungerDecreaseTime = 180.0f;  // 3 minutes for 1 hunger point when idle
        private const float MovingHungerDecreaseTime = 30.0f; // 30 seconds for 1 hunger point when moving
        private const float SprintingHungerDecreaseTime = 10.0f; // 10 seconds for 1 hunger point when sprinting

        // Resource consumption rates
        private const float StaminaSprintCost = 1.5f; // Stamina cost per second while sprinting

        // Movement speed modifiers based on stamina
        private const float LowStaminaThreshold = 15; // Below this stamina, movement is reduced
        private const float LowStaminaSpeedMultiplier = 0.6f; // Speed multiplier when stamina is low

        private const float JumpStaminaThreshold = 10f; // Minimum stamina required to jump
        private const float SprintStaminaThreshold = 0f; // Minimum stamina required to sprint (can sprint until 0)


        /// <summary>
        /// Initializes a new instance of the <see cref="Ant"/> class with default properties
        /// </summary>
        public Ant()
        {
            IsSpriteAtlas = true;
            HasDirection = false;
            Color = Color.White;
            EntityStatus = new EntityStatus();
        }

        /// <summary>
        /// Gets or sets the current hit points of the ant
        /// </summary>
        public int HitPoints { get; set; } = 100;

        /// <summary>
        /// Gets or sets the maximum hit points the ant can have.
        /// Used for health cap calculations and UI representation.
        /// </summary>
        public int MaxHitPoint { get; set; } = 100;

        /// <summary>
        /// Gets or sets the current stamina of the ant
        /// </summary>
        public int Stamina { get; set; } = 100;

        /// <summary>
        /// Gets or sets the maximum stamina the ant can have
        /// </summary>
        public int MaxStamina { get; set; } = 100;

        /// <summary>
        /// Gets or sets the current hunger level of the ant
        /// </summary>
        public int Hunger { get; set; } = 100;

        /// <summary>
        /// Gets or sets the maximum hunger level the ant can have
        /// </summary>
        public int MaxHunger { get; set; } = 100;


        /// <summary>
        /// Override of the Update method to handle resource management
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Check for active movement input (not just velocity)
            bool isActivelyMoving = KeyboardState.IsKeyDown(Keys.Left) ||
                                  KeyboardState.IsKeyDown(Keys.Right) ||
                                  KeyboardState.IsKeyDown(Keys.A) ||
                                  KeyboardState.IsKeyDown(Keys.D);

            bool isSprinting = KeyboardState.IsKeyDown(Keys.LeftShift) ||
                              KeyboardState.IsKeyDown(Keys.RightShift);

            // Handle hunger decrease based on activity level
            _hungerTimer += deltaTime;

            float currentHungerDecreaseTime;
            if (isActivelyMoving && isSprinting)
            {
                currentHungerDecreaseTime = SprintingHungerDecreaseTime;
            }
            else if (isActivelyMoving)
            {
                currentHungerDecreaseTime = MovingHungerDecreaseTime;
            }
            else
            {
                currentHungerDecreaseTime = IdleHungerDecreaseTime;
            }

            // Check if it's time to decrease hunger
            if (_hungerTimer >= currentHungerDecreaseTime)
            {
                Hunger = Math.Max(0, Hunger - 1);
                _hungerTimer = 0f; // Reset timer after decreasing hunger
            }


            // Then in the Ant class Update method, modify the stamina handling section
            // First, check if enough stamina for actions
            bool canSprint = Stamina > SprintStaminaThreshold;
            bool canJump = Stamina > JumpStaminaThreshold;

            // Then update the sprinting logic
            if (isActivelyMoving && isSprinting && canSprint)
            {
                // Reset stamina regeneration timer when sprinting
                _staminaRegenTimer = 0f;

                // Calculate stamina cost (only when sprinting)
                float staminaCost = StaminaSprintCost * deltaTime;

                // Apply stamina cost
                Stamina = Math.Max(0, Stamina - (int)Math.Ceiling(staminaCost));

                // Adjust movement speed based on stamina level
                if (Stamina < LowStaminaThreshold)
                {
                    MovementSpeed = EntityStatus.Agility * LowStaminaSpeedMultiplier;
                }

                // Update ability flags after stamina cost is applied
                canSprint = Stamina > SprintStaminaThreshold;
                canJump = Stamina > JumpStaminaThreshold;
            }
            else
            {
                // Handle stamina regeneration when not sprinting
                _staminaRegenTimer += deltaTime;

                if (_staminaRegenTimer >= StaminaRegenDelay)
                {
                    // Regenerate stamina
                    float regenAmount = StaminaRegenRate * deltaTime;
                    Stamina = Math.Min(MaxStamina, Stamina + (int)Math.Ceiling(regenAmount));

                    // Reset movement speed to normal when rested
                    if (Stamina >= LowStaminaThreshold)
                    {
                        MovementSpeed = EntityStatus.Agility;
                    }

                    // Update ability flags after regeneration
                    canSprint = Stamina > SprintStaminaThreshold;
                    canJump = Stamina > JumpStaminaThreshold;
                }
            }

            // Apply hunger effects (could add additional logic here)
            if (Hunger <= 0)
            {
                // Maybe apply health damage when starving
                HitPoints = Math.Max(0, HitPoints - 1);
            }
        }

        // Override the HandleInput method to add stamina restrictions
        public override void HandleInput(KeyboardState keyboardState, GamePadState gamePadState, GameTime gameTime)
        {
            GamePadState = GamePad.GetState(0);
            KeyboardState = Keyboard.GetState();

            // Check if we have enough stamina to sprint or jump
            bool canSprint = Stamina > SprintStaminaThreshold;
            bool canJump = Stamina > JumpStaminaThreshold;

            // Check for jump before movement utilities to handle the sound
            bool wasJumping = _isJumping;
            bool wasOnGround = _isOnGround;

            // Use MovementUtilities to handle player input and physics
            MovementUtilities.HandlePlayerInputWithStaminaRestrictions(
                ref _position,
                ref _velocity,
                ref _isOnGround,
                ref _isJumping,
                ref _jumpDiagonalPosY,
                ref _isCenterOnDiagonal,
                ref _soundTimer,
                ref _movementSpeed,
                ref _animationSpeed,
                KeyboardState,
                PreviousKeyboardState,
                GameState.CurrentMap,
                CollisionBounding,
                TextureInfo,
                EntityStatus,
                ref Flipped,
                Friction,
                Gravity,
                JumpStrength,
                PlayMoveSound,
                canSprint,
                canJump,
                gameTime);

            // Play jump sound if we just started jumping
            if (!wasJumping && _isJumping && wasOnGround)
            {
                JumpSound?.Play();
            }

            // Update the previous keyboard state
            PreviousKeyboardState = KeyboardState;
        }

        /// <summary>
        /// Consumes food to replenish hunger
        /// </summary>
        /// <param name="nutritionalValue">The amount of hunger to restore</param>
        public void Eat(int nutritionalValue)
        {
            Hunger = Math.Min(MaxHunger, Hunger + nutritionalValue);
        }

        /// <summary>
        /// Restores stamina, such as from consuming special items
        /// </summary>
        /// <param name="amount">The amount of stamina to restore</param>
        public void RestoreStamina(int amount)
        {
            Stamina = Math.Min(MaxStamina, Stamina + amount);
        }
    }
}