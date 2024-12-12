using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.Collections.Generic;

namespace ContentPipeline
{
    /// <summary>
    /// A content processor for tilemap content that processes tilesets, layers, and objects,
    /// building necessary texture assets and preparing them for runtime use.
    /// </summary>
    [ContentProcessor(DisplayName = "Tilemap Processor")]
    public class TilemapProcessor : ContentProcessor<BasicMap, BasicMap>
    {
        public override BasicMap Process(BasicMap input, ContentProcessorContext context)
        {
            context.Logger.LogMessage("Starting Tilemap Processing...");

            try
            {
                // Process each tileset
                foreach (var tilesetEntry in input.Tilesets)
                {
                    ProcessTileset(tilesetEntry.Value, context, input.Filename);
                }

                // Process each object group's objects
                foreach (var objectGroup in input.ObjectGroups.Values)
                {
                    foreach (var obj in objectGroup.Objects.Values)
                    {
                        if (!string.IsNullOrEmpty(obj.Image))
                        {
                            ProcessObject(obj, context, input.Filename);
                        }
                    }
                }

                context.Logger.LogMessage("Tilemap Processing completed successfully.");
                return input;
            }
            catch (Exception ex)
            {
                context.Logger.LogImportantMessage($"Error processing tilemap: {ex.Message}");
                throw;
            }
        }

        private void ProcessTileset(BasicTileset tileset, ContentProcessorContext context, string mapFilename)
        {
            if (string.IsNullOrEmpty(tileset.Image)) return;

            try
            {
                string texturePath = GetTexturePath(tileset.Image, mapFilename, context);
                context.Logger.LogMessage($"Processing tileset texture: {texturePath}");

                ExternalReference<TextureContent> textureReference = new(texturePath);
                tileset.TileTexture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                    textureReference,
                    "TextureProcessor"
                );

                if (tileset.TileTexture.Mipmaps.Count > 0)
                {
                    tileset.TexWidth = tileset.TileTexture.Mipmaps[0].Width;
                    tileset.TexHeight = tileset.TileTexture.Mipmaps[0].Height;
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogWarning("", new ContentIdentity(),
                    $"Failed to process tileset {tileset.Name}: {ex.Message}");
                throw;
            }
        }

        private string GetTexturePath(string imageSource, string mapFilename, ContentProcessorContext context)
        {
            context.Logger.LogMessage($"GetTexturePath: Starting with imageSource={imageSource}, mapFilename={mapFilename}");

            // Get the Content folder path (two levels up from Maps folder)
            string contentRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(mapFilename)));
            context.Logger.LogMessage($"GetTexturePath: Content root={contentRoot}");

            // Get the directory of the currently processing content file
            string contentDir = Path.GetDirectoryName(mapFilename);
            context.Logger.LogMessage($"GetTexturePath: Content directory={contentDir}");

            // Handle the "../" in the image source path
            string normalizedImageSource = imageSource;
            if (imageSource.StartsWith("../"))
            {
                contentDir = Path.GetDirectoryName(contentDir);
                normalizedImageSource = imageSource.Substring(3);
            }

            // Get the absolute path of the image
            string absolutePath = Path.GetFullPath(Path.Combine(contentDir, normalizedImageSource));
            context.Logger.LogMessage($"GetTexturePath: Absolute path={absolutePath}");

            // Make the path relative to the Content folder
            string relativePath = Path.GetRelativePath(contentRoot, absolutePath);
            context.Logger.LogMessage($"GetTexturePath: Relative path={relativePath}");

            string processedPath = relativePath.Replace('\\', '/');
            context.Logger.LogMessage($"GetTexturePath: Final processed path={processedPath}");

            return processedPath;
        }

        private void ProcessObject(BasicObject obj, ContentProcessorContext context, string mapFilename)
        {
            if (string.IsNullOrEmpty(obj.Image)) return;

            try
            {
                string texturePath = GetTexturePath(obj.Image, mapFilename, context);
                context.Logger.LogMessage($"Processing object texture: {texturePath}");

                ExternalReference<TextureContent> textureReference = new(texturePath);
                obj.TileTexture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                    textureReference,
                    "TextureProcessor"
                );

                if (obj.TileTexture.Mipmaps.Count > 0)
                {
                    obj.TexWidth = obj.TileTexture.Mipmaps[0].Width;
                    obj.TexHeight = obj.TileTexture.Mipmaps[0].Height;
                }

                // If object dimensions weren't specified, use texture dimensions
                if (obj.Width == 0) obj.Width = obj.TexWidth;
                if (obj.Height == 0) obj.Height = obj.TexHeight;
            }
            catch (Exception ex)
            {
                context.Logger.LogWarning("", new ContentIdentity(),
                    $"Failed to process object {obj.Name}: {ex.Message}");
                throw;
            }
        }
    }
}