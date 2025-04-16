namespace Superorganism.Common
{
    /// <summary>
    /// Defines the attributes and status values for entities in the game system
    /// </summary>
    public class EntityStatus
    {
        /// <summary>
        /// Physical power attribute
        /// </summary>
        public float Strength { get; set; } = 1;

        /// <summary>
        /// Sensory and awareness attribute
        /// </summary>
        public float Perception { get; set; } = 1;

        /// <summary>
        /// Stamina and resilience attribute
        /// </summary>
        public float Endurance { get; set; } = 1;

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
        public float HitPoints { get; set; } = 1;
    }
}