using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Superorganism.Enums;

namespace Superorganism
{
    public class EntityActionInfo
    {
        public Vector2 Velocity { get; set; }

        public Vector2 Acceleration { get; set; }

        public Strategy CurrentStrategy { get; set; }

        public List<Strategy> StrategyHistory { get; set; }

        public Direction Direction { get; set; }
    }
}
