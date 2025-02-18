using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Superorganism.AI;

namespace Superorganism.Core.SaveLoadSystem
{
    public class GameStateContent
    {
        public List<EntityData> Entities { get; set; }
        public bool IsGameOver { get; set; }
        public bool IsGameWon { get; set; }
        public TimeSpan GameProgressTime { get; set; }
        public string SaveFilename { get; set; }
        public string MapFileName { get; set; }
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
