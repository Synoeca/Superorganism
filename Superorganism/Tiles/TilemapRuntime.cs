using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Superorganism.Tiles
{
    /// <summary>
    /// Runtime class for the Tilemap system
    /// </summary>
    public class TilemapRuntime
    {
        public Dictionary<string, TilesetRuntime> Tilesets = new();
        public Dictionary<string, LayerRuntime> Layers = new();
        public Dictionary<string, ObjectGroupRuntime> ObjectGroups = new();
        public Dictionary<string, string> Properties = new();

        public int Width;
        public int Height;
        public int TileWidth;
        public int TileHeight;

        public void Draw(SpriteBatch batch, Rectangle viewport, Vector2 cameraPosition)
        {
            // Calculate the visible area in world coordinates
            Rectangle visibleArea = new(
                (int)cameraPosition.X - viewport.Width / 2,
                (int)cameraPosition.Y - viewport.Height / 2,
                viewport.Width,
                viewport.Height
            );

            // Add padding to ensure we draw tiles just outside the visible area
            visibleArea.Inflate(TileWidth * 2, TileHeight * 2);

            // Draw each layer
            foreach (LayerRuntime layer in Layers.Values)
            {
                layer.Draw(batch, Tilesets.Values, visibleArea, cameraPosition, TileWidth, TileHeight);
            }

            // Draw each object group
            foreach (ObjectGroupRuntime objectGroup in ObjectGroups.Values)
            {
                objectGroup.Draw(this, batch, visibleArea, cameraPosition);
            }
        }
    }

    public class TilesetRuntime
    {
        public int FirstTileId;
        public int TileWidth;
        public int TileHeight;
        public int Spacing;
        public int Margin;
        public Dictionary<int, Dictionary<string, string>> TileProperties = new();
        public Texture2D TileTexture;
        public int TexWidth;
        public int TexHeight;

        public bool MapTileToRect(int index, ref Rectangle rect)
        {
            index -= FirstTileId;
            if (index < 0) return false;

            int rowSize = TexWidth / (TileWidth + Spacing);
            int row = index / rowSize;
            int numRows = TexHeight / (TileHeight + Spacing);
            if (row >= numRows) return false;

            int col = index % rowSize;
            rect.X = col * TileWidth + col * Spacing + Margin;
            rect.Y = row * TileHeight + row * Spacing + Margin;
            rect.Width = TileWidth;
            rect.Height = TileHeight;
            return true;
        }
    }

    public class LayerRuntime
    {
        public Dictionary<string, string> LayerProperties = new();

        public struct TileInfo
        {
            public Texture2D Texture;
            public Rectangle Rectangle;
        }

        public int Width;
        public int Height;
        public float Opacity;
        public int[] Tiles;
        public byte[] FlipAndRotate;
        private TileInfo[] TileInfoCache;

        private const byte HorizontalFlipDrawFlag = 1;
        private const byte VerticalFlipDrawFlag = 2;
        private const byte DiagonallyFlipDrawFlag = 4;

        protected void BuildTileInfoCache(Dictionary<string, TilesetRuntime>.ValueCollection tilesets)
        {
            Rectangle rect = new();
            List<TileInfo> cache = new();
            int i = 1;

            while (true)
            {
                bool found = false;
                foreach (TilesetRuntime ts in tilesets)
                {
                    if (ts.MapTileToRect(i, ref rect))
                    {
                        cache.Add(new TileInfo
                        {
                            Texture = ts.TileTexture,
                            Rectangle = rect
                        });
                        i++;
                        found = true;
                        break;
                    }
                }
                if (!found) break;
            }

            TileInfoCache = cache.ToArray();
        }

        /// <summary>
        /// Gets the tile index of the tile at position (<paramref name="x"/>,<paramref name="y"/>)
        /// in the layer
        /// </summary>
        /// <param name="x">The tile's x-position in the layer</param>
        /// <param name="y">The tile's y-position in the layer</param>
        /// <returns>The index of the tile in the tileset(s)</returns>
        public int GetTile(int x, int y)
        {
            if ((x < 0) || (y < 0) || (x >= Width) || (y >= Height))
                throw new InvalidOperationException();

            int index = (y * Width) + x;
            return Tiles[index];
        }

        public void Draw(SpriteBatch batch, Dictionary<string, TilesetRuntime>.ValueCollection tilesets,
            Rectangle rectangle, Vector2 viewportPosition, int tileWidth, int tileHeight)
        {
            if (TileInfoCache == null) BuildTileInfoCache(tilesets);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int i = (y * Width) + x;
                    byte flipAndRotate = FlipAndRotate[i];
                    SpriteEffects flipEffect = SpriteEffects.None;
                    float rotation = 0f;

                    if ((flipAndRotate & HorizontalFlipDrawFlag) != 0)
                        flipEffect |= SpriteEffects.FlipHorizontally;
                    if ((flipAndRotate & VerticalFlipDrawFlag) != 0)
                        flipEffect |= SpriteEffects.FlipVertically;
                    if ((flipAndRotate & DiagonallyFlipDrawFlag) != 0)
                    {
                        if ((flipAndRotate & HorizontalFlipDrawFlag) != 0 &&
                            (flipAndRotate & VerticalFlipDrawFlag) != 0)
                        {
                            rotation = (float)(Math.PI / 2);
                            flipEffect ^= SpriteEffects.FlipVertically;
                        }
                        else if ((flipAndRotate & HorizontalFlipDrawFlag) != 0)
                        {
                            rotation = (float)-(Math.PI / 2);
                            flipEffect ^= SpriteEffects.FlipVertically;
                        }
                        else if ((flipAndRotate & VerticalFlipDrawFlag) != 0)
                        {
                            rotation = (float)(Math.PI / 2);
                            flipEffect ^= SpriteEffects.FlipHorizontally;
                        }
                        else
                        {
                            rotation = -(float)(Math.PI / 2);
                            flipEffect ^= SpriteEffects.FlipHorizontally;
                        }
                    }

                    int index = Tiles[i] - 1;
                    if (index >= 0 && index < TileInfoCache.Length)
                    {
                        TileInfo info = TileInfoCache[index];
                        Vector2 position = new(x * tileWidth, y * tileHeight);
                        batch.Draw(info.Texture, position, info.Rectangle,
                            Color.White * Opacity, rotation, Vector2.Zero, 1f,
                            flipEffect, 0);
                    }
                }
            }
        }
    }

    public class ObjectGroupRuntime
    {
        public Dictionary<string, ObjectRuntime> Objects = new();
        public Dictionary<string, string> ObjectGroupProperties = new();
        public int Width;
        public int Height;
        public int X;
        public int Y;
        public float Opacity;

        public void Draw(TilemapRuntime map, SpriteBatch batch, Rectangle rectangle, Vector2 viewportPosition)
        {
            foreach (ObjectRuntime obj in Objects.Values)
            {
                if (obj.TileTexture != null)
                {
                    obj.Draw(batch, rectangle, new Vector2(X * map.TileWidth, Y * map.TileHeight),
                        viewportPosition, Opacity);
                }
            }
        }
    }

    public class ObjectRuntime
    {
        public Dictionary<string, string> ObjectProperties = new();
        public int Width;
        public int Height;
        public int X;
        public int Y;
        public Texture2D TileTexture;

        public void Draw(SpriteBatch batch, Rectangle rectangle, Vector2 offset,
            Vector2 viewportPosition, float opacity)
        {
            int minX = (int)Math.Floor(viewportPosition.X);
            int minY = (int)Math.Floor(viewportPosition.Y);
            int maxX = (int)Math.Ceiling(rectangle.Width + viewportPosition.X);
            int maxY = (int)Math.Ceiling(rectangle.Height + viewportPosition.Y);

            if (X + offset.X + Width > minX && X + offset.X < maxX &&
                Y + offset.Y + Height > minY && Y + offset.Y < maxY)
            {
                int x = (int)(X + offset.X - viewportPosition.X);
                int y = (int)(Y + offset.Y - viewportPosition.Y);
                batch.Draw(TileTexture,
                    new Rectangle(x, y, Width, Height),
                    new Rectangle(0, 0, TileTexture.Width, TileTexture.Height),
                    Color.White * opacity);
            }
        }
    }
}