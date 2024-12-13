using System.Diagnostics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using static ContentPipeline.BasicLayer;

namespace ContentPipeline
{
    [ContentProcessor(DisplayName = "Tilemap Processor")]
    public class TilemapProcessor : ContentProcessor<BasicMap, BasicMap>
    {
        private string GetTexturePath(string imageSource, string mapFilename, ContentProcessorContext context)
        {
            context.Logger.LogMessage($"GetTexturePath: Starting with imageSource={imageSource}, mapFilename={mapFilename}");
            string contentRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(mapFilename)));
            context.Logger.LogMessage($"GetTexturePath: Content root={contentRoot}");
            string contentDir = Path.GetDirectoryName(mapFilename);
            context.Logger.LogMessage($"GetTexturePath: Content directory={contentDir}");

            string normalizedImageSource = imageSource;
            if (imageSource.StartsWith("../"))
            {
                contentDir = Path.GetDirectoryName(contentDir);
                normalizedImageSource = imageSource[3..];
                context.Logger.LogMessage($"GetTexturePath: Normalized image source to: {normalizedImageSource}");
            }

            string absolutePath = Path.GetFullPath(Path.Combine(contentDir, normalizedImageSource));
            context.Logger.LogMessage($"GetTexturePath: Absolute path={absolutePath}");
            string relativePath = Path.GetRelativePath(contentRoot, absolutePath);
            string processedPath = relativePath.Replace('\\', '/');
            context.Logger.LogMessage($"GetTexturePath: Final processed path={processedPath}");

            return processedPath;
        }

        public override BasicMap Process(BasicMap input, ContentProcessorContext context)
        {
            context.Logger.LogMessage("\n=== Starting Tilemap Processing ===");
            context.Logger.LogMessage($"Input Map State:");
            context.Logger.LogMessage($"  Filename: {input.Filename}");
            context.Logger.LogMessage($"  Dimensions: {input.Width}x{input.Height}");
            context.Logger.LogMessage($"  Tile Dimensions: {input.TileWidth}x{input.TileHeight}");
            context.Logger.LogMessage($"  Number of Tilesets: {input.Tilesets.Count}");
            context.Logger.LogMessage($"  Number of Layers: {input.Layers.Count}");
            context.Logger.LogMessage($"  Number of Object Groups: {input.ObjectGroups.Count}");

            try
            {
                // Process each tileset
                foreach (KeyValuePair<string, BasicTileset> tilesetEntry in input.Tilesets)
                {
                    context.Logger.LogMessage($"\n=== Processing Tileset: {tilesetEntry.Key} ===");
                    BasicTileset tileset = tilesetEntry.Value;

                    // Log pre-processing state
                    context.Logger.LogMessage("Pre-processing Tileset State:");
                    context.Logger.LogMessage($"  Name: {tileset.Name}");
                    context.Logger.LogMessage($"  FirstTileId: {tileset.FirstTileId}");
                    context.Logger.LogMessage($"  TileWidth: {tileset.TileWidth}");
                    context.Logger.LogMessage($"  TileHeight: {tileset.TileHeight}");
                    context.Logger.LogMessage($"  Spacing: {tileset.Spacing}");
                    context.Logger.LogMessage($"  Margin: {tileset.Margin}");
                    context.Logger.LogMessage($"  Image Path: {tileset.Image}");
                    context.Logger.LogMessage($"  Properties Count: {tileset.TileProperties?.Count ?? 0}");

                    if (!string.IsNullOrEmpty(tileset.Image))
                    {
                        string texturePath = GetTexturePath(tileset.Image, input.Filename, context);
                        context.Logger.LogMessage($"Processing texture: {texturePath}");

                        try
                        {
                            tileset.Texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                                new ExternalReference<TextureContent>(texturePath),
                                "TextureProcessor"
                            );

                            if (tileset.Texture?.Mipmaps.Count > 0)
                            {
                                tileset.TexWidth = tileset.Texture.Mipmaps[0].Width;
                                tileset.TexHeight = tileset.Texture.Mipmaps[0].Height;
                                context.Logger.LogMessage($"Texture processed successfully:");
                                context.Logger.LogMessage($"  Width: {tileset.TexWidth}");
                                context.Logger.LogMessage($"  Height: {tileset.TexHeight}");
                            }
                            else
                            {
                                context.Logger.LogWarning("", new ContentIdentity(), "No mipmaps found in processed texture!");
                            }
                        }
                        catch (Exception ex)
                        {
                            context.Logger.LogImportantMessage($"Error processing texture: {ex.Message}");
                            throw;
                        }
                    }

                    // Log post-processing state
                    context.Logger.LogMessage("\nPost-processing Tileset State:");
                    context.Logger.LogMessage($"  Name: {tileset.Name}");
                    context.Logger.LogMessage($"  FirstTileId: {tileset.FirstTileId}");
                    context.Logger.LogMessage($"  TileWidth: {tileset.TileWidth}");
                    context.Logger.LogMessage($"  TileHeight: {tileset.TileHeight}");
                    context.Logger.LogMessage($"  Spacing: {tileset.Spacing}");
                    context.Logger.LogMessage($"  Margin: {tileset.Margin}");
                    context.Logger.LogMessage($"  TexWidth: {tileset.TexWidth}");
                    context.Logger.LogMessage($"  TexHeight: {tileset.TexHeight}");
                    context.Logger.LogMessage($"  Has Texture: {tileset.Texture != null}");
                }

                // Process layers with detailed logging
                foreach (KeyValuePair<string, BasicLayer> layerEntry in input.Layers)
                {
                    context.Logger.LogMessage($"\n=== Processing Layer: {layerEntry.Key} ===");
                    BasicLayer layer = layerEntry.Value;
                    context.Logger.LogMessage($"Layer State:");
                    context.Logger.LogMessage($"  Dimensions: {layer.Width}x{layer.Height}");
                    context.Logger.LogMessage($"  Tile Count: {layer.Tiles?.Length ?? 0}");
                    context.Logger.LogMessage($"  Properties: {layer.Properties.Count}");

                    if (layer.Tiles != null)
                    {
                        context.Logger.LogMessage($"  First few tile indices: {string.Join(", ", layer.Tiles.Take(5))}...");
                    }
                }

                // Process object groups with detailed logging
                foreach (BasicObjectGroup group in input.ObjectGroups.Values)
                {
                    context.Logger.LogMessage($"\n=== Processing Object Group: {group.Name} ===");
                    foreach (BasicObject obj in group.Objects.Values)
                    {
                        ProcessObject(obj, input.Filename, context);
                    }
                }

                // Final validation with detailed state
                context.Logger.LogMessage("\n=== Final Map State ===");

                // Basic map properties
                context.Logger.LogMessage($"Map Properties:");
                context.Logger.LogMessage($"  Dimensions: {input.Width}x{input.Height}");
                context.Logger.LogMessage($"  Tile Dimensions: {input.TileWidth}x{input.TileHeight}");
                context.Logger.LogMessage($"  Custom Properties Count: {input.Properties.Count}");
                foreach (KeyValuePair<string, string> prop in input.Properties)
                {
                    context.Logger.LogMessage($"    {prop.Key}: {prop.Value}");
                }

                // Tilesets
                context.Logger.LogMessage($"\nTilesets ({input.Tilesets.Count}):");
                foreach (BasicTileset tileset in input.Tilesets.Values)
                {
                    context.Logger.LogMessage($"  Tileset '{tileset.Name}' final state:");
                    context.Logger.LogMessage($"    FirstTileId: {tileset.FirstTileId}");
                    context.Logger.LogMessage($"    TileWidth: {tileset.TileWidth}");
                    context.Logger.LogMessage($"    TileHeight: {tileset.TileHeight}");
                    context.Logger.LogMessage($"    Spacing: {tileset.Spacing}");
                    context.Logger.LogMessage($"    Margin: {tileset.Margin}");
                    context.Logger.LogMessage($"    TexWidth: {tileset.TexWidth}");
                    context.Logger.LogMessage($"    TexHeight: {tileset.TexHeight}");
                    context.Logger.LogMessage($"    Properties Count: {tileset.TileProperties?.Count ?? 0}");
                }

                // Layers
                context.Logger.LogMessage($"\nLayers ({input.Layers.Count}):");
                foreach (KeyValuePair<string, BasicLayer> layer in input.Layers)
                {
                    context.Logger.LogMessage($"  Layer '{layer.Key}' final state:");
                    context.Logger.LogMessage($"    Dimensions: {layer.Value.Width}x{layer.Value.Height}");
                    context.Logger.LogMessage($"    Tiles Count: {layer.Value.Tiles?.Length ?? 0}");
                    context.Logger.LogMessage($"    Properties Count: {layer.Value.Properties.Count}");
                }

                // Object Groups
                context.Logger.LogMessage($"\nObject Groups ({input.ObjectGroups.Count}):");
                foreach (KeyValuePair<string, BasicObjectGroup> group in input.ObjectGroups)
                {
                    context.Logger.LogMessage($"  Group '{group.Key}' final state:");
                    context.Logger.LogMessage($"    Objects Count: {group.Value.Objects.Count}");
                    context.Logger.LogMessage($"    Properties Count: {group.Value.Properties.Count}");
                }

                context.Logger.LogMessage("\n=== Building Tile Info Caches ===");
                foreach (var layer in input.Layers.Values)
                {
                    context.Logger.LogMessage($"Building cache for layer: {layer.Name}");
                    layer.BuildTileInfoCache(input.Tilesets.Values, context);  // Pass the context
                    context.Logger.LogMessage($"  Cache size: {layer.TileInfoCache?.Length ?? 0} entries");
                }

                context.Logger.LogMessage("\n=== Processing Complete ===");



                // Add this before return input; in the Process method
                try
                {
                    context.Logger.LogMessage("\n=== RAW INPUT STATE BEFORE RETURN ===");

                    // Log map properties
                    context.Logger.LogMessage($"Map raw values:");
                    context.Logger.LogMessage($"Width: {input.Width} (Type: {input.Width.GetType()})");
                    context.Logger.LogMessage($"Height: {input.Height} (Type: {input.Height.GetType()})");
                    context.Logger.LogMessage($"TileWidth: {input.TileWidth} (Type: {input.TileWidth.GetType()})");
                    context.Logger.LogMessage($"TileHeight: {input.TileHeight} (Type: {input.TileHeight.GetType()})");

                    // Log each tileset's raw values
                    foreach (var tileset in input.Tilesets.Values)
                    {
                        context.Logger.LogMessage($"\nTileset raw values:");
                        context.Logger.LogMessage($"Name: {tileset.Name}");
                        context.Logger.LogMessage($"FirstTileId: {tileset.FirstTileId} (Type: {tileset.FirstTileId.GetType()})");
                        context.Logger.LogMessage($"TileWidth: {tileset.TileWidth} (Type: {tileset.TileWidth.GetType()})");
                        context.Logger.LogMessage($"TileHeight: {tileset.TileHeight} (Type: {tileset.TileHeight.GetType()})");
                        context.Logger.LogMessage($"Spacing: {tileset.Spacing} (Type: {tileset.Spacing.GetType()})");
                        context.Logger.LogMessage($"Margin: {tileset.Margin} (Type: {tileset.Margin.GetType()})");
                        context.Logger.LogMessage($"Has Texture: {tileset.Texture != null}");
                        context.Logger.LogMessage($"TexWidth: {tileset.TexWidth} (Type: {tileset.TexWidth.GetType()})");
                        context.Logger.LogMessage($"TexHeight: {tileset.TexHeight} (Type: {tileset.TexHeight.GetType()})");
                    }

                    // Log each layer's raw values
                    foreach (var layer in input.Layers.Values)
                    {
                        context.Logger.LogMessage($"\nLayer raw values:");
                        context.Logger.LogMessage($"Name: {layer.Name}");
                        context.Logger.LogMessage($"Width: {layer.Width} (Type: {layer.Width.GetType()})");
                        context.Logger.LogMessage($"Height: {layer.Height} (Type: {layer.Height.GetType()})");
                        context.Logger.LogMessage($"Opacity: {layer.Opacity} (Type: {layer.Opacity.GetType()})");
                        context.Logger.LogMessage($"Tiles Count: {layer.Tiles?.Length ?? 0}");
                        context.Logger.LogMessage($"TileInfoCache Count: {layer.TileInfoCache?.Length ?? 0}");
                    }

                    context.Logger.LogMessage("\n=== END RAW INPUT STATE ===");
                }
                catch (Exception ex)
                {
                    context.Logger.LogImportantMessage($"Error logging raw state: {ex.Message}");
                }

                return input;

                return input;
            }
            catch (Exception ex)
            {
                context.Logger.LogImportantMessage($"Fatal error during processing: {ex.Message}");
                context.Logger.LogImportantMessage($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void ProcessObject(BasicObject obj, string mapFilename, ContentProcessorContext context)
        {
            context.Logger.LogMessage($"Processing object: {obj.Name}");
            context.Logger.LogMessage($"  Position: ({obj.X}, {obj.Y})");
            context.Logger.LogMessage($"  Size: {obj.Width}x{obj.Height}");

            if (!string.IsNullOrEmpty(obj.Image))
            {
                string texturePath = GetTexturePath(obj.Image, mapFilename, context);
                context.Logger.LogMessage($"  Processing object texture: {texturePath}");

                try
                {
                    obj.TileTexture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                        new ExternalReference<TextureContent>(texturePath),
                        "TextureProcessor"
                    );

                    if (obj.TileTexture?.Mipmaps.Count > 0)
                    {
                        obj.TexWidth = obj.TileTexture.Mipmaps[0].Width;
                        obj.TexHeight = obj.TileTexture.Mipmaps[0].Height;
                        context.Logger.LogMessage($"  Texture processed - Size: {obj.TexWidth}x{obj.TexHeight}");
                    }
                }
                catch (Exception ex)
                {
                    context.Logger.LogImportantMessage($"  Failed to process object texture: {ex.Message}");
                }
            }
        }

        private void LogWarning(ContentProcessorContext context, string message, params object[] messageArgs)
        {
            context.Logger.LogWarning("", new ContentIdentity(), message, messageArgs);
        }


    }
}