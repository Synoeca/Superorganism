using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Superorganism.Core.SaveLoadSystem
{
    public static class MapFileCreator
    {
        private static readonly string MapContentPath = Path.Combine(
            Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName,
            "Content", "Tileset", "Maps");

        private static readonly string ContentMgcbPath = Path.Combine(
            Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName,
            "Content", "Content.mgcb");

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

                //UpdateContentMgcb($"Tileset/Maps/{newMapName}.tmx");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating map file: {ex.Message}");
                throw;
            }
        }

        private static void UpdateContentMgcb(string mapPath)
        {
            try
            {
                // Read existing content
                string[] lines = File.ReadAllLines(ContentMgcbPath);

                // Find the Content section
                int contentIndex = Array.FindIndex(lines, line => line.Trim() == "#---------------------------------- Content ---------------------------------#");

                if (contentIndex == -1)
                {
                    throw new Exception("Content section not found in Content.mgcb");
                }

                // Create the new map entry
                StringBuilder newEntry = new StringBuilder();
                newEntry.AppendLine($"#begin {mapPath}");
                newEntry.AppendLine("/importer:TiledMapEngineContentImporter");
                newEntry.AppendLine("/processor:TiledMapEngineContentProcessor");
                newEntry.AppendLine($"/build:{mapPath}");
                newEntry.AppendLine();

                // Insert the new entry after the Content section
                List<string> updatedLines = new List<string>();
                updatedLines.AddRange(lines.Take(contentIndex + 1));
                updatedLines.Add(newEntry.ToString());
                updatedLines.AddRange(lines.Skip(contentIndex + 1));

                // Write back to the file
                File.WriteAllLines(ContentMgcbPath, updatedLines);

                Console.WriteLine($"Successfully updated Content.mgcb with new map: {mapPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating Content.mgcb: {ex.Message}");
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