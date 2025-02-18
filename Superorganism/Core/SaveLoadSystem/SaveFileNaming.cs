using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Superorganism.Core.SaveLoadSystem
{
    public static class SaveFileNaming
    {
        private const string DateTimeFormat = "yyyyMMdd_HHmmss";

        // Regular expression to match our datetime format
        private static readonly Regex DateTimePattern = new(@"_\d{8}_\d{6}");

        public static string GetBaseMapName(string fileName)
        {
            // Remove extension first
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

            // Remove any existing datetime patterns
            return DateTimePattern.Replace(nameWithoutExt, "");
        }

        public static string GenerateSaveFileName(string mapName)
        {
            // Get the base name without any datetime
            string baseName = GetBaseMapName(mapName);

            string timestamp = DateTime.Now.ToString(DateTimeFormat);
            return $"{baseName}_{timestamp}.sav";
        }

        public static (string MapName, DateTime SaveTime) ParseSaveFileName(string fileName)
        {
            try
            {
                string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                string[] parts = nameWithoutExt.Split('_');
                if (parts.Length < 3)
                    throw new FormatException("Invalid save file name format");

                string dateTimeStr = $"{parts[^2]}_{parts[^1]}";
                DateTime saveTime = DateTime.ParseExact(dateTimeStr, DateTimeFormat, null);
                string mapName = string.Join("_", parts.Take(parts.Length - 2));
                return (mapName, saveTime);
            }
            catch
            {
                return (fileName, DateTime.MinValue);
            }
        }

        public static string GetDisplayName(string fileName)
        {
            try
            {
                (string mapName, DateTime saveTime) = ParseSaveFileName(fileName);
                return $"{mapName} - {saveTime:MMM dd, yyyy HH:mm}";
            }
            catch
            {
                return fileName;
            }
        }

        public static bool IsValidSaveFileName(string fileName)
        {
            try
            {
                (_, DateTime saveTime) = ParseSaveFileName(fileName);
                return saveTime != DateTime.MinValue;
            }
            catch
            {
                return false;
            }
        }
    }
}