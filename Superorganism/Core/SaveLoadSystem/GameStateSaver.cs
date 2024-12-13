using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ContentPipeline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Superorganism.AI;
using Superorganism.Core.Managers;

namespace Superorganism.Core.SaveLoadSystem
{
    public static class GameStateSaver
    {
        private static readonly string BaseContentPath =
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Content"));

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            Converters = { new Vector2Converter() }
        };

        private static int GetNextSaveNumber()
        {
            string savePath = Path.Combine(BaseContentPath, "Saves");
            if (!Directory.Exists(savePath))
                return 1;

            string[] saveFiles = Directory.GetFiles(savePath, "Save*.sav");
            if (!saveFiles.Any())
                return 1;

            IEnumerable<int> numbers = saveFiles
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Select(name =>
                {
                    if (int.TryParse(name.Replace("Save", ""), out int num))
                        return num;
                    return 0;
                });

            return numbers.Max() + 1;
        }

        public static void SaveGameState(GameStateManager gameState, ContentManager content, string saveFileName = null)
        {
            if (string.IsNullOrEmpty(saveFileName))
            {
                int nextNumber = GetNextSaveNumber();
                saveFileName = $"Save{nextNumber}.sav";
            }

            Vector2[] enemyPositions = gameState.GetEnemyPositions();
            Strategy[] strategies = gameState.GetEnemyStrategies();
            string[] strategyStrings = strategies.Select(s => s.ToString()).ToArray();

            GameStateContent state = new()
            {
                IsGameOver = gameState.IsGameOver,
                IsGameWon = gameState.IsGameWon,
                CropsLeft = gameState.CropsLeft,
                ElapsedTime = gameState.ElapsedTime,
                PlayerPosition = gameState.GetPlayerPosition(),
                PlayerHealth = gameState.GetPlayerHealth(),
                EnemyPositions = enemyPositions,
                EnemyStrategies = strategyStrings,
                SaveFilename = saveFileName
            };

            string savePath = Path.Combine(BaseContentPath, "Saves", saveFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));

            string jsonContent = JsonSerializer.Serialize(state, SerializerOptions);
            File.WriteAllText(savePath, jsonContent);
        }
    }
}