using Superorganism.Core.Managers;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Superorganism.Entities;
using Superorganism.Common;
using Superorganism.Core.Timing;

namespace Superorganism.Core.SaveLoadSystem
{
    public static class GameStateLoader
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            Converters = { new Vector2Converter() }
        };

        public static (GameStateInfo state, string mapFileName) LoadGameState(string saveFile)
        {
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
            catch (Exception)
            {
                // Reset the timer for a new game
                GameTimer.Reset();
                return (CreateNewGameState(), "Tileset/Maps/TestMapRev5");
            }
        }

        public static GameStateInfo RestoreGameState(GameStateContent savedState)
        {
            GameStateInfo gameState = new GameStateInfo
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

        private static Entity CreateEntity(EntityData data)
        {
            switch (data.Type)
            {
                case "Ant":
                    Ant ant = new Ant
                    {
                        Position = data.Position
                    };
                    // Apply EntityStatus first
                    if (data.Status != null)
                    {
                        ant.EntityStatus = DeserializeEntityStatus(data.Status);
                    }
                    // Then set HitPoints through the property (which updates EntityStatus.HitPoints)
                    ant.EntityStatus.HitPoints = data.Health;
                    return ant;

                case "AntEnemy":
                    AntEnemy enemy = new()
                    {
                        Position = data.Position,
                        Strategy = data.CurrentStrategy,
                        StrategyHistory = data.StrategyHistory?.Select(sh =>
                            (sh.Strategy, sh.StartTime, sh.LastActionTime)).ToList() ?? new()
                    };
                    // Apply EntityStatus first
                    if (data.Status != null)
                    {
                        enemy.EntityStatus = DeserializeEntityStatus(data.Status);
                    }
                    // Then set Health through EntityStatus.HitPoints
                    enemy.EntityStatus.HitPoints = data.Health;
                    return enemy;

                case "Crop":
                    Crop crop = new Crop
                    {
                        Position = data.Position
                    };
                    // Apply EntityStatus first
                    if (data.Status != null)
                    {
                        //crop.EntityStatus = DeserializeEntityStatus(data.Status);
                    }
                    // Then set Health through EntityStatus.HitPoints
                    //crop.EntityStatus.HitPoints = data.Health;
                    return crop;

                case "Fly":
                    Fly fly = new Fly
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
                    return fly;

                default:
                    return null;
            }
        }

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
                }
            };
            return new GameStateInfo
            {
                Entities = [newAnt],
                GameProgressTime = TimeSpan.Zero
            };
        }

        // Helper method to deserialize EntityStatus
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
    }
}