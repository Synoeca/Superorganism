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
        public float Health { get; set; }  // Keep as int for backward compatibility
        public Strategy CurrentStrategy { get; set; }  // For AntEnemy
        public List<StrategyHistoryEntry> StrategyHistory { get; set; }  // For AntEnemy
        public EntityStatusData Status { get; set; }  // EntityStatus data
    }

    public class StrategyHistoryEntry
    {
        public Strategy Strategy { get; set; }
        public double StartTime { get; set; }
        public double LastActionTime { get; set; }
    }

    public class EntityStatusData
    {
        // Core attributes
        public float Strength { get; set; }
        public float Perception { get; set; }
        public float Endurance { get; set; }
        public float Charisma { get; set; }
        public float Intelligence { get; set; }
        public float Agility { get; set; }
        public float Luck { get; set; }

        // Resource tracking - EntityStatus uses float for HitPoints
        public float HitPoints { get; set; }
        public float MaxHitPoints { get; set; }
        public float Stamina { get; set; }
        public float MaxStamina { get; set; }
        public float Hunger { get; set; }
        public float MaxHunger { get; set; }

        // Resource management timing
        public float StaminaRegenDelay { get; set; }
        public float StaminaRegenRate { get; set; }
        public float StaminaSprintCost { get; set; }
        public float StaminaRegenTimer { get; set; }
        public float IdleHungerDecreaseTime { get; set; }
        public float MovingHungerDecreaseTime { get; set; }
        public float SprintingHungerDecreaseTime { get; set; }
        public float LowStaminaThreshold { get; set; }
        public float LowStaminaSpeedMultiplier { get; set; }
        public float HungerTimer { get; set; }
    }
}