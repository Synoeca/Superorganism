using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Core.Background
{
    public class ParallaxBackground(GraphicsDevice graphicsDevice)
    {
        private class Layer(Texture2D texture, float scrollSpeed, float heightRatio)
        {
            public readonly Texture2D Texture = texture;
            public readonly float ScrollSpeed = scrollSpeed;
            public readonly float HeightRatio = heightRatio;
        }

        private readonly Layer[] _layers = new Layer[3];
        private const int MapWidth = 12800; // 200 * 64
        private const int MapHeight = 3200; // 50 * 64

        public void LoadContent(ContentManager content)
        {
            // Background (1280x1472) - stays at bottom
            _layers[0] = new Layer(content.Load<Texture2D>("background"), 0.0250f, 0f);
            // Midground (7800x480) - lifted
            _layers[1] = new Layer(content.Load<Texture2D>("midground"), 0.055f, 0.1f);
            // Foreground (14000x480) - lifted
            _layers[2] = new Layer(content.Load<Texture2D>("foreground"), 0.0880f, 0.1f);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            float screenHeight = graphicsDevice.Viewport.Height;

            foreach (Layer layer in _layers)
            {
                float parallaxOffset = -cameraPosition.X * layer.ScrollSpeed;
                int textureWidth = layer.Texture.Width;
                float yPosition = screenHeight - layer.Texture.Height - (screenHeight * layer.HeightRatio);

                if (textureWidth == 1280) // Background
                {
                    // Calculate how many segments needed to cover entire map width
                    int totalSegmentsNeeded = (int)Math.Ceiling((float)MapWidth / textureWidth) + 2;

                    // Calculate the first visible texture position
                    float firstX = parallaxOffset;
                    firstX = firstX - (textureWidth * (float)Math.Floor(firstX / textureWidth));
                    firstX -= textureWidth * 2; // Add buffer on the left

                    // Draw all segments
                    for (int i = 0; i < totalSegmentsNeeded; i++)
                    {
                        Vector2 position = new(
                            firstX + (i * textureWidth),
                            yPosition
                        );

                        spriteBatch.Draw(
                            layer.Texture,
                            position,
                            null,
                            Color.White,
                            0f,
                            Vector2.Zero,
                            Vector2.One,
                            SpriteEffects.None,
                            0.9f
                        );
                    }
                }
                else // Midground and foreground
                {
                    // For midground (7800px wide)
                    int segmentsNeeded = (textureWidth == 7800) ? 2 : 1; // One segment for foreground (14000px)
                    float startX = (parallaxOffset % textureWidth) - textureWidth;

                    for (int i = 0; i < segmentsNeeded; i++)
                    {
                        Vector2 position = new(
                            startX + (i * textureWidth),
                            yPosition
                        );

                        float layerDepth = 0.9f - (Array.IndexOf(_layers, layer) * 0.1f);
                        spriteBatch.Draw(
                            layer.Texture,
                            position,
                            null,
                            Color.White,
                            0f,
                            Vector2.Zero,
                            Vector2.One,
                            SpriteEffects.None,
                            layerDepth
                        );
                    }
                }
            }
        }

        public void Unload()
        {
            foreach (Layer layer in _layers)
            {
                layer.Texture?.Dispose();
            }
        }
    }
}