using Superorganism.Core.Managers;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Entities;
using Superorganism.Common;
using Superorganism.Core.Timing;
using Superorganism.Core.InventorySystem;
using System.Collections.Generic;
using Superorganism.Tiles;
using Superorganism.AI;
using Superorganism.Collisions;

namespace Superorganism.Core.SaveLoadSystem
{
    /// <summary>
    /// Class responsible for loading game state from disk
    /// </summary>
    public static class GameStateLoader
    {
        /// <summary>
        /// JSON serializer options
        /// </summary>
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            Converters = { new Vector2Converter() }
        };

        /// <summary>
        /// Content manager for loading textures
        /// </summary>
        private static ContentManager _contentManager;

        /// <summary>
        /// Game map containing tilesets
        /// </summary>
        private static TiledMap _map;

        /// <summary>
        /// Loads a game state from a save file
        /// </summary>
        /// <param name="saveFile">The save file to load</param>
        /// <param name="contentManager">Content manager for loading resources</param>
        /// <param name="map">The game map containing tilesets</param>
        /// <returns>The loaded game state and map file name</returns>
        public static (GameStateInfo state, string mapFileName) LoadGameState(string saveFile, ContentManager contentManager, TiledMap map)
        {
            _contentManager = contentManager;
            _map = map;

            string savePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Superorganism", "Saves", saveFile);
            try
            {
                string jsonContent = File.ReadAllText(savePath);
                GameStateContent savedState = JsonSerializer.Deserialize<GameStateContent>(jsonContent, SerializerOptions);

                // Restore the GameTimer
                GameTimer.Load(savedState.GameplayTime);

                return (RestoreGameState(savedState), savedState.MapFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading game state: {ex.Message}");
                // Reset the timer for a new game
                GameTimer.Reset();
                return (CreateNewGameState(), "Tileset/Maps/TestMapRev5");
            }
        }

        /// <summary>
        /// Restores a game state from serialized data
        /// </summary>
        /// <param name="savedState">The serialized game state</param>
        /// <returns>The restored game state</returns>
        public static GameStateInfo RestoreGameState(GameStateContent savedState)
        {
            GameStateInfo gameState = new()
            {
                GameProgressTime = TimeSpan.FromSeconds(savedState.GameProgressTime.TotalSeconds),
                Entities = []
            };

            foreach (EntityData entityData in savedState.Entities)
            {
                Entity entity = CreateEntity(entityData);
                if (entity != null)
                {
                    gameState.Entities.Add(entity);
                }
            }

            return gameState;
        }

        /// <summary>
        /// Creates an entity from serialized data
        /// </summary>
        /// <param name="data">The serialized entity data</param>
        /// <returns>The created entity</returns>
        private static Entity CreateEntity(EntityData data)
        {
            switch (data.Type)
            {
                case "Ant":
                    Ant ant = new()
                    {
                        Position = data.Position,
                        IsControlled = data.IsControlled
                    };
                    // Apply EntityStatus first
                    if (data.Status != null)
                    {
                        ant.EntityStatus = DeserializeEntityStatus(data.Status);
                    }
                    // Then set HitPoints through the property (which updates EntityStatus.HitPoints)
                    ant.EntityStatus.HitPoints = data.Health;
                    // Load inventory data if available
                    if (data.Inventory != null)
                    {
                        ant.Inventory = DeserializeInventory(data.Inventory);
                    }
                    return ant;

                case "AntEnemy":
                    AntEnemy enemy = new()
                    {
                        Position = data.Position,
                        LastKnownTargetPosition = data.LastKnownTargetPosition,
                        Strategy = data.CurrentStrategy,
                        StrategyHistory = data.StrategyHistory?.Select(sh =>
                            (sh.Strategy, sh.StartTime, sh.LastActionTime)).ToList() ?? []
                    };
                    // Apply EntityStatus first
                    if (data.Status != null)
                    {
                        enemy.EntityStatus = DeserializeEntityStatus(data.Status);
                    }
                    // Then set Health through EntityStatus.HitPoints
                    enemy.EntityStatus.HitPoints = data.Health;
                    // Load inventory data if available
                    if (data.Inventory != null)
                    {
                        enemy.Inventory = DeserializeInventory(data.Inventory);
                    }
                    return enemy;

                case "Crop":
                    Crop crop = new()
                    {
                        Position = data.Position
                    };
                    // Apply EntityStatus first
                    if (data.Status != null)
                    {
                        //crop.EntityStatus = DeserializeEntityStatus(data.Status);
                    }
                    // Load inventory data if available
                    if (data.Inventory != null)
                    {
                        crop.Inventory = DeserializeInventory(data.Inventory);
                    }
                    return crop;

                case "Fly":
                    Fly fly = new()
                    {
                        Position = data.Position
                    };
                    // Apply EntityStatus first
                    if (data.Status != null)
                    {
                        fly.EntityStatus = DeserializeEntityStatus(data.Status);
                    }
                    // Then set Health through EntityStatus.HitPoints
                    fly.EntityStatus.HitPoints = data.Health;
                    // Load inventory data if available
                    if (data.Inventory != null)
                    {
                        fly.Inventory = DeserializeInventory(data.Inventory);
                    }
                    return fly;

                case "DroppedItem":
                    DroppedItem droppedItem = new()
                    {
                        // Set position
                        Position = data.Position
                    };

                    // Apply entity status if available
                    if (data.Status != null)
                    {
                        droppedItem.EntityStatus = DeserializeEntityStatus(data.Status);
                    }

                    // Load item details from inventory data if available
                    if (data.Inventory != null && data.Inventory.Items.Count > 0)
                    {
                        InventoryItemData itemData = data.Inventory.Items[0];

                        // Set basic properties
                        droppedItem.ItemName = itemData.Name;
                        droppedItem.ItemDescription = itemData.Description;
                        droppedItem.IsSpriteAtlas = itemData.IsSpriteAtlas;
                        droppedItem.IsFromTileset = itemData.IsFromTileset;
                        droppedItem.TilesetIndex = itemData.TilesetIndex;
                        droppedItem.TileIndex = itemData.TileIndex;

                        // Set source rectangle
                        droppedItem.SourceRectangle = itemData.SourceRectangle;

                        // Load texture - either from content manager or tileset
                        if (itemData.IsFromTileset && _map != null &&
                            itemData.TilesetIndex >= 0 && itemData.TileIndex >= 0)
                        {
                            try
                            {
                                // Get the tileset texture - access public tileset properties
                                if (_map.Tilesets.Count > itemData.TilesetIndex)
                                {
                                    Tileset tileset = _map.Tilesets.Values[itemData.TilesetIndex];
                                    // Use the TileTexture property which should be public instead of Texture
                                    droppedItem.Texture = tileset.TileTexture;

                                    // Calculate source rectangle from tile index if not set
                                    if (droppedItem.SourceRectangle == Rectangle.Empty)
                                    {
                                        // Use tileset's method to calculate the rect
                                        tileset.MapTileToRect(itemData.TileIndex, ref droppedItem._sourceRectangle);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Warning: Could not load tileset texture for DroppedItem. Error: {ex.Message}");
                            }
                        }
                        else if (!string.IsNullOrEmpty(itemData.TextureName))
                        {
                            try
                            {
                                droppedItem.Texture = _contentManager.Load<Texture2D>(itemData.TextureName);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Warning: Could not load texture '{itemData.TextureName}' for DroppedItem. Error: {ex.Message}");
                            }
                        }

                        // Set up texture info
                        int width = droppedItem.SourceRectangle != Rectangle.Empty ?
                            droppedItem.SourceRectangle.Width :
                            droppedItem.Texture?.Width ?? 32;

                        int height = droppedItem.SourceRectangle != Rectangle.Empty ?
                            droppedItem.SourceRectangle.Height :
                            droppedItem.Texture?.Height ?? 32;

                        // Create a new TextureInfo instance with correct values
                        droppedItem.TextureInfo = new TextureInfo
                        {
                            TextureWidth = droppedItem.Texture?.Width ?? width,
                            TextureHeight = droppedItem.Texture?.Height ?? height,
                            NumOfSpriteCols = 1,
                            NumOfSpriteRows = 1,
                            SizeScale = itemData.Scale,
                            Center = new Vector2(width / 2.0f, height / 2.0f)
                        };

                        // Create collision bounds slightly smaller than the item for better collision
                        float boundingWidth = width * itemData.Scale * 0.8f;
                        float boundingHeight = height * itemData.Scale * 0.8f;

                        BoundingRectangle boundingRect = new(
                            droppedItem.Position.X - (boundingWidth / 2),
                            droppedItem.Position.Y - (boundingHeight / 2),
                            boundingWidth,
                            boundingHeight
                        );
                        droppedItem.CollisionBounding = boundingRect;

                        // Set color
                        droppedItem.Color = Color.White;
                    }

                    // Initialize the state for loading
                    droppedItem.InitializeFromSavedState();

                    // Set collected status to false since we're only saving uncollected items
                    droppedItem.Collected = false;

                    // Return null so it doesn't also get added to gameState.Entities
                    return droppedItem;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Creates a new game state for new games
        /// </summary>
        /// <returns>A new game state</returns>
        private static GameStateInfo CreateNewGameState()
        {
            // Reset the timer for a new game
            GameTimer.Reset();

            Ant newAnt = new()
            {
                Position = new Vector2(100, 100),
                EntityStatus =
                {
                    HitPoints = 100
                },
                Inventory = [] // Initialize with empty inventory
            };
            return new GameStateInfo
            {
                Entities = [newAnt],
                GameProgressTime = TimeSpan.Zero
            };
        }

        /// <summary>
        /// Deserializes entity status data
        /// </summary>
        /// <param name="statusData">The serialized entity status data</param>
        /// <returns>The deserialized entity status</returns>
        private static EntityStatus DeserializeEntityStatus(EntityStatusData statusData)
        {
            if (statusData == null) return new EntityStatus();

            return new EntityStatus
            {
                // Core attributes
                Strength = statusData.Strength,
                Perception = statusData.Perception,
                Endurance = statusData.Endurance,
                Charisma = statusData.Charisma,
                Intelligence = statusData.Intelligence,
                Agility = statusData.Agility,
                Luck = statusData.Luck,

                // Resource tracking
                HitPoints = statusData.HitPoints,
                MaxHitPoints = statusData.MaxHitPoints,
                Stamina = statusData.Stamina,
                MaxStamina = statusData.MaxStamina,
                Hunger = statusData.Hunger,
                MaxHunger = statusData.MaxHunger,

                // Resource management timing
                StaminaRegenDelay = statusData.StaminaRegenDelay,
                StaminaRegenRate = statusData.StaminaRegenRate,
                StaminaSprintCost = statusData.StaminaSprintCost,
                StaminaRegenTimer = statusData.StaminaRegenTimer,
                IdleHungerDecreaseTime = statusData.IdleHungerDecreaseTime,
                MovingHungerDecreaseTime = statusData.MovingHungerDecreaseTime,
                SprintingHungerDecreaseTime = statusData.SprintingHungerDecreaseTime,
                LowStaminaThreshold = statusData.LowStaminaThreshold,
                LowStaminaSpeedMultiplier = statusData.LowStaminaSpeedMultiplier,
                HungerTimer = statusData.HungerTimer
            };
        }

        /// <summary>
        /// Deserializes inventory data
        /// </summary>
        /// <param name="inventoryData">The serialized inventory data</param>
        /// <returns>The deserialized inventory</returns>
        private static Inventory DeserializeInventory(InventoryData inventoryData)
        {
            if (inventoryData == null) return [];

            Inventory inventory = [];

            foreach (InventoryItemData itemData in inventoryData.Items)
            {
                InventoryItem item = null;

                // Check if this is a tileset item
                if (itemData.IsFromTileset && itemData.TilesetIndex >= 0 && itemData.TileIndex >= 0 && _map != null)
                {
                    try
                    {
                        // Create item from tileset using the SortedList and indices
                        item = InventoryItem.CreateFromTileset(
                            itemData.Name,
                            itemData.Quantity,
                            itemData.Description,
                            _map.Tilesets,
                            itemData.TilesetIndex,
                            itemData.TileIndex
                        );

                        Console.WriteLine($"Recreated tileset item: {itemData.Name} from tileset {itemData.TilesetIndex}, tile {itemData.TileIndex}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error recreating tileset item: {ex.Message}");
                    }
                }

                // If not a tileset item or if tileset creation failed, fall back to regular item creation
                if (item == null)
                {
                    Texture2D texture = null;

                    // Try to load texture if texture name is available
                    if (!string.IsNullOrEmpty(itemData.TextureName))
                    {
                        try
                        {
                            // Load texture directly from content manager
                            texture = _contentManager.Load<Texture2D>(itemData.TextureName);
                            Console.WriteLine($"Loaded texture: {itemData.TextureName}");
                        }
                        catch (Exception ex)
                        {
                            // Texture couldn't be loaded
                            Console.WriteLine($"Warning: Could not load texture '{itemData.TextureName}'. Error: {ex.Message}");
                        }
                    }

                    // Create the inventory item
                    item = new InventoryItem(
                        itemData.Name,
                        itemData.Quantity,
                        itemData.Description,
                        texture,
                        itemData.SourceRectangle,
                        itemData.IsSpriteAtlas,
                        itemData.Scale
                    );
                }

                // Add the item to inventory
                inventory.Add(item);
            }

            return inventory;
        }
    }
}