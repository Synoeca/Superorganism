using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using static Assimp.Metadata;
using Superorganism.AI;
using Superorganism.Core.Managers;

namespace Superorganism.Core.SaveLoadSystem
{
    public class GameStateContent
    {
        public List<EntityData> Entities { get; set; }
        public bool IsGameOver { get; set; }
        public bool IsGameWon { get; set; }
        public TimeSpan GameProgressTime { get; set; }
        public string SaveFilename { get; set; }
    }

    public class EntityData
    {
        public string Type { get; set; }  // "Ant", "AntEnemy", "Crop", "Fly"
        public Vector2 Position { get; set; }
        public int Health { get; set; }
        public Strategy CurrentStrategy { get; set; }  // For AntEnemy
        public List<StrategyHistoryEntry> StrategyHistory { get; set; }  // For AntEnemy
    }

    public class StrategyHistoryEntry
    {
        public Strategy Strategy { get; set; }
        public double StartTime { get; set; }
        public double LastActionTime { get; set; }
    }


}
