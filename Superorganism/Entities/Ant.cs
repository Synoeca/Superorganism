using Microsoft.Xna.Framework;
using Superorganism.Common;

namespace Superorganism.Entities
{
    /// <summary>
    /// Represents a controllable ant entity in the game
    /// </summary>
    public sealed class Ant : ControllableEntity
    {
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

    }
}
