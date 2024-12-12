using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.IO;

namespace ContentPipeline
{
    [ContentProcessor(DisplayName = "Tiled Map Processor")]
    public class TiledMapProcessor : ContentProcessor<BasicMap, BasicMap>
    {
        public override BasicMap Process(BasicMap input, ContentProcessorContext context)
        {
            // Process tilesets and their textures
            foreach (var tileset in input.Tilesets.Values)
            {
                if (!string.IsNullOrEmpty(tileset.ImageSource))
                {
                    string texturePath = Path.Combine(
                        Path.GetDirectoryName(input.Filename),
                        tileset.ImageSource
                    );

                    // Use BuildAndLoadAsset for textures
                    tileset.Texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                        new ExternalReference<TextureContent>(texturePath),
                        "TextureProcessor"
                    );
                }
            }

            // Process object textures
            foreach (var objectGroup in input.ObjectGroups.Values)
            {
                foreach (var obj in objectGroup.Objects.Values)
                {
                    if (!string.IsNullOrEmpty(obj.ImageSource))
                    {
                        string texturePath = Path.Combine(
                            Path.GetDirectoryName(input.Filename),
                            obj.ImageSource
                        );

                        obj.Texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                            new ExternalReference<TextureContent>(texturePath),
                            "TextureProcessor"
                        );
                    }
                }
            }

            return input;
        }
    }
}