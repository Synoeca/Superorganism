using System.Diagnostics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

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
    public class TilemapProcessor : ContentProcessor<BasicMap, dynamic>
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
                    Tilesets = new Dictionary<string, BasicMap.BasicTileset>(input.Tilesets),
                    Layers = new Dictionary<string, BasicMap.BasicLayer>(input.Layers),
                    ObjectGroups = new Dictionary<string, BasicMap.BasicObjectGroup>(input.ObjectGroups),
                    Properties = new Dictionary<string, string>(input.Properties)
                };

                context.Logger.LogMessage($"Processed Map Width: {processedMap.Width}");
                context.Logger.LogMessage($"Processed Map Height: {processedMap.Height}");
                context.Logger.LogMessage($"Processed Map TileWidth: {processedMap.TileWidth}");
                context.Logger.LogMessage($"Processed Map TileHeight: {processedMap.TileHeight}");

                context.Logger.LogMessage($"Processed Map Tilesets Count: {processedMap.Tilesets.Count}");
                foreach (KeyValuePair<string, BasicMap.BasicTileset> tileset in processedMap.Tilesets)
                {
                    context.Logger.LogMessage($"Tileset Key: {tileset.Key}, Name: {tileset.Value.Name}");
                    context.Logger.LogMessage($"  FirstTileId: {tileset.Value.FirstTileId}");
                    context.Logger.LogMessage($"  TileWidth: {tileset.Value.TileWidth}");
                    context.Logger.LogMessage($"  TileHeight: {tileset.Value.TileHeight}");
                    context.Logger.LogMessage($"  Spacing: {tileset.Value.Spacing}");
                    context.Logger.LogMessage($"  Margin: {tileset.Value.Margin}");
                    context.Logger.LogMessage($"  Image Path: {tileset.Value.Image}");
                    context.Logger.LogMessage($"  Properties Count: {tileset.Value.TileProperties?.Count ?? 0}");
                    if (tileset.Value.Texture != null)
                    {
                        context.Logger.LogMessage($"  Texture Width: {tileset.Value.TexWidth}");
                        context.Logger.LogMessage($"  Texture Height: {tileset.Value.TexHeight}");
                    }
                }

                context.Logger.LogMessage($"Processed Map Layers Count: {processedMap.Layers.Count}");
                foreach (KeyValuePair<string, BasicMap.BasicLayer> layer in processedMap.Layers)
                {
                    context.Logger.LogMessage($"Layer Key: {layer.Key}, Name: {layer.Value.Name}");
                    context.Logger.LogMessage($"  Dimensions: {layer.Value.Width}x{layer.Value.Height}");
                    context.Logger.LogMessage($"  Opacity: {layer.Value.Opacity}");
                    context.Logger.LogMessage($"  Tiles Count: {layer.Value.Tiles?.Length ?? 0}");
                    context.Logger.LogMessage($"  FlipAndRotate Count: {layer.Value.FlipAndRotate?.Length ?? 0}");
                    context.Logger.LogMessage($"  Properties Count: {layer.Value.Properties.Count}");
                }

                context.Logger.LogMessage($"Processed Map ObjectGroups Count: {processedMap.ObjectGroups.Count}");
                foreach (KeyValuePair<string, BasicMap.BasicObjectGroup> objectGroup in processedMap.ObjectGroups)
                {
                    context.Logger.LogMessage($"ObjectGroup Key: {objectGroup.Key}, Name: {objectGroup.Value.Name}");
                    context.Logger.LogMessage($"  Width: {objectGroup.Value.Width}");
                    context.Logger.LogMessage($"  Height: {objectGroup.Value.Height}");
                    context.Logger.LogMessage($"  X: {objectGroup.Value.X}");
                    context.Logger.LogMessage($"  Y: {objectGroup.Value.Y}");
                    context.Logger.LogMessage($"  Opacity: {objectGroup.Value._opacity}");
                    context.Logger.LogMessage($"  Objects Count: {objectGroup.Value.Objects.Count}");
                    context.Logger.LogMessage($"  Properties Count: {objectGroup.Value.Properties.Count}");
                }

                context.Logger.LogMessage($"Processed Map Properties Count: {processedMap.Properties.Count}");
                foreach (KeyValuePair<string, string> property in processedMap.Properties)
                {
                    context.Logger.LogMessage($"Property Key: {property.Key}, Value: {property.Value}");
                }

                context.Logger.LogMessage($"Before process tileset");

                //foreach (var pair in input.Tilesets)
                //{
                //    string texturePath = TexturePathUtil.GetTexturePath(pair.Value.Image, input.Filename, context);
                //    context.BuildAsset<string, BasicTileset>(
                //        new ExternalReference<string>(texturePath),
                //        "TilesetProcessor",
                //        null,
                //        "TilemapImporter",
                //input.Filename
                //    );
                //}

                //foreach (var pair in input.Layers)
                //{
                //    context.BuildAsset<string, BasicLayer>(
                //        new ExternalReference<string>(pair.Value.Name),
                //        Path.Combine("Tileset/Maps", pair.Key),
                //        null,
                //        "TilemapImporter",
                //        "LayerProcessor"
                //    );
                //}

                //foreach (var pair in input.ObjectGroups)
                //{
                //    context.BuildAsset<string, BasicObjectGroup>(
                //        new ExternalReference<string>(pair.Value.Name),
                //        Path.Combine("Tileset/Maps", pair.Key),
                //        null,
                //        "TilemapImporter",
                //        "ObjectGroupProcessor"
                //    );
                //}

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
    public class TilesetProcessor : ContentProcessor<BasicMap.BasicTileset, BasicMap.BasicTileset>
    {
        public override BasicMap.BasicTileset Process(BasicMap.BasicTileset input, ContentProcessorContext context)
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

            BasicMap.BasicTileset processed = new()
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
    public class LayerProcessor : ContentProcessor<BasicMap.BasicLayer, BasicMap.BasicLayer>
    {
        public override BasicMap.BasicLayer Process(BasicMap.BasicLayer input, ContentProcessorContext context)
        {
            context.Logger.LogMessage($"\n=== Processing Layer ===");
            context.Logger.LogMessage($"Dimensions: {input.Width}x{input.Height}");
            context.Logger.LogMessage($"Tile Count: {input.Tiles?.Length ?? 0}");

            BasicMap.BasicLayer processed = new()
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
    public class ObjectProcessor : ContentProcessor<BasicMap.BasicObject, BasicMap.BasicObject>
    {
        public override BasicMap.BasicObject Process(BasicMap.BasicObject input, ContentProcessorContext context)
        {
            context.Logger.LogMessage($"Processing object: {input.Name}");
            context.Logger.LogMessage($"Position: ({input.X}, {input.Y})");
            context.Logger.LogMessage($"Size: {input.Width}x{input.Height}");

            BasicMap.BasicObject processed = new()
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
    public class ObjectGroupProcessor : ContentProcessor<BasicMap.BasicObjectGroup, BasicMap.BasicObjectGroup>
    {
        public override BasicMap.BasicObjectGroup Process(BasicMap.BasicObjectGroup input, ContentProcessorContext context)
        {
            context.Logger.LogMessage($"\n=== Processing Object Group ===");
            context.Logger.LogMessage($"Objects Count: {input.Objects.Count}");

            BasicMap.BasicObjectGroup processed = new()
            {
                Name = input.Name,
                Width = input.Width,
                Height = input.Height,
                X = input.X,
                Y = input.Y,
                _opacity = input._opacity,
                Objects = new Dictionary<string, BasicMap.BasicObject>(input.Objects),
                Properties = new Dictionary<string, string>(input.Properties)
            };

            return processed;
        }
    }
}