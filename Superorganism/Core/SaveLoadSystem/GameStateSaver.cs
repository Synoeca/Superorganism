using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Superorganism.Core.Managers;
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

        public static void SaveGameState(GameStateInfo gameState, string mapFileName, string saveFileName = null)
        {
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

            // Generate the map file name by combining the base map name with the save number
            string baseMapName = Path.GetFileNameWithoutExtension(mapFileName);
            string newMapFileName = $"{baseMapName}_Save{saveNumber}";

            MapFileCreator.CreateMapFileForSave(baseMapName, newMapFileName);

            List<EntityData> entityDataList = [];

            // Save player (Ant)
            Ant player = gameState.Entities.OfType<Ant>().FirstOrDefault();
            if (player != null)
            {
                entityDataList.Add(new EntityData
                {
                    Type = "Ant",
                    Position = player.Position,
                    Health = player.HitPoints
                });
            }

            // Save enemies (AntEnemy)
            foreach (AntEnemy enemy in gameState.Entities.OfType<AntEnemy>())
            {
                entityDataList.Add(new EntityData
                {
                    Type = "AntEnemy",
                    Position = enemy.Position,
                    //Health = enemy.Health,
                    CurrentStrategy = enemy.Strategy,
                    StrategyHistory = enemy.StrategyHistory.Select(sh => new StrategyHistoryEntry
                    {
                        Strategy = sh.Strategy,
                        StartTime = sh.StartTime,
                        LastActionTime = sh.LastActionTime
                    }).ToList()
                });
            }

            // Save crops
            foreach (Crop crop in gameState.Entities.OfType<Crop>())
            {
                entityDataList.Add(new EntityData
                {
                    Type = "Crop",
                    Position = crop.Position,
                    //Health = crop.Health
                });
            }

            // Save flies
            foreach (Fly fly in gameState.Entities.OfType<Fly>())
            {
                entityDataList.Add(new EntityData
                {
                    Type = "Fly",
                    Position = fly.Position,
                    //Health = fly.Health
                });
            }

            GameStateContent state = new()
            {
                Entities = entityDataList,
                //IsGameOver = !player?.IsAlive ?? true,
                IsGameOver = !(player!.HitPoints  > 0),
                IsGameWon = !gameState.Entities.OfType<Crop>().Any(),
                GameProgressTime = gameState.GameProgressTime,
                SaveFilename = saveFileName,
                MapFileName = newMapFileName
            };

            string savePath = Path.Combine(BaseContentPath, saveFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));

            string jsonContent = JsonSerializer.Serialize(state, SerializerOptions);
            File.WriteAllText(savePath, jsonContent);
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
    }
}