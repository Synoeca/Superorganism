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
        /// <summary>
        /// Initializes a new instance of the <see cref="Ant"/> class with default properties
        /// </summary>
        public Ant()
        {
            IsSpriteAtlas = true;
            HasDirection = false;
            Color = Color.White;
            EntityStatus = new EntityStatus()
                { Agility = 2};
        }
    }
}