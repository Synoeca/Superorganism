using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Superorganism.Entities;

namespace Superorganism.Core.Managers
{
    public class GameStateInfo
    {
        /// <summary>
        /// Current game's entities
        /// </summary>
        public List<Entity> Entities { get; set; }

        /// <summary>
        /// Overall game time through the save
        /// </summary>
        public TimeSpan GameProgressTime { get; set; }
    }
}
