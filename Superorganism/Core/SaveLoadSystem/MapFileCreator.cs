using Superorganism.Core.Managers;
using System;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Superorganism.Tiles;

namespace Superorganism.Core.SaveLoadSystem
{
    public static class MapFileCreator
    {
        // Use only runtime paths in AppData, not Content directory
        private static readonly string SavedMapsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Superorganism", "SavedMaps");

        public static string CreateMapFileForSave(string originalMapName, string newMapName, ContentManager content = null)
        {
            string retMapFileName;
            try
            {
                // Generate new filename using the base map name
                string baseMapName = SaveFileNaming.GetBaseMapName(originalMapName);
                string savFileName = SaveFileNaming.GenerateSaveFileName(baseMapName);
                string newMapFileName = Path.ChangeExtension(savFileName, ".tmx");

                // Use saved maps directory in AppData
                string newMapPath = Path.Combine(SavedMapsPath, newMapFileName);

                // Create directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(newMapPath) ?? throw new InvalidOperationException());

                // Get the current map from GameState
                TiledMap currentMap = GameState.CurrentMap;

                // Construct the original map path
                string originalMapPath = null;

                // First, check if originalMapName looks like a saved map (contains datetime pattern)
                if (SaveFileNaming.IsValidSaveFileName(originalMapName) || originalMapName.Contains("_20"))
                {
                    // This is a saved map, look in SavedMapsPath
                    string tempPath = Path.Combine(SavedMapsPath, $"{originalMapName}.tmx");
                    if (File.Exists(tempPath))
                    {
                        originalMapPath = tempPath;
                        Console.WriteLine($"Found saved map: {originalMapPath}");
                    }
                }

                // If not found as a saved map, try the current map's source path
                if (string.IsNullOrEmpty(originalMapPath) && currentMap.SourcePath != null)
                {
                    originalMapPath = currentMap.SourcePath;
                    Console.WriteLine($"Using current map source: {originalMapPath}");
                }

                // If we don't have a source path, try to construct it from the Content directory
                if (string.IsNullOrEmpty(originalMapPath) && content != null)
                {
                    originalMapPath = Path.Combine(content.RootDirectory, "Tileset", "Maps", $"{originalMapName}.tmx");
                    Console.WriteLine($"Trying content directory: {originalMapPath}");
                }

                // If we still don't have a path, try the runtime directory
                if (string.IsNullOrEmpty(originalMapPath) || !File.Exists(originalMapPath))
                {
                    originalMapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Tileset", "Maps", $"{originalMapName}.tmx");
                    Console.WriteLine($"Trying runtime directory: {originalMapPath}");
                }

                // Final check - if we still can't find the original map, throw an error
                if (string.IsNullOrEmpty(originalMapPath) || !File.Exists(originalMapPath))
                {
                    // Log all the paths we tried
                    Console.WriteLine($"Failed to find original map. Tried:");
                    Console.WriteLine($"  SavedMaps: {Path.Combine(SavedMapsPath, $"{originalMapName}.tmx")}");
                    Console.WriteLine($"  Current map source: {currentMap.SourcePath}");
                    if (content != null)
                        Console.WriteLine($"  Content dir: {Path.Combine(content.RootDirectory, "Tileset", "Maps", $"{originalMapName}.tmx")}");
                    Console.WriteLine($"  Runtime dir: {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Tileset", "Maps", $"{originalMapName}.tmx")}");

                    throw new FileNotFoundException($"Could not find original map: {originalMapName}");
                }

                Console.WriteLine($"Using original map: {originalMapPath}");

                // Use TmxSaver to save the current map state
                TmxSaver.SaveTmxFile(currentMap, originalMapPath, newMapPath);

                retMapFileName = Path.GetFileNameWithoutExtension(newMapFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating map file for save: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
            return retMapFileName;
        }

        // Helper method to get saved maps path
        public static string GetSavedMapsPath()
        {
            return SavedMapsPath;
        }
    }
}