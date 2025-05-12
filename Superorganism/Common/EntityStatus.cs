using System;

namespace Superorganism.Common
{
    /// <summary>
    /// Defines the attributes and status values for entities in the game system
    /// Including resource management for stamina and hunger
    /// </summary>
    public class EntityStatus
    {
        // Core attributes
        /// <summary>
        /// Physical power attribute
        /// </summary>
        public float Strength { get; set; } = 1;

        /// <summary>
        /// Sensory and awareness attribute
        /// </summary>
        public float Perception { get; set; } = 1;

        private float _endurance = 1;

        /// <summary>
        /// Stamina and resilience attribute
        /// </summary>
        public float Endurance
        {
            get => _endurance;
            set => _endurance = value;
        }

        /// <summary>
        /// Social and persuasion attribute
        /// </summary>
        public float Charisma { get; set; } = 1;

        /// <summary>
        /// Mental and reasoning attribute
        /// </summary>
        public float Intelligence { get; set; } = 1;

        /// <summary>
        /// Speed and dexterity attribute
        /// </summary>
        public float Agility { get; set; } = 1;

        /// <summary>
        /// Fortune and chance attribute
        /// </summary>
        public float Luck { get; set; } = 1;

        /// <summary>
        /// Current health value
        /// </summary>
        public float HitPoints { get; set; } = 100;

        /// <summary>
        /// Maximum health value
        /// </summary>
        public float MaxHitPoints { get; set; } = 100;

        // Resource management properties
        /// <summary>
        /// Current stamina level
        /// </summary>
        public float Stamina { get; set; } = 100f;

        /// <summary>
        /// Maximum stamina capacity
        /// </summary>
        public float MaxStamina { get; set; } = 100f;

        /// <summary>
        /// Current hunger level
        /// </summary>
        public float Hunger { get; set; } = 100f;

        /// <summary>
        /// Maximum hunger capacity
        /// </summary>
        public float MaxHunger { get; set; } = 100f;

        /// <summary>
        /// Timer for Hunger regeneration
        /// </summary>
        public float HungerTimer { get; set; } = 0f;

        // Resource management timing configuration
        /// <summary>
        /// Time before stamina begins regenerating (seconds)
        /// </summary>
        public float StaminaRegenDelay { get; set; } = 3.0f;

        /// <summary>
        /// Rate of stamina regeneration per second
        /// </summary>
        public float StaminaRegenRate { get; set; } = 5.0f;

        /// <summary>
        /// Cost of stamina per second while sprinting
        /// </summary>
        public float StaminaSprintCost { get; set; } = 1.5f;

        /// <summary>
        /// The strength of the jump measured as initial upward velocity
        /// Negative values move upward in the coordinate system
        /// Higher absolute values result in higher jumps
        /// </summary>
        public float JumpStrength { get; set; } = -14f;

        /// <summary>
        /// Stamina cost when jumping
        /// </summary>
        public float JumpStaminaCost { get; set; } = 10.0f;

        /// <summary>
        /// Timer for stamina regeneration
        /// </summary>
        public float StaminaRegenTimer { get; set; } = 0f;

        /// <summary>
        /// Time for hunger to decrease by 1 when idle (seconds)
        /// </summary>
        public float IdleHungerDecreaseTime { get; set; } = 180.0f;

        /// <summary>
        /// Time for hunger to decrease by 1 when moving (seconds)
        /// </summary>
        public float MovingHungerDecreaseTime { get; set; } = 30.0f;

        /// <summary>
        /// Time for hunger to decrease by 1 when sprinting (seconds)
        /// </summary>
        public float SprintingHungerDecreaseTime { get; set; } = 10.0f;

        /// <summary>
        /// Minimum stamina required to jump
        /// </summary>
        public const float JumpStaminaThreshold = 10f;

        /// <summary>
        /// Minimum stamina required to sprint (can sprint until 0)
        /// </summary>
        public const float SprintStaminaThreshold = 0f;

        /// <summary>
        /// Stamina threshold below which movement speed is reduced
        /// </summary>
        public float LowStaminaThreshold { get; set; } = 15f;

        /// <summary>
        /// Speed multiplier when stamina is below threshold
        /// </summary>
        public float LowStaminaSpeedMultiplier { get; set; } = 0.6f;

        /// <summary>
        /// Hunger rate when idle (hunger lost per second)
        /// </summary>
        public float IdleHungerRate { get; set; } = 1.0f / 180.0f; // 1 hunger per 3 minutes

        /// <summary>
        /// Hunger rate when moving (hunger lost per second)
        /// </summary>
        public float MovingHungerRate { get; set; } = 1.0f / 30.0f; // 1 hunger per 30 seconds

        /// <summary>
        /// Hunger rate when sprinting (hunger lost per second)
        /// </summary>
        public float SprintingHungerRate { get; set; } = 1.0f / 10.0f; // 1 hunger per 10 seconds

        /// <summary>
        /// Updates resource management (stamina, hunger) based on entity status and activity level
        /// Handles stamina consumption during sprinting, regeneration during rest, 
        /// hunger decrease over time, and applies status effects like health damage when starving
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update in seconds</param>
        /// <param name="isActivelyMoving">Whether the entity is actively moving</param>
        /// <param name="isSprinting">Whether the entity is attempting to sprint</param>
        /// <param name="isJumping">Whether the entity just started jumping</param>
        /// <returns>The current movement speed based on agility and stamina levels</returns>
        public float UpdateResourceManagement(float deltaTime, bool isActivelyMoving, bool isSprinting, bool isJumping)
        {
            // Handle hunger decrease based on activity level
            float hungerRate;
            if (isActivelyMoving && isSprinting)
            {
                hungerRate = SprintingHungerRate;
            }
            else if (isActivelyMoving)
            {
                hungerRate = MovingHungerRate;
            }
            else
            {
                hungerRate = IdleHungerRate;
            }

            // Apply hunger decrease
            Hunger = Math.Max(0, Hunger - (hungerRate * deltaTime));

            // Calculate movement speed multiplier
            float speedMultiplier = Agility;

            // Handle stamina consumption and regeneration
            if (isActivelyMoving && isSprinting && Stamina > SprintStaminaThreshold)
            {
                // Reset stamina regeneration timer when actively sprinting
                StaminaRegenTimer = 0f;

                // Calculate stamina cost (only when actually sprinting)
                float staminaCost = StaminaSprintCost * deltaTime;

                // Apply stamina cost
                Stamina = Math.Max(0, Stamina - staminaCost);

                // Adjust movement speed based on stamina level
                if (Stamina < LowStaminaThreshold)
                {
                    speedMultiplier = Agility * LowStaminaSpeedMultiplier;
                }
            }
            else
            {
                // Always regenerate stamina when not actively sprinting
                StaminaRegenTimer += deltaTime;

                if (StaminaRegenTimer >= StaminaRegenDelay)
                {
                    // Regenerate stamina
                    float regenAmount = StaminaRegenRate * deltaTime;
                    Stamina = Math.Min(MaxStamina, Stamina + regenAmount);

                    // Reset movement speed to normal when rested
                    if (Stamina >= LowStaminaThreshold)
                    {
                        speedMultiplier = Agility;
                    }
                }
            }

            // Handle jumping stamina cost
            if (isJumping && Stamina >= JumpStaminaCost)
            {
                Stamina = Math.Max(0, Stamina - JumpStaminaCost);
                StaminaRegenTimer = 0f; // Reset stamina regeneration timer
            }

            // Apply hunger effects
            if (Hunger <= 0)
            {
                // Apply health damage when starving
                float damagePerSecond = 1.0f; // Adjust this as needed
                HitPoints = Math.Max(0, HitPoints - (damagePerSecond * deltaTime));
            }

            return speedMultiplier;
        }

        /// <summary>
        /// Consumes food to replenish hunger
        /// </summary>
        /// <param name="nutritionalValue">The amount of hunger to restore</param>
        public void Eat(float nutritionalValue)
        {
            Hunger = Math.Min(MaxHunger, Hunger + nutritionalValue);
        }

        /// <summary>
        /// Restores stamina from items or abilities
        /// </summary>
        /// <param name="amount">The amount of stamina to restore</param>
        public void RestoreStamina(float amount)
        {
            Stamina = Math.Min(MaxStamina, Stamina + amount);
        }

        /// <summary>
        /// Gets the appropriate movement speed based on current stamina levels
        /// </summary>
        /// <param name="isSprinting">Whether entity is attempting to sprint</param>
        /// <returns>The movement speed modifier to apply</returns>
        public float GetMovementSpeedModifier(bool isSprinting)
        {
            if (isSprinting && Stamina > 0)
            {
                if (Stamina < LowStaminaThreshold)
                {
                    // Low stamina sprint speed
                    return LowStaminaSpeedMultiplier;
                }

                // Normal sprint speed
                return 2.0f;
            }

            // Normal walk speed
            return 1.0f;
        }
    }
}