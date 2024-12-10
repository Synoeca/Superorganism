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
    }
}
