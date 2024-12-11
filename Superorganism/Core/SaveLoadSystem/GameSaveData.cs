using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Superorganism.Enums;

namespace Superorganism.Core.SaveLoadSystem
{
    public class GameSaveData
    {
        // Player data
        public Vector2 PlayerPosition { get; set; }
        public int PlayerHealth { get; set; }

        // Enemy data
        public Vector2 EnemyPosition { get; set; }
        public string CurrentStrategy { get; set; }

        // Collectibles
        public List<CropData> Crops { get; set; } = new();
        public List<FlyData> Flies { get; set; } = new();

        // Map state
        public string CurrentMapName { get; set; }
    }

    public class CropData
    {
        public Vector2 Position { get; set; }
        public bool Collected { get; set; }
    }

    public class FlyData
    {
        public Vector2 Position { get; set; }
        public bool Destroyed { get; set; }
        public Direction Direction { get; set; }
    }
}