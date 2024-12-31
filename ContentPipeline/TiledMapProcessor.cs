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
                BasicMap processedMap = new()
                {
                    Width = input.Width,
                    Height = input.Height,
                    TileWidth = input.TileWidth,
                    TileHeight = input.TileHeight,
                    Tilesets = new Dictionary<string, BasicTileset>(),
                    Layers = new Dictionary<string, BasicLayer>(),
                    ObjectGroups = new Dictionary<string, BasicObjectGroup>(),
                    Properties = new Dictionary<string, string>(input.Properties),
                };

                // Process each tileset
                foreach (KeyValuePair<string, BasicTileset> tilesetEntry in input.Tilesets)
                {
                    context.Logger.LogMessage($"\n=== Processing Tileset: {tilesetEntry.Key} ===");
                    BasicTileset tileset = new()
                    {
                        Name = tilesetEntry.Key,
                        FirstTileId = tilesetEntry.Value.FirstTileId,
                        TileWidth = tilesetEntry.Value.TileWidth,
                        TileHeight = tilesetEntry.Value.TileHeight,
                        Spacing = tilesetEntry.Value.Spacing,
                        Margin = tilesetEntry.Value.Margin,
                        TileProperties = new Dictionary<int, Dictionary<string, string>>(
                            tilesetEntry.Value.TileProperties
                        ),
                        Image = tilesetEntry.Value.Image
                    };

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
                                tileset.TileTexture = tileset.Texture;
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

                    processedMap.Tilesets.Add(tileset.Name, tileset);
                }

                // Process layers with detailed logging
                foreach (KeyValuePair<string, BasicLayer> layerEntry in input.Layers)
                {
                    context.Logger.LogMessage($"\n=== Processing Layer: {layerEntry.Key} ===");
                    BasicLayer layer = layerEntry.Value;

                    BasicLayer processedLayer = new()
                    {
                        Properties = new Dictionary<string, string>(layer.Properties),
                        Name = layer.Name,
                        Width = layer.Width,
                        Height = layer.Height,
                        Opacity = layer.Opacity,
                        Tiles = layer.Tiles?.ToArray() ?? [],
                        FlipAndRotate = layer.FlipAndRotate?.ToArray() ?? [],
                        TileInfoCache = layer.TileInfoCache
 
                    };

                    context.Logger.LogMessage($"Layer State:");
                    context.Logger.LogMessage($"  Dimensions: {processedLayer.Width}x{processedLayer.Height}");
                    context.Logger.LogMessage($"  Tile Count: {processedLayer.Tiles?.Length ?? 0}");
                    context.Logger.LogMessage($"  Properties: {processedLayer.Properties.Count}");

                    if (processedLayer.Tiles != null)
                    {
                        context.Logger.LogMessage($"  First few tile indices: {string.Join(", ", processedLayer.Tiles.Take(5))}...");
                    }

                    processedMap.Layers.Add(layerEntry.Key, processedLayer);
                }

                // Process ObjectGroups
                foreach (KeyValuePair<string, BasicObjectGroup> kvp in input.ObjectGroups)
                {
                    BasicObjectGroup group = kvp.Value;
                    BasicObjectGroup processedGroup = new()
                    {
                        Name = group.Name,
                        Width = group.Width,
                        Height = group.Height,
                        X = group.X,
                        Y = group.Y,
                        _opacity = group._opacity,  // Note: underscore prefix
                        Objects = new Dictionary<string, BasicObject>(),
                        Properties = new Dictionary<string, string>(group.Properties)
                    };

                    // Process each object in the group
                    foreach (KeyValuePair<string, BasicObject> objKvp in group.Objects)
                    {
                        BasicObject processedObject = ProcessObject(objKvp.Value, input.Filename, context);
                        processedGroup.Objects.Add(objKvp.Key, processedObject);
                    }
                    processedMap.ObjectGroups.Add(kvp.Key, processedGroup);
                }

                // Final validation with detailed state
                context.Logger.LogMessage("\n=== Final Map State ===");

                // Basic map properties
                context.Logger.LogMessage($"Map Properties:");
                context.Logger.LogMessage($"  Dimensions: {processedMap.Width}x{processedMap.Height}");
                context.Logger.LogMessage($"  Tile Dimensions: {processedMap.TileWidth}x{processedMap.TileHeight}");
                context.Logger.LogMessage($"  Custom Properties Count: {processedMap.Properties.Count}");
                foreach (KeyValuePair<string, string> prop in processedMap.Properties)
                {
                    context.Logger.LogMessage($"    {prop.Key}: {prop.Value}");
                }

                // Tilesets
                context.Logger.LogMessage($"\nTilesets ({processedMap.Tilesets.Count}):");
                foreach (BasicTileset tileset in processedMap.Tilesets.Values)
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
                context.Logger.LogMessage($"\nLayers ({processedMap.Layers.Count}):");
                foreach (KeyValuePair<string, BasicLayer> layer in input.Layers)
                {
                    context.Logger.LogMessage($"  Layer '{layer.Key}' final state:");
                    context.Logger.LogMessage($"    Dimensions: {layer.Value.Width}x{layer.Value.Height}");
                    context.Logger.LogMessage($"    Tiles Count: {layer.Value.Tiles?.Length ?? 0}");
                    context.Logger.LogMessage($"    Properties Count: {layer.Value.Properties.Count}");
                }

                // Object Groups
                context.Logger.LogMessage($"\nObject Groups ({processedMap.ObjectGroups.Count}):");
                foreach (KeyValuePair<string, BasicObjectGroup> group in input.ObjectGroups)
                {
                    context.Logger.LogMessage($"  Group '{group.Key}' final state:");
                    context.Logger.LogMessage($"    Objects Count: {group.Value.Objects.Count}");
                    context.Logger.LogMessage($"    Properties Count: {group.Value.Properties.Count}");
                }

                context.Logger.LogMessage("\n=== Building Tile Info Caches ===");
                foreach (BasicLayer layer in processedMap.Layers.Values)
                {
                    context.Logger.LogMessage($"Building cache for layer: {layer.Name}");
                    layer.BuildTileInfoCache(processedMap.Tilesets.Values, context);  // Pass the context
                    context.Logger.LogMessage($"  Cache size: {layer.TileInfoCache?.Length ?? 0} entries");
                }

                context.Logger.LogMessage("\n=== Processing Complete ===");



                // Add this before return input; in the Process method
                try
                {
                    context.Logger.LogMessage("\n=== RAW INPUT STATE BEFORE RETURN ===");

                    // Log map properties
                    context.Logger.LogMessage($"Map raw values:");
                    context.Logger.LogMessage($"Width: {processedMap.Width} (Type: {processedMap.Width.GetType()})");
                    context.Logger.LogMessage($"Height: {processedMap.Height} (Type: {processedMap.Height.GetType()})");
                    context.Logger.LogMessage($"TileWidth: {processedMap.TileWidth} (Type: {processedMap.TileWidth.GetType()})");
                    context.Logger.LogMessage($"TileHeight: {processedMap.TileHeight} (Type: {processedMap.TileHeight.GetType()})");

                    // Log each tileset's raw values
                    foreach (BasicTileset tileset in processedMap.Tilesets.Values)
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
                    foreach (BasicLayer layer in processedMap.Layers.Values)
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

                return processedMap;
            }
            catch (Exception ex)
            {
                context.Logger.LogImportantMessage($"Fatal error during processing: {ex.Message}");
                context.Logger.LogImportantMessage($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private BasicObject ProcessObject(BasicObject obj, string mapFilename, ContentProcessorContext context)
        {
            BasicObject processedObject = new()
            {
                Name = obj.Name,
                Width = obj.Width,
                Height = obj.Height,
                X = obj.X,
                Y = obj.Y,
                Properties = new Dictionary<string, string>(obj.Properties ?? new Dictionary<string, string>())
            };

            if (!string.IsNullOrEmpty(obj.Image))
            {
                string texturePath = GetTexturePath(obj.Image, mapFilename, context);
                context.Logger.LogMessage($"  Processing object texture: {texturePath}");

                try
                {
                    processedObject.TileTexture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                        new ExternalReference<TextureContent>(texturePath),
                        "TextureProcessor"
                    );

                    if (processedObject.TileTexture?.Mipmaps.Count > 0)
                    {
                        processedObject.TexWidth = processedObject.TileTexture.Mipmaps[0].Width;
                        processedObject.TexHeight = processedObject.TileTexture.Mipmaps[0].Height;
                    }
                }
                catch (Exception ex)
                {
                    context.Logger.LogImportantMessage($"  Failed to process object texture: {ex.Message}");
                }
            }

            return processedObject;
        }

        private void LogWarning(ContentProcessorContext context, string message, params object[] messageArgs)
        {
            context.Logger.LogWarning("", new ContentIdentity(), message, messageArgs);
        }


    }
}