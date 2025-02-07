using System;
using System.IO;

namespace Superorganism.Core.SaveLoadSystem
{
    public static class MapFileCreator
    {
        private static readonly string MapContentPath = Path.Combine(
            Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName,
            "Content", "Tileset", "Maps");

        public static void CreateMapFileForSave(string originalMapName, string newMapName)
        {
            // Get the full paths
            string originalMapPath = Path.Combine(MapContentPath, $"{originalMapName}.tmx");
            string newMapPath = Path.Combine(MapContentPath, $"{newMapName}.tmx");

            try
            {
                // Check if original map exists
                if (!File.Exists(originalMapPath))
                {
                    throw new FileNotFoundException($"Original map file not found: {originalMapPath}");
                }

                // Copy the content of the original map to the new map file
                File.Copy(originalMapPath, newMapPath, true); // true to overwrite if exists

                Console.WriteLine($"Successfully created new map file: {newMapPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating map file: {ex.Message}");
                throw;
            }
        }

        // Helper method to verify path
        public static string GetMapPath()
        {
            return MapContentPath;
        }
    }
}