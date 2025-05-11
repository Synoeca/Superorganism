using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework.Content;
using Superorganism.Common;
using Superorganism.Core.Managers;
using Superorganism.Core.Timing;
using Superorganism.Entities;

namespace Superorganism.Core.SaveLoadSystem
{
    public static class GameStateSaver
    {
        private static readonly string BaseContentPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Superorganism", "Saves");

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            Converters = { new Vector2Converter() }
        };

        public static void SaveGameState(GameStateInfo gameState, string mapFileName, ContentManager content, string saveFileName = null)
        {
            try
            {
                Console.WriteLine("Starting save operation...");

                // If no custom file name is provided, fall back to generating a numbered save
                string saveNumber;
                if (string.IsNullOrEmpty(saveFileName))
                {
                    int nextNumber = GetNextSaveNumber();
                    saveNumber = nextNumber.ToString();
                    saveFileName = $"Save{saveNumber}.sav";
                }
                else
                {
                    // Extract save number from filename
                    saveNumber = Path.GetFileNameWithoutExtension(saveFileName).Replace("Save", "");

                    // If the name doesn't already end with ".sav", add the extension
                    if (!saveFileName.EndsWith(".sav", StringComparison.OrdinalIgnoreCase))
                    {
                        saveFileName += ".sav";
                    }
                }

                Console.WriteLine($"Save file name: {saveFileName}");

                // Generate the map file name by combining the base map name with the save number
                string baseMapName = Path.GetFileNameWithoutExtension(mapFileName);
                string newMapFileName = $"{baseMapName}_Save{saveNumber}";

                Console.WriteLine("Creating map file for save...");
                try
                {
                    newMapFileName = MapFileCreator.CreateMapFileForSave(baseMapName, newMapFileName, content);
                    Console.WriteLine($"Map file created: {newMapFileName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating map file: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    throw;
                }

                List<EntityData> entityDataList = [];

                Console.WriteLine("Saving entities...");

                // Save player (Ant)
                Ant player = gameState.Entities.OfType<Ant>().FirstOrDefault();
                if (player != null)
                {
                    entityDataList.Add(new EntityData
                    {
                        Type = "Ant",
                        Position = player.Position,
                        IsControlled = player.IsControlled,
                        Health = player.EntityStatus.HitPoints,
                        Status = SerializeEntityStatus(player.EntityStatus)
                    });
                    Console.WriteLine("Player saved");
                }

                // Save enemies (AntEnemy)
                foreach (AntEnemy enemy in gameState.Entities.OfType<AntEnemy>())
                {
                    entityDataList.Add(new EntityData
                    {
                        Type = "AntEnemy",
                        Position = enemy.Position,
                        Health = enemy.EntityStatus.HitPoints,
                        LastKnownTargetPosition = enemy.LastKnownTargetPosition,
                        CurrentStrategy = enemy.Strategy,
                        StrategyHistory = enemy.StrategyHistory.Select(sh => new StrategyHistoryEntry
                        {
                            Strategy = sh.Strategy,
                            StartTime = sh.StartTime,
                            LastActionTime = sh.LastActionTime
                        }).ToList(),
                        Status = SerializeEntityStatus(enemy.EntityStatus)
                    });
                }
                Console.WriteLine($"Enemies saved: {gameState.Entities.OfType<AntEnemy>().Count()}");

                // Save crops
                foreach (Crop crop in gameState.Entities.OfType<Crop>())
                {
                    entityDataList.Add(new EntityData
                    {
                        Type = "Crop",
                        Position = crop.Position,
                    });
                }
                Console.WriteLine($"Crops saved: {gameState.Entities.OfType<Crop>().Count()}");

                // Save flies
                foreach (Fly fly in gameState.Entities.OfType<Fly>())
                {
                    entityDataList.Add(new EntityData
                    {
                        Type = "Fly",
                        Position = fly.Position,
                        Health = (int)fly.EntityStatus.HitPoints,
                        Status = SerializeEntityStatus(fly.EntityStatus)
                    });
                }
                Console.WriteLine($"Flies saved: {gameState.Entities.OfType<Fly>().Count()}");

                Console.WriteLine("Creating game state content...");
                GameStateContent state = new()
                {
                    Entities = entityDataList,
                    IsGameOver = !(player!.EntityStatus.HitPoints > 0),
                    IsGameWon = !gameState.Entities.OfType<Crop>().Any(),
                    GameProgressTime = gameState.GameProgressTime,
                    SaveFilename = saveFileName,
                    MapFileName = newMapFileName,
                    GameplayTime = GameTimer.TotalGameplayTime
                };

                Console.WriteLine("Creating save directory...");
                string savePath = Path.Combine(BaseContentPath, saveFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));

                Console.WriteLine($"Saving to: {savePath}");
                string jsonContent = JsonSerializer.Serialize(state, SerializerOptions);
                File.WriteAllText(savePath, jsonContent);

                Console.WriteLine("Save completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error during save: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private static int GetNextSaveNumber()
        {
            if (!Directory.Exists(BaseContentPath))
                return 1;

            string[] saveFiles = Directory.GetFiles(BaseContentPath, "Save*.sav");
            if (!saveFiles.Any())
                return 1;

            return saveFiles
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Select(name => int.TryParse(name.Replace("Save", ""), out int num) ? num : 0)
                .Max() + 1;
        }

        // Helper method to serialize EntityStatus
        private static EntityStatusData SerializeEntityStatus(EntityStatus status)
        {
            if (status == null) return null;

            return new EntityStatusData
            {
                // Core attributes
                Strength = status.Strength,
                Perception = status.Perception,
                Endurance = status.Endurance,
                Charisma = status.Charisma,
                Intelligence = status.Intelligence,
                Agility = status.Agility,
                Luck = status.Luck,

                // Resource tracking
                HitPoints = status.HitPoints,
                MaxHitPoints = status.MaxHitPoints,
                Stamina = status.Stamina,
                MaxStamina = status.MaxStamina,
                Hunger = status.Hunger,
                MaxHunger = status.MaxHunger,

                // Resource management timing
                StaminaRegenDelay = status.StaminaRegenDelay,
                StaminaRegenRate = status.StaminaRegenRate,
                StaminaSprintCost = status.StaminaSprintCost,
                StaminaRegenTimer = status.StaminaRegenTimer,
                IdleHungerDecreaseTime = status.IdleHungerDecreaseTime,
                MovingHungerDecreaseTime = status.MovingHungerDecreaseTime,
                SprintingHungerDecreaseTime = status.SprintingHungerDecreaseTime,
                LowStaminaThreshold = status.LowStaminaThreshold,
                LowStaminaSpeedMultiplier = status.LowStaminaSpeedMultiplier,
                HungerTimer = status.HungerTimer
            };
        }
    }
}