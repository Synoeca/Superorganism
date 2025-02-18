using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace ContentPipeline
{
    [ContentProcessor(DisplayName = "TSXProcessor")]
    public class TSXProcessor : ContentProcessor<TSXTilesetContent, TSXTilesetContent>
    {
        public override TSXTilesetContent Process(TSXTilesetContent content,
            ContentProcessorContext context)
        {
            context.Logger.LogMessage($"\n=== Processing TSX Tileset: {content.Name} ===");
            LogTilesetState(content, context, "Pre-processing");

            // Process tileset texture
            if (!string.IsNullOrEmpty(content.Image))
            {
                string texturePath = GetTexturePath(content.Image, content.Filename, context);
                context.Logger.LogMessage($"Processing texture: {texturePath}");

                try
                {
                    // Create unique identity for this texture
                    ContentIdentity textureIdentity = new ContentIdentity(texturePath);
                    textureIdentity.SourceTool = content.Name;

                    ExternalReference<TextureContent> textureReference = new ExternalReference<TextureContent>(texturePath, textureIdentity);
                    content.Texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                        textureReference,
                        "TextureProcessor"
                    );

                    if (content.Texture?.Mipmaps.Count > 0)
                    {
                        context.Logger.LogMessage($"Texture processed successfully:");
                        context.Logger.LogMessage($" Width: {content.TexWidth}");
                        context.Logger.LogMessage($" Height: {content.TexHeight}");
                    }
                    else
                    {
                        context.Logger.LogWarning("", textureIdentity, "No mipmaps found in processed texture!");
                    }
                }
                catch (Exception ex)
                {
                    context.Logger.LogImportantMessage($"Error processing texture: {ex.Message}");
                    throw;
                }
            }

            LogTilesetState(content, context, "Post-processing");
            return content;
        }

        private static string GetTexturePath(string imageSource, string mapFilename,
            ContentProcessorContext context)
        {
            string contentDir = Path.GetDirectoryName(mapFilename) ?? string.Empty;
            string absolutePath = Path.GetFullPath(Path.Combine(contentDir, imageSource));
            string relativePath = absolutePath.Replace(context.OutputDirectory, "")
                .TrimStart(Path.DirectorySeparatorChar);
            return relativePath.Replace('\\', '/');
        }

        private static void LogTilesetState(TSXTilesetContent content,
            ContentProcessorContext context, string stage)
        {
            context.Logger.LogMessage($"\n{stage} State:");
            context.Logger.LogMessage($" Name: {content.Name}");
            context.Logger.LogMessage($" TileWidth: {content.TileWidth}");
            context.Logger.LogMessage($" TileHeight: {content.TileHeight}");
            context.Logger.LogMessage($" Spacing: {content.Spacing}");
            context.Logger.LogMessage($" Margin: {content.Margin}");
            context.Logger.LogMessage($" TexWidth: {content.TexWidth}");
            context.Logger.LogMessage($" TexHeight: {content.TexHeight}");
            context.Logger.LogMessage($" Has Texture: {content.Texture != null}");
            context.Logger.LogMessage($" Number of Tiles: {content.Tiles?.Count ?? 0}");
        }
    }
}
