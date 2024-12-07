using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Core.Background
{
    public class ParallaxBackground(GraphicsDevice graphicsDevice)
    {
        private class Layer(Texture2D texture, float scrollSpeed)
        {
            public readonly Texture2D Texture = texture;
            public readonly float ScrollSpeed = scrollSpeed;
        }

        private readonly Layer[] _layers = new Layer[3]; // Background, midground, foreground
        private const int GroundY = 400;

        public void LoadContent(ContentManager content)
        {
            // Load textures and set their scroll speeds
            // Background moves slowest, foreground moves fastest
            _layers[0] = new Layer(content.Load<Texture2D>("background"), 0.0250f);
            _layers[1] = new Layer(content.Load<Texture2D>("midground"), 0.055f);
            _layers[2] = new Layer(content.Load<Texture2D>("foreground"), 0.0880f);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            // For each layer, calculate its position based on camera position and scroll speed
            foreach (Layer layer in _layers)
            {
                // Center the texture initially by offsetting by half its width
                float centerOffset = layer.Texture.Width / 2f;

                // Calculate the parallax offset for this layer
                float parallaxOffset = -cameraPosition.X * layer.ScrollSpeed;

                // Calculate how many times we need to repeat the texture
                int textureWidth = layer.Texture.Width;
                int screensNeeded = (graphicsDevice.Viewport.Width / textureWidth) + 2;

                // Calculate vertical position - align bottom with ground
                float yPosition = GroundY - layer.Texture.Height;

                // Calculate the starting position to ensure textures are visible
                float startX = (parallaxOffset % textureWidth) - textureWidth + centerOffset;

                // Draw the layer multiple times to cover the screen
                for (int i = 0; i < screensNeeded; i++)
                {
                    Vector2 position = new(
                        startX + (i * textureWidth),
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
                        1f
                    );
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