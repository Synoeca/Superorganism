using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace ContentPipeline
{
    [ContentProcessor(DisplayName = "Tiled Map Processor")]
    public class TiledMapProcessor : ContentProcessor<TiledMapContent, TiledMapContent>
    {
        public override TiledMapContent Process(TiledMapContent input, ContentProcessorContext context)
        {
            // Process tilesets and their textures
            foreach (TilesetContent? tileset in input.Tilesets)
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
            foreach (ObjectGroupContent? objectGroup in input.ObjectGroups)
            {
                foreach (ObjectContent? obj in objectGroup.Objects)
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