using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentPipeline
{
    [ContentImporter(".tmx", DisplayName = "BasicTilemapImporter", DefaultProcessor = "BasicTilemapProcessor")]
    public class BasicTilemapImporter
    {
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

        public class TiledMapImporter : ContentImporter<BasicMapContent>
        {
            public override BasicMapContent Import(string filename, ContentImporterContext context)
            {
                return new BasicMapContent();
            }
        }
    }
}
