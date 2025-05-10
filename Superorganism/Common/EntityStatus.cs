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

        public float JumpStrength { get; set; } = -14f;

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
        public int Stamina { get; set; } = 100;

        /// <summary>
        /// Maximum stamina capacity
        /// </summary>
        public int MaxStamina { get; set; } = 100;

        /// <summary>
        /// Current hunger level
        /// </summary>
        public int Hunger { get; set; } = 100;

        /// <summary>
        /// Maximum hunger capacity
        /// </summary>
        public int MaxHunger { get; set; } = 100;

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

        public const float JumpStaminaThreshold = 10f;
        public const float SprintStaminaThreshold = 0f;

        /// <summary>
        /// Stamina threshold below which movement speed is reduced
        /// </summary>
        public float LowStaminaThreshold { get; set; } = 15;

        /// <summary>
        /// Speed multiplier when stamina is below threshold
        /// </summary>
        public float LowStaminaSpeedMultiplier { get; set; } = 0.6f;

        // Internal timers for resource management
        private float _staminaRegenTimer = 0f;
        private float _hungerTimer = 0f;

        /// <summary>
        /// Updates resource management (stamina, hunger) based on entity status and activity level
        /// Handles stamina consumption during sprinting, regeneration during rest, 
        /// hunger decrease over time, and applies status effects like health damage when starving
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update in seconds</param>
        /// <param name="isActivelyMoving">Whether the entity is actively moving</param>
        /// <param name="isSprinting">Whether the entity is attempting to sprint</param>
        /// <returns>The current movement speed based on agility and stamina levels</returns>
        public float UpdateResourceManagement(float deltaTime, bool isActivelyMoving, bool isSprinting)
        {
            // Handle hunger decrease based on activity level
            HungerTimer += deltaTime;

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
            if (HungerTimer >= currentHungerDecreaseTime)
            {
                Hunger = System.Math.Max(0, Hunger - 1);
                HungerTimer = 0f; // Reset timer after decreasing hunger
            }

            // First, check if enough stamina for actions
            bool canSprint = Stamina > SprintStaminaThreshold;
            bool canJump = Stamina > JumpStaminaThreshold;

            // Calculate movement speed multiplier
            float speedMultiplier = Agility;

            // Then update the sprinting logic
            if (isActivelyMoving && isSprinting && canSprint)
            {
                // Reset stamina regeneration timer when sprinting
                StaminaRegenTimer = 0f;

                // Calculate stamina cost (only when sprinting)
                float staminaCost = StaminaSprintCost * deltaTime;

                // Apply stamina cost
                Stamina = System.Math.Max(0, Stamina - (int)System.Math.Ceiling(staminaCost));

                // Adjust movement speed based on stamina level
                if (Stamina < LowStaminaThreshold)
                {
                    speedMultiplier = Agility * LowStaminaSpeedMultiplier;
                }

                // Update ability flags after stamina cost is applied
                canSprint = Stamina > SprintStaminaThreshold;
                canJump = Stamina > JumpStaminaThreshold;
            }
            else
            {
                // Handle stamina regeneration when not sprinting
                StaminaRegenTimer += deltaTime;

                if (StaminaRegenTimer >= StaminaRegenDelay)
                {
                    // Regenerate stamina
                    float regenAmount = StaminaRegenRate * deltaTime;
                    Stamina = System.Math.Min(MaxStamina, Stamina + (int)System.Math.Ceiling(regenAmount));

                    // Reset movement speed to normal when rested
                    if (Stamina >= LowStaminaThreshold)
                    {
                        speedMultiplier = Agility;
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
                HitPoints = System.Math.Max(0, HitPoints - 1);
            }

            return speedMultiplier;
        }

        /// <summary>
        /// Consumes food to replenish hunger
        /// </summary>
        /// <param name="nutritionalValue">The amount of hunger to restore</param>
        public void Eat(int nutritionalValue)
        {
            Hunger = System.Math.Min(MaxHunger, Hunger + nutritionalValue);
        }

        /// <summary>
        /// Restores stamina from items or abilities
        /// </summary>
        /// <param name="amount">The amount of stamina to restore</param>
        public void RestoreStamina(int amount)
        {
            Stamina = System.Math.Min(MaxStamina, Stamina + amount);
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
                else
                {
                    // Normal sprint speed
                    return 2.0f; // Default sprint multiplier
                }
            }
            else
            {
                // Normal walk speed
                return 1.0f;
            }
        }
    }
}