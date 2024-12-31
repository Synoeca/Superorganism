using System.Diagnostics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using static ContentPipeline.BasicLayer;

namespace ContentPipeline
{
    public static class TexturePathUtil
    {
        public static string GetTexturePath(string imageSource, string mapFilename, ContentProcessorContext context)
        {
            context.Logger.LogMessage("\n=== GetTexturePath Debug Info ===");
            context.Logger.LogMessage($"Image Source: {imageSource}");
            context.Logger.LogMessage($"Map Filename: {mapFilename}");
            context.Logger.LogMessage($"Current Directory: {Directory.GetCurrentDirectory()}");

            string contentRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(mapFilename)));
            context.Logger.LogMessage($"Content Root: {contentRoot}");

            string contentDir = Path.GetDirectoryName(mapFilename);
            context.Logger.LogMessage($"Content Dir: {contentDir}");

            string normalizedImageSource = imageSource;
            context.Logger.LogMessage($"Initial Image Source: {normalizedImageSource}");

            if (imageSource.StartsWith("../"))
            {
                contentDir = Path.GetDirectoryName(contentDir);
                normalizedImageSource = imageSource[3..];
                context.Logger.LogMessage($"Adjusted Content Dir: {contentDir}");
                context.Logger.LogMessage($"Normalized Image Source: {normalizedImageSource}");
            }

            string absolutePath = Path.GetFullPath(Path.Combine(contentDir, normalizedImageSource));
            context.Logger.LogMessage($"Absolute Path: {absolutePath}");
            context.Logger.LogMessage($"File Exists?: {File.Exists(absolutePath)}");

            string relativePath = Path.GetRelativePath(contentRoot, absolutePath);
            context.Logger.LogMessage($"Relative Path: {relativePath}");

            string processedPath = relativePath.Replace('\\', '/');
            context.Logger.LogMessage($"Final Processed Path: {processedPath}\n");

            return processedPath;
        }

        public static string FileName;

        public static string TexturePath;
    }

    [ContentProcessor(DisplayName = "Tilemap Processor")]
    public class TilemapProcessor : ContentProcessor<BasicMap, BasicMap>
    {
        public override BasicMap Process(BasicMap input, ContentProcessorContext context)
        {
            context.Logger.LogMessage("\n=== Starting Tilemap Processing ===");
            context.Logger.LogMessage($"Input Map: {input.Width}x{input.Height}, Tile Size: {input.TileWidth}x{input.TileHeight}");
            context.Logger.LogMessage($"Contains: Tilesets({input.Tilesets.Count}), Layers({input.Layers.Count}), ObjectGroups({input.ObjectGroups.Count})");

            TexturePathUtil.FileName = input.Filename;
            context.Logger.LogMessage($"input.Filename: {input.Filename}");
            context.Logger.LogMessage($"TexturePathUtil.FileName: {TexturePathUtil.FileName}");

            context.Logger.LogMessage($"Before try");
            try
            {
                context.Logger.LogMessage($"Inside try");
                BasicMap processedMap = new()
                {
                    Width = input.Width,
                    Height = input.Height,
                    TileWidth = input.TileWidth,
                    TileHeight = input.TileHeight,
                    Properties = new Dictionary<string, string>(input.Properties)
                };


                context.Logger.LogMessage($"Before process tileset");

                foreach (KeyValuePair<string, BasicTileset> pair in input.Tilesets)
                {
                    string texturePath = TexturePathUtil.GetTexturePath(pair.Value.Image, input.Filename, context);
                    TexturePathUtil.TexturePath = texturePath;
                    context.Logger.LogMessage($"TexturePathUtil.TexturePath: {TexturePathUtil.TexturePath}");

                    // Load the texture separately
                    Texture2DContent texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                        new ExternalReference<TextureContent>(texturePath),
                        "TextureProcessor"
                    );

                    // Process the tileset
                    BasicTileset tileset = pair.Value;
                    tileset.Texture = texture;
                }

                context.Logger.LogMessage($"After Tileset Foreach");

                foreach (KeyValuePair<string, BasicLayer> pair in input.Layers)
                {
                    context.Logger.LogMessage($"Processing layer: {pair.Key}");
                    BasicLayer layer = pair.Value;
                    // Process the layer if needed
                }

                foreach (KeyValuePair<string, BasicObjectGroup> pair in input.ObjectGroups)
                {
                    context.Logger.LogMessage($"Processing object group: {pair.Key}");
                    BasicObjectGroup group = pair.Value;
                    // Process the object group if needed
                }

                context.Logger.LogMessage("=== Map Processing Complete ===");
                return processedMap;
            }
            catch (Exception ex)
            {
                context.Logger.LogImportantMessage($"Error processing map: {ex.Message}");
                throw;
            }
        }

        private void LogWarning(ContentProcessorContext context, string message, params object[] messageArgs)
        {
            context.Logger.LogWarning("", new ContentIdentity(), message, messageArgs);
        }
    }

    [ContentProcessor(DisplayName = "Tileset Processor")]
    public class TilesetProcessor : ContentProcessor<BasicTileset, BasicTileset>
    {
        public override BasicTileset Process(BasicTileset input, ContentProcessorContext context)
        {
            context.Logger.LogMessage("\n=== Processing Tileset ===");
            context.Logger.LogMessage("Pre-processing Tileset State:");
            context.Logger.LogMessage($"  Name: {input.Name}");
            context.Logger.LogMessage($"  FirstTileId: {input.FirstTileId}");
            context.Logger.LogMessage($"  TileWidth: {input.TileWidth}");
            context.Logger.LogMessage($"  TileHeight: {input.TileHeight}");
            context.Logger.LogMessage($"  Spacing: {input.Spacing}");
            context.Logger.LogMessage($"  Margin: {input.Margin}");
            context.Logger.LogMessage($"  Image Path: {input.Image}");
            context.Logger.LogMessage($"  Properties Count: {input.TileProperties?.Count ?? 0}");

            BasicTileset processed = new()
            {
                Name = input.Name,
                FirstTileId = input.FirstTileId,
                TileWidth = input.TileWidth,
                TileHeight = input.TileHeight,
                Spacing = input.Spacing,
                Margin = input.Margin,
                TileProperties = new Dictionary<int, Dictionary<string, string>>(input.TileProperties ?? new Dictionary<int, Dictionary<string, string>>())
            };

            if (!string.IsNullOrEmpty(input.Image))
            {
                string texturePath = TexturePathUtil.GetTexturePath(input.Image, TexturePathUtil.FileName, context);
                processed.Texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                    new ExternalReference<TextureContent>(texturePath),
                    "TextureProcessor"
                );

                if (processed.Texture?.Mipmaps.Count > 0)
                {
                    processed.TexWidth = processed.Texture.Mipmaps[0].Width;
                    processed.TexHeight = processed.Texture.Mipmaps[0].Height;
                    processed.TileTexture = processed.Texture;
                    context.Logger.LogMessage($"  Width: {processed.TexWidth}");
                    context.Logger.LogMessage($"  Height: {processed.TexHeight}");
                }
            }

            context.Logger.LogMessage("\nPost-processing State:");
            context.Logger.LogMessage($"  FirstTileId: {processed.FirstTileId}");
            context.Logger.LogMessage($"  TileWidth: {processed.TileWidth}");
            context.Logger.LogMessage($"  TileHeight: {processed.TileHeight}");
            context.Logger.LogMessage($"  TexWidth: {processed.TexWidth}");
            context.Logger.LogMessage($"  TexHeight: {processed.TexHeight}");

            return processed;
        }
    }

    [ContentProcessor(DisplayName = "Layer Processor")]
    public class LayerProcessor : ContentProcessor<BasicLayer, BasicLayer>
    {
        public override BasicLayer Process(BasicLayer input, ContentProcessorContext context)
        {
            context.Logger.LogMessage($"\n=== Processing Layer ===");
            context.Logger.LogMessage($"Dimensions: {input.Width}x{input.Height}");
            context.Logger.LogMessage($"Tile Count: {input.Tiles?.Length ?? 0}");

            BasicLayer processed = new()
            {
                Name = input.Name,
                Width = input.Width,
                Height = input.Height,
                Opacity = input.Opacity,
                Tiles = input.Tiles?.ToArray() ?? Array.Empty<int>(),
                FlipAndRotate = input.FlipAndRotate?.ToArray() ?? Array.Empty<byte>(),
                Properties = new Dictionary<string, string>(input.Properties)
            };

            context.Logger.LogMessage($"Layer processed successfully");
            return processed;
        }
    }

    [ContentProcessor(DisplayName = "Object Processor")]
    public class ObjectProcessor : ContentProcessor<BasicObject, BasicObject>
    {
        public override BasicObject Process(BasicObject input, ContentProcessorContext context)
        {
            context.Logger.LogMessage($"Processing object: {input.Name}");
            context.Logger.LogMessage($"Position: ({input.X}, {input.Y})");
            context.Logger.LogMessage($"Size: {input.Width}x{input.Height}");

            BasicObject processed = new()
            {
                Name = input.Name,
                Width = input.Width,
                Height = input.Height,
                X = input.X,
                Y = input.Y,
                Properties = new Dictionary<string, string>(input.Properties)
            };

            if (!string.IsNullOrEmpty(input.Image))
            {
                string texturePath = TexturePathUtil.GetTexturePath(input.Image, input.Name, context);
                processed.TileTexture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                    new ExternalReference<TextureContent>(input.Image),
                    "TextureProcessor"
                );

                if (processed.TileTexture?.Mipmaps.Count > 0)
                {
                    processed.TexWidth = processed.TileTexture.Mipmaps[0].Width;
                    processed.TexHeight = processed.TileTexture.Mipmaps[0].Height;
                }
            }

            return processed;
        }
    }

    [ContentProcessor(DisplayName = "Object Group Processor")]
    public class ObjectGroupProcessor : ContentProcessor<BasicObjectGroup, BasicObjectGroup>
    {
        public override BasicObjectGroup Process(BasicObjectGroup input, ContentProcessorContext context)
        {
            context.Logger.LogMessage($"\n=== Processing Object Group ===");
            context.Logger.LogMessage($"Objects Count: {input.Objects.Count}");

            BasicObjectGroup processed = new()
            {
                Name = input.Name,
                Width = input.Width,
                Height = input.Height,
                X = input.X,
                Y = input.Y,
                _opacity = input._opacity,
                Objects = new Dictionary<string, BasicObject>(input.Objects),
                Properties = new Dictionary<string, string>(input.Properties)
            };

            return processed;
        }
    }
}