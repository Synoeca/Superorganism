using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Superorganism.AI;

namespace Superorganism.Core.SaveLoadSystem
{
    /// <summary>
    /// 
    /// </summary>
    public class GameStateContent
    {
        /// <summary>
        /// 
        /// </summary>
        public List<EntityData> Entities { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsGameOver { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsGameWon { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan GameProgressTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SaveFilename { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MapFileName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double GameplayTime { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class EntityData
    {
        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsControlled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector2 LastKnownTargetPosition { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Health { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Strategy CurrentStrategy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<StrategyHistoryEntry> StrategyHistory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public EntityStatusData Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public InventoryData Inventory { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class StrategyHistoryEntry
    {
        /// <summary>
        /// 
        /// </summary>
        public Strategy Strategy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double LastActionTime { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class EntityStatusData
    {
        /// <summary>
        /// 
        /// </summary>
        public float Strength { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Perception { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Endurance { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Charisma { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Intelligence { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Agility { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Luck { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float HitPoints { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float MaxHitPoints { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Stamina { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float MaxStamina { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Hunger { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float MaxHunger { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float StaminaRegenDelay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float StaminaRegenRate { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public float StaminaSprintCost { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public float StaminaRegenTimer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float IdleHungerDecreaseTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float MovingHungerDecreaseTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float SprintingHungerDecreaseTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float LowStaminaThreshold { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float LowStaminaSpeedMultiplier { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float HungerTimer { get; set; }
    }

    /// <summary>
    /// Serializable data class for inventory
    /// </summary>
    public class InventoryData
    {
        /// <summary>
        /// List of serialized inventory items
        /// </summary>
        public List<InventoryItemData> Items { get; set; } = [];
    }

    /// <summary>
    /// Serializable data class for inventory items
    /// </summary>
    public class InventoryItemData
    {
        /// <summary>
        /// Name of the item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Quantity of the item
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Description of the item
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Name of the texture for loading
        /// </summary>
        public string TextureName { get; set; }

        /// <summary>
        /// Source rectangle within the texture
        /// </summary>
        public Rectangle SourceRectangle { get; set; }

        /// <summary>
        /// Whether the texture is a sprite atlas
        /// </summary>
        public bool IsSpriteAtlas { get; set; }

        /// <summary>
        /// Scale factor for rendering
        /// </summary>
        public float Scale { get; set; }

        /// <summary>
        /// Whether the item was created from a tileset
        /// </summary>
        public bool IsFromTileset { get; set; }

        /// <summary>
        /// Index of the tileset in the map's tileset collection
        /// </summary>
        public int TilesetIndex { get; set; }

        /// <summary>
        /// Index of the tile in the tileset
        /// </summary>
        public int TileIndex { get; set; }
    }
}