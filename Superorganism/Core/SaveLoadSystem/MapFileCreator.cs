using System;
using System.IO;

namespace Superorganism.Core.SaveLoadSystem
{
    public static class MapFileCreator
    {
        private static readonly string MapContentPath = Path.Combine(
            Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName,
            "Content", "Tileset", "Maps");

        private static readonly string RuntimeMapPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Content", "Tileset", "Maps");

        public static string CreateMapFileForSave(string originalMapName, string newMapName)
        {
            string retMapFileName = "";
            try
            {
                // Generate new filename using the base map name (without any datetime)
                string baseMapName = SaveFileNaming.GetBaseMapName(originalMapName);
                string savFileName = SaveFileNaming.GenerateSaveFileName(baseMapName);
                string newMapFileName = Path.ChangeExtension(savFileName, ".tmx");

                // Get the full paths
                string originalMapPath = Path.Combine(MapContentPath, $"{originalMapName}.tmx");
                string newContentMapPath = Path.Combine(MapContentPath, newMapFileName);
                string newRuntimeMapPath = Path.Combine(RuntimeMapPath, newMapFileName);

                // Check if original map exists
                if (!File.Exists(originalMapPath))
                {
                    throw new FileNotFoundException($"Original map file not found: {originalMapPath}");
                }

                // Create directories if they don't exist
                Directory.CreateDirectory(Path.GetDirectoryName(newContentMapPath));
                Directory.CreateDirectory(Path.GetDirectoryName(newRuntimeMapPath));

                // Copy to content directory
                File.Copy(originalMapPath, newContentMapPath, true);

                // Also copy to runtime directory
                File.Copy(originalMapPath, newRuntimeMapPath, true);

                retMapFileName = Path.GetFileNameWithoutExtension(newMapFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating map file: {ex.Message}");
                throw;
            }
            return retMapFileName;
        }

        // Helper method to get content path
        public static string GetMapContentPath()
        {
            return MapContentPath;
        }

        // Helper method to get runtime path
        public static string GetRuntimeMapPath()
        {
            return RuntimeMapPath;
        }
    }
}