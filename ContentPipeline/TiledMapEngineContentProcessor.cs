using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace ContentPipeline
{
    [ContentProcessor(DisplayName = "TiledMapEngineContentProcessor")]
    public class TiledMapEngineContentProcessor : ContentProcessor<TiledMapEngineContent, TiledMapEngineContent>
    {
        public override TiledMapEngineContent Process(TiledMapEngineContent mapEngine, ContentProcessorContext context)
        {
            context.Logger.LogMessage("\n=== Starting Tilemap Processing ===");
            context.Logger.LogMessage($"Input Map State:");
            context.Logger.LogMessage($"  Filename: {mapEngine.Filename}");
            context.Logger.LogMessage($"  Dimensions: {mapEngine.Width}x{mapEngine.Height}");
            context.Logger.LogMessage($"  Tile Dimensions: {mapEngine.TileWidth}x{mapEngine.TileHeight}");
            context.Logger.LogMessage($"  Number of Tilesets: {mapEngine.Tilesets.Count}");

            context.Logger.LogMessage("\n=== Starting Tilemap Processing ===");
            context.Logger.LogMessage($"Input Map State:");
            context.Logger.LogMessage($"  Filename: {mapEngine.Filename}");
            context.Logger.LogMessage($"  Dimensions: {mapEngine.Width}x{mapEngine.Height}");
            context.Logger.LogMessage($"  Tile Dimensions: {mapEngine.TileWidth}x{mapEngine.TileHeight}");
            context.Logger.LogMessage($"  Number of Tilesets: {mapEngine.Tilesets.Count}");

            foreach (KeyValuePair<string, TilesetContent> tilesetEntry in mapEngine.Tilesets)
            {
                context.Logger.LogMessage($"\nKey: {tilesetEntry.Key}");
                context.Logger.LogMessage($"Value: {tilesetEntry.Value}");
                LogTilesetState(tilesetEntry.Value, context, "Pre-processing");

                if (!string.IsNullOrEmpty(tilesetEntry.Value.Image))
                {
                    string texturePath = GetTexturePath(tilesetEntry.Value.Image, tilesetEntry.Value.Filename, context);
                    context.Logger.LogMessage($"Processing texture: {texturePath}");

                    try
                    {
                        tilesetEntry.Value.Texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                            new ExternalReference<TextureContent>(texturePath),
                            "TextureProcessor"
                        );

                        if (tilesetEntry.Value.Texture?.Mipmaps.Count > 0)
                        {
                            //tilesetEntry.Value.TexWidth = tilesetEntry.Value.Texture.Mipmaps[0].Width;
                            //tilesetEntry.Value.TexHeight = tilesetEntry.Value.Texture.Mipmaps[0].Height;
                            context.Logger.LogMessage($"Texture processed successfully:");
                            context.Logger.LogMessage($" Width: {tilesetEntry.Value.TexWidth}");
                            context.Logger.LogMessage($" Height: {tilesetEntry.Value.TexHeight}");
                        }
                        else
                        {
                            context.Logger.LogWarning("", new ContentIdentity(),
                                "No mipmaps found in processed texture!");
                        }
                    }
                    catch (Exception ex)
                    {
                        context.Logger.LogImportantMessage($"Error processing texture: {ex.Message}");
                        throw;
                    }

                    LogTilesetState(tilesetEntry.Value, context, "Post-processing");
                }
            }

            foreach (KeyValuePair<string, LayerContent> layerEntry in mapEngine.Layers)
            {
                context.Logger.LogMessage($"\nKey: {layerEntry.Key}");
                context.Logger.LogMessage($"Value: {layerEntry.Value}");

                context.Logger.LogMessage($"\n=== Processing Layer: {layerEntry.Value.Name} ===");
                context.Logger.LogMessage($"  Dimensions: {layerEntry.Value.Width}x{layerEntry.Value.Height}");
                context.Logger.LogMessage($"  Opacity: {layerEntry.Value.Opacity}");
                context.Logger.LogMessage($"  Tiles Count: {layerEntry.Value.Tiles?.Length ?? 0}");
                context.Logger.LogMessage($"  FlipAndRotate Count: {layerEntry.Value.FlipAndRotate?.Length ?? 0}");
                context.Logger.LogMessage($"  Properties Count: {layerEntry.Value.Properties?.Count ?? 0}");
                context.Logger.LogMessage($"  FlippedHorizontallyFlag: {LayerContent.FlippedHorizontallyFlag}");
                context.Logger.LogMessage($"  FlippedVerticallyFlag: {LayerContent.FlippedVerticallyFlag}");
                context.Logger.LogMessage($"  FlippedDiagonallyFlag: {LayerContent.FlippedDiagonallyFlag}");
                context.Logger.LogMessage($"  HorizontalFlipDrawFlag: {LayerContent.HorizontalFlipDrawFlag}");
                context.Logger.LogMessage($"  VerticalFlipDrawFlag: {LayerContent.VerticalFlipDrawFlag}");
                context.Logger.LogMessage($"  DiagonallyFlipDrawFlag: {LayerContent.DiagonallyFlipDrawFlag}");
                context.Logger.LogMessage($"  TileInfoCache Count: {layerEntry.Value.TileInfoCache?.Length ?? 0}");
                context.Logger.LogMessage($"  Filename: {layerEntry.Value.Filename}");
            }

            foreach (KeyValuePair<string, GroupContent> groupEntry in mapEngine.Groups)
            {
                context.Logger.LogMessage($"\nKey: {groupEntry.Key}");
                context.Logger.LogMessage($"Value: {groupEntry.Value}");

                context.Logger.LogMessage("\n=== Starting Group Processing ===");
                context.Logger.LogMessage($"Input Group State:");
                context.Logger.LogMessage($"  Name: {groupEntry.Value.Name}");
                context.Logger.LogMessage($"  Id: {groupEntry.Value.Id}");
                context.Logger.LogMessage($"  Locked: {groupEntry.Value.Locked}");
                context.Logger.LogMessage($"  Number of Object Groups: {groupEntry.Value.ObjectGroups?.Count ?? 0}");
                context.Logger.LogMessage($"  Number of Properties: {groupEntry.Value.Properties?.Count ?? 0}");

                if (groupEntry.Value.ObjectGroups != null)
                {
                    foreach (KeyValuePair<string, ObjectGroupContent> objGroupEntry in groupEntry.Value.ObjectGroups)
                    {
                        context.Logger.LogMessage($"\nProcessing Object Group: {objGroupEntry.Key}");
                        ObjectGroupContent objectGroup = objGroupEntry.Value;
                        LogObjectGroupState(objectGroup, context, "Pre-processing");

                        // Process each object in the object group
                        if (objectGroup.Objects != null)
                        {
                            foreach (KeyValuePair<string, ObjectContent> objectEntry in objectGroup.Objects)
                            {
                                context.Logger.LogMessage($"\nProcessing Object: {objectEntry.Key}");
                                ObjectContent obj = objectEntry.Value;
                                LogObjectState(obj, context, "Pre-processing");

                                if (!string.IsNullOrEmpty(obj.Image))
                                {
                                    ProcessObjectTexture(obj, groupEntry.Value.Filename, context);
                                }

                                LogObjectState(obj, context, "Post-processing");
                            }
                        }

                        LogObjectGroupState(objectGroup, context, "Post-processing");
                    }
                }
            }

            return mapEngine;
        }

        private static string GetTexturePath(string imageSource, string mapFilename, ContentProcessorContext context)
        {
            context.Logger.LogMessage($"GetTexturePath: Starting with imageSource={imageSource}, mapFilename={mapFilename}");

            // Ensure we start in the correct Content/Tileset/Maps directory
            string mapDir = "Tileset/Maps";
            context.Logger.LogMessage($"GetTexturePath: Map directory={mapDir}");

            string basePath = mapDir;
            string remainingPath = imageSource;
            while (remainingPath.StartsWith("../"))
            {
                basePath = Path.GetDirectoryName(basePath) ?? string.Empty;
                remainingPath = remainingPath.Substring(3);
            }

            string absolutePath = Path.GetFullPath(Path.Combine(basePath, remainingPath));
            context.Logger.LogMessage($"GetTexturePath: Absolute path={absolutePath}");

            string relativePath = absolutePath.Replace(context.OutputDirectory, "").TrimStart(Path.DirectorySeparatorChar);
            string processedPath = relativePath.Replace('\\', '/');
            context.Logger.LogMessage($"GetTexturePath: Final processed path={processedPath}");

            return processedPath;
        }


        private static void LogTilesetState(TilesetContent tileset, ContentProcessorContext context, string stage)
        {
            context.Logger.LogMessage($"\n{stage} Tileset State:");
            context.Logger.LogMessage($" Name: {tileset.Name}");
            context.Logger.LogMessage($" FirstTileId: {tileset.FirstTileId}");
            context.Logger.LogMessage($" TileWidth: {tileset.TileWidth}");
            context.Logger.LogMessage($" TileHeight: {tileset.TileHeight}");
            context.Logger.LogMessage($" Spacing: {tileset.Spacing}");
            context.Logger.LogMessage($" Margin: {tileset.Margin}");
            context.Logger.LogMessage($" TexWidth: {tileset.TexWidth}");
            context.Logger.LogMessage($" TexHeight: {tileset.TexHeight}");
            context.Logger.LogMessage($" Has Texture: {tileset.Texture != null}\n");
        }

        private void ProcessObjectTexture(ObjectContent obj, string baseFilename, ContentProcessorContext context)
        {
            string texturePath = GetTexturePath(obj.Image, baseFilename, context);
            context.Logger.LogMessage($"Processing texture for Object {obj.Name}: {texturePath}");

            try
            {
                Texture2DContent texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                    new ExternalReference<TextureContent>(texturePath),
                    "TextureProcessor"
                );

                if (texture?.Mipmaps.Count > 0)
                {
                    obj.Texture = texture;
                    obj.TexWidth = texture.Mipmaps[0].Width;
                    obj.TexHeight = texture.Mipmaps[0].Height;

                    context.Logger.LogMessage($"Texture processed successfully:");
                    context.Logger.LogMessage($" Width: {obj.TexWidth}");
                    context.Logger.LogMessage($" Height: {obj.TexHeight}");
                }
                else
                {
                    context.Logger.LogWarning("", new ContentIdentity(),
                        $"No mipmaps found in processed texture for Object {obj.Name}!");
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogImportantMessage($"Error processing texture for Object {obj.Name}: {ex.Message}");
                throw;
            }
        }

        private static void LogObjectState(ObjectContent obj, ContentProcessorContext context, string stage)
        {
            context.Logger.LogMessage($"\n{stage} Object State:");
            context.Logger.LogMessage($" Name: {obj.Name}");
            context.Logger.LogMessage($" Position: ({obj.X}, {obj.Y})");
            context.Logger.LogMessage($" Dimensions: {obj.Width}x{obj.Height}");
            context.Logger.LogMessage($" Image: {obj.Image}");
            context.Logger.LogMessage($" Properties Count: {obj.Properties?.Count ?? 0}");
            context.Logger.LogMessage($" TexWidth: {obj.TexWidth}");
            context.Logger.LogMessage($" TexHeight: {obj.TexHeight}\n");
        }

        private static void LogObjectGroupState(ObjectGroupContent objGroup, ContentProcessorContext context, string stage)
        {
            context.Logger.LogMessage($"\n{stage} Object Group State:");
            context.Logger.LogMessage($" Name: {objGroup.Name}");
            context.Logger.LogMessage($" Class: {objGroup.Class}");
            context.Logger.LogMessage($" Position: ({objGroup.OffsetX}, {objGroup.OffsetY})");
            context.Logger.LogMessage($" Opacity: {objGroup.Opacity}");
            context.Logger.LogMessage($" ParallaxX: {objGroup.ParallaxX}");
            context.Logger.LogMessage($" ParallaxY: {objGroup.ParallaxY}");
            //context.Logger.LogMessage($" Color: {objGroup.Color}");
            //context.Logger.LogMessage($" TintColor: {objGroup.TintColor}");
            //if (objGroup.Color != null)
            //{
            //    context.Logger.LogMessage($"  color A: {objGroup.Color} -> {objGroup.Color.Value.A}");
            //    context.Logger.LogMessage($"  color B: {objGroup.Color} -> {objGroup.Color.Value.B}");
            //    context.Logger.LogMessage($"  color G: {objGroup.Color} -> {objGroup.Color.Value.G}");
            //    context.Logger.LogMessage($"  color R: {objGroup.Color} -> {objGroup.Color.Value.R}");
            //}
            //else
            //{
            //    context.Logger.LogMessage($"  color is null!!");
            //}
            //if (objGroup.TintColor != null)
            //{
            //    context.Logger.LogMessage($"  Tint color A: {objGroup.TintColor} -> {objGroup.TintColor.Value.A}");
            //    context.Logger.LogMessage($"  Tint color B: {objGroup.TintColor} -> {objGroup.TintColor.Value.B}");
            //    context.Logger.LogMessage($"  Tint color G: {objGroup.TintColor} -> {objGroup.TintColor.Value.G}");
            //    context.Logger.LogMessage($"  Tint color R: {objGroup.TintColor} -> {objGroup.TintColor.Value.R}");
            //}
            //else
            //{
            //    context.Logger.LogMessage($"  tint color is null!!");
            //}
            context.Logger.LogMessage($" Visible: {objGroup.Visible}");
            context.Logger.LogMessage($" Locked: {objGroup.Locked}");
            context.Logger.LogMessage($" Properties Count: {objGroup.ObjectProperties?.Count ?? 0}");
            context.Logger.LogMessage($" Objects Count: {objGroup.Objects?.Count ?? 0}\n");
        }
    }
}
