using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace Superorganism.ContentPipeline
{
    [ContentProcessor(DisplayName = "Tiled Map Processor")]
    public class TiledMapProcessor : ContentProcessor<BasicTilemapContent, BasicTilemapContent>
    {
        public override BasicTilemapContent Process(BasicTilemapContent map, ContentProcessorContext context)
        {
            // Process each tileset
            foreach (Tileset tileset in map.Tilesets.Values)
            {
                // Convert relative path to absolute
                string absolutePath = Path.GetDirectoryName(map.MapFilename);
                string imagePath = Path.Combine(absolutePath!, tileset.ImagePath.Replace('/', Path.DirectorySeparatorChar));

                // Build and load the tileset texture
                tileset.TileTexture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                    new ExternalReference<TextureContent>(imagePath),
                    "TextureProcessor"
                );
            }

            return map;
        }
    }
}
