using System;
using System.IO;
using System.Text.Json;

namespace Superorganism.Core.SaveLoadSystem
{
    public static class SaveSystem
    {
        private const string SaveDirectory = "Saves";
        private const string SaveExtension = ".sav";

        public static void SaveGame(GameSaveData saveData, string saveName)
        {
            string directory = Path.Combine(AppContext.BaseDirectory, SaveDirectory);
            Directory.CreateDirectory(directory);

            string filePath = Path.Combine(directory, saveName + SaveExtension);
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };

            string jsonString = JsonSerializer.Serialize(saveData, options);
            File.WriteAllText(filePath, jsonString);
        }

        public static GameSaveData LoadGame(string saveName)
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, SaveDirectory, saveName + SaveExtension);
            if (!File.Exists(filePath)) return null;

            string jsonString = File.ReadAllText(filePath);
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IncludeFields = true
            };

            return JsonSerializer.Deserialize<GameSaveData>(jsonString, options);
        }

        public static bool DoesSaveExist(string saveName)
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, SaveDirectory, saveName + SaveExtension);
            return File.Exists(filePath);
        }

        public static void DeleteSave(string saveName)
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, SaveDirectory, saveName + SaveExtension);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}