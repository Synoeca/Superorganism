using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism.Tiles
{

    /// <summary>
    /// Define content paths for loading tilesets
    /// </summary>
    public static class ContentPaths
    {
        public const string TilesetDir = "Tileset";
        public const string MapDir = "Maps";

        public static string GetTilesetPath(string filename)
        {
            return Path.Combine(TilesetDir, filename);
        }

        public static string GetMapPath(string filename)
        {
            return Path.Combine(TilesetDir, MapDir, filename);
        }

        public static string GetTexturePath(string imageSource, string mapFilename)
        {
            // Case 1: "../../tileset.png" -> look in Content root
            if (imageSource.Contains(".."))
            {
                return Path.GetFileNameWithoutExtension(imageSource).Replace('\\', '/');
            }

            // Case 2: "tileset64.png" -> look in the same directory as the map
            // For files in the same directory as the map, keep them in Tileset/Maps
            return Path.Combine(TilesetDir, MapDir, Path.GetFileNameWithoutExtension(imageSource))
                .Replace('\\', '/');
        }
    }
}