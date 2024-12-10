using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Core.Background;

public class ParallaxBackground
{
    private class Layer(Texture2D texture, float scrollSpeed)
    {
        public readonly Texture2D Texture = texture;
        public readonly float ScrollSpeed = scrollSpeed;
    }

    private readonly Layer[] _layers = new Layer[3];
    private const int MapWidth = 12800; // 200 * 64
    private const float TargetYPosition = -32f; // Base Y position for background
    private const float MidgroundYOffset = 850; // Adjust this to move midground down
    private const float ForegroundYOffset = 850f; // Adjust this to move foreground down

    public void LoadContent(ContentManager content)
    {
        // Background (1280x1472)
        _layers[0] = new Layer(content.Load<Texture2D>("background"), 0.0250f);
        // Midground (7800x480)
        _layers[1] = new Layer(content.Load<Texture2D>("midground"), 0.055f);
        // Foreground (14000x480)
        _layers[2] = new Layer(content.Load<Texture2D>("foreground"), 0.1f);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
    {
        foreach (Layer layer in _layers)
        {
            float parallaxOffset = -cameraPosition.X * layer.ScrollSpeed;
            int textureWidth = layer.Texture.Width;
            float yPosition = TargetYPosition;

            if (textureWidth == 1280) // Background
            {
                // Background logic remains the same
                int totalSegmentsNeeded = (int)Math.Ceiling((float)MapWidth / textureWidth) + 2;
                float firstX = parallaxOffset;
                firstX -= (textureWidth * (float)Math.Floor(firstX / textureWidth));
                firstX -= textureWidth * 2;

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
                // Adjust Y position
                if (textureWidth == 7800) // Midground
                {
                    yPosition += MidgroundYOffset;
                }
                else // Foreground (14000px)
                {
                    yPosition += ForegroundYOffset;
                }

                // Always use 2 segments for both midground and foreground
                int segmentsNeeded = 2;

                // Calculate starting position
                float startX;
                if (textureWidth == 14000) // Foreground
                {
                    // Adjust the starting position calculation for the wider foreground
                    startX = (parallaxOffset % (textureWidth / 2)) - (textureWidth / 2);
                }
                else // Midground
                {
                    startX = (parallaxOffset % textureWidth) - textureWidth;
                }

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