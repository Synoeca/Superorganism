#region Credit
/*
Squared.Tiled
Copyright (C) 2009 Kevin Gadd

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Kevin Gadd kevin.gadd@gmail.com http://luminance.org/
*/
/*
 * Updates by Stephen Belanger - July, 13 2009
 * 
 * -added ProhibitDtd = false, so you don't need to remove the doctype line after each time you edit the map.
 * -changed everything to use SortedLists for easier referencing
 * -added objectgroups
 * -added movable and resizable objects
 * -added object images
 * -added meta property support to maps, layers, object groups and objects
 * -added non-binary encoded layer data
 * -added layer and object group transparency
 * 
 * TODO: I might add support for .tsx Tileset definitions. Note sure yet how beneficial that would be...
*/
/*
 * Modifications by Zach Musgrave - August 2012.
 * 
 * - Fixed errors in TileExample.cs
 * - Added support for rotated and flipped tiles (press Z, X, or Y in Tiled to rotate or flip tiles)
 * - Fixed exception when loading an object without a height or width attribute
 * - Fixed property loading bugs (properties now loaded for Layers, Maps, Objects)
 * - Added support for margin and spacing in tile sets
 * - CF-compatible System.IO.Compression library available via GitHub release. See releases at https://github.com/zachmu/tiled-xna
 * 
 * Zach Musgrave zach.musgrave@gmail.com http://gamedev.sleptlate.org
 */
/* Modifications by Nathan Bean - March 2022.
 * 
 * - Changed XMLReader settings to use DtdProcessing instead of now-depreciated ProhibitDtd = false 
 * - Added XML-style comments to each class and member
 * - Updated Example to use MonoGame 
 * 
 * Nathan Bean nhbean@ksu.edu
 */
 /* Modifications by Synoeca - December 2024.
 *
 * - Added support for ground level collision detection in tilemap
 *
 * Synoeca synoeca523@ksu.edu
 */
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using CompressionMode = System.IO.Compression.CompressionMode;
using GZipStream = System.IO.Compression.GZipStream;

namespace Superorganism.Tiles
{
    /// <summary>
    /// A class representing a TileSet created with the Tiled map editor.
    /// </summary>
    public class Tileset
    {
        ///// <summary>
        ///// A class for holding a list of tile properties 
        ///// </summary>
        ///// <remarks>
        ///// Essentially, a &lt;string, string&gt; dictionary
        ///// </remarks>
        //public class TilePropertyList : Dictionary<string, string>
        //{
        //}

        public string Name;
        public int FirstTileId;
        public int TileWidth;
        public int TileHeight;
        public int Spacing;
        public int Margin;
        public Dictionary<int, Dictionary<string, string>> TileProperties = new();
        public string Image;

        protected Texture2D Texture;
        protected int TexWidth;
        protected int TexHeight;

        // Helper method to parse integer attributes
        private static int ParseIntAttribute(XmlReader reader, string attributeName, int defaultValue = 0)
        {
            string value = reader.GetAttribute(attributeName);
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// Gets the properties of the specified tile
        /// </summary>
        /// <param name="index">The index of the tile</param>
        /// <returns>A dictionary of property names to values</returns>
        public Dictionary<string, string> GetTileProperties(int index)
        {
            index -= FirstTileId;
            if (index < 0)
                return null;
            TileProperties.TryGetValue(index, out Dictionary<string, string> result);
            return result;
        }

        /// <summary>
        /// Gets the texture of this Tileset
        /// </summary>
        public Texture2D TileTexture
        {
            get => Texture;
            set
            {
                Texture = value;
                TexWidth = value.Width;
                TexHeight = value.Height;
            }
        }

        /// <summary>
        /// Converts a map position into a rectangle providing the
        /// bounds of the tile in the TileSet texture.
        /// </summary>
        /// <param name="index">The tile index</param>
        /// <param name="rect">The bounds of the tile in the tileset texture</param>
        /// <returns>True if the tile index exists in the tileset</returns>
        internal bool MapTileToRect(int index, ref Rectangle rect)
        {
            index -= FirstTileId;

            if (index < 0)
                return false;

            int rowSize = TexWidth / (TileWidth + Spacing);
            int row = index / rowSize;
            int numRows = TexHeight / (TileHeight + Spacing);
            if (row >= numRows)
                return false;

            int col = index % rowSize;

            rect.X = col * TileWidth + col * Spacing + Margin;
            rect.Y = row * TileHeight + row * Spacing + Margin;
            rect.Width = TileWidth;
            rect.Height = TileHeight;
            return true;
        }
    }

    /// <summary>
    /// A class representing a Tiled map layer
    /// </summary>
    public class Layer
    {
        /*
         * High-order bits in the tile data indicate tile flipping
         */
        private const uint FlippedHorizontallyFlag = 0x80000000;
        private const uint FlippedVerticallyFlag = 0x40000000;
        private const uint FlippedDiagonallyFlag = 0x20000000;

        internal const byte HorizontalFlipDrawFlag = 1;
        internal const byte VerticalFlipDrawFlag = 2;
        internal const byte DiagonallyFlipDrawFlag = 4;

        public Dictionary<string, string> Properties = new();
        internal struct TileInfo
        {
            public Texture2D Texture;
            public Rectangle Rectangle;
        }

        public string Name;
        public int Width;
        public int Height;
        public float Opacity = 1;
        public int[] Tiles;
        public byte[] FlipAndRotate;
        internal TileInfo[] TileInfoCache;


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

        /// <summary>
        /// Caches the information about each specific tile in the layer
        /// (its texture and bounds within that texture) in a list indexed 
        /// by the tile index for quick retreival/processing
        /// </summary>
        /// <param name="tilesets">The list of tilesets containing tiles to cache</param>
        protected void BuildTileInfoCache(Dictionary<string, Tileset>.ValueCollection tilesets)
        {
            Rectangle rect = new();
            List<TileInfo> cache = [];
            int i = 1;

        next:
            foreach (Tileset ts in tilesets)
            {
                if (ts.MapTileToRect(i, ref rect))
                {
                    cache.Add(new TileInfo
                    {
                        Texture = ts.TileTexture,
                        Rectangle = rect
                    });
                    i += 1;
                    goto next;
                }
            }

            TileInfoCache = cache.ToArray();
        }

        /// <summary>
        /// Draws the layer
        /// </summary>
        /// <param name="batch">The SpriteBatch to draw with</param>
        /// <param name="tilesets">A list of tilesets associated with the layer</param>
        /// <param name="rectangle">The viewport to render within</param>
        /// <param name="viewportPosition">The viewport's position in the layer</param>
        /// <param name="tileWidth">The width of a tile</param>
        /// <param name="tileHeight">The height of a tile</param>
        public void Draw(SpriteBatch batch, Dictionary<string, Tileset>.ValueCollection tilesets, Rectangle rectangle, Vector2 viewportPosition, int tileWidth, int tileHeight)
        {
            if (TileInfoCache == null)
                BuildTileInfoCache(tilesets);

            // Draw all tiles in the layer
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int i = (y * Width) + x;

                    byte flipAndRotate = FlipAndRotate[i];
                    SpriteEffects flipEffect = SpriteEffects.None;
                    float rotation = 0f;

                    // Handle flip and rotation flags
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
                    if (index >= 0 && index < TileInfoCache!.Length)
                    {
                        TileInfo info = TileInfoCache[index];

                        // Position tiles relative to ground level
                        Vector2 position = new(
                            x * tileWidth,    // X position is straightforward left-to-right
                            y * tileHeight    // Y position is top-to-bottom
                        );

                        batch.Draw(
                            info.Texture,
                            position,
                            info.Rectangle,
                            Color.White * Opacity,
                            rotation,
                            Vector2.Zero, // Don't use center origin since it positions manually
                            1f,
                            flipEffect,
                            0
                        );
                    }
                }
            }
        }
    }

    /// <summary>
    /// A class representing a group of map Objects
    /// </summary>
    public class ObjectGroup
    {
        public Dictionary<string, Object> Objects = new();
        public Dictionary<string, string> Properties = new();

        public string Name;
        public int Width, Height, X, Y;
        private float _opacity = 1;

        public void Draw(Map result, SpriteBatch batch, Rectangle rectangle, Vector2 viewportPosition)
        {
            foreach (Object objects in Objects.Values)
            {
                if (objects.TileTexture != null)
                {
                    objects.Draw(batch, rectangle, new Vector2(X * result.TileWidth, Y * result.TileHeight), viewportPosition, _opacity);
                }
            }
        }
    }

    /// <summary>
    /// A class representing a map object
    /// </summary>
    /// <remarks>
    /// A map object represents an object in a map; it has a 
    /// position, width, and height, and a collection of properties.
    /// It can be used for spawn locations, triggers, etc.
    /// In this implementation, it also has a texture 
    /// </remarks>
    public class Object
    {
        public Dictionary<string, string> Properties = new();

        public string Name, Image;
        public int Width, Height, X, Y;

        protected Texture2D Texture;
        protected int TexWidth;
        protected int TexHeight;

        /// <summary>
        /// The texture of the Object
        /// </summary>
        /// <remarks>
        /// This is not supplied by Tiled, and must be set manually!
        /// </remarks>
        public Texture2D TileTexture
        {
            get => Texture;
            set
            {
                Texture = value;
                TexWidth = value.Width;
                TexHeight = value.Height;
            }
        }

        /// <summary>
        /// Draws the Object
        /// </summary>
        /// <param name="batch">The SpriteBatch to draw with</param>
        /// <param name="rectangle">The viewport (visible screen size)</param>
        /// <param name="offset">An offset to apply when rendering the viewport on-screen</param>
        /// <param name="viewportPosition">The viewport's position in the world</param>
        /// <param name="opacity">An opacity value for making the object semi-transparent (1.0=fully opaque)</param>
        public void Draw(SpriteBatch batch, Rectangle rectangle, Vector2 offset, Vector2 viewportPosition, float opacity)
        {
            int minX = (int)Math.Floor(viewportPosition.X);
            int minY = (int)Math.Floor(viewportPosition.Y);
            int maxX = (int)Math.Ceiling((rectangle.Width + viewportPosition.X));
            int maxY = (int)Math.Ceiling((rectangle.Height + viewportPosition.Y));

            if (X + offset.X + Width > minX && X + offset.X < maxX 
                                            && Y + offset.Y + Height > minY && Y + offset.Y < maxY)
            {
                int x = (int)(X + offset.X - viewportPosition.X);
                int y = (int)(Y + offset.Y - viewportPosition.Y);
                batch.Draw(Texture, new Rectangle(x, y, Width, Height), new Rectangle(0, 0, Texture.Width, Texture.Height), Color.White * opacity);
            }
        }
    }

    /// <summary>
    /// A class representing a map created by the Tiled editor
    /// </summary>
    public class Map
    {
        /// <summary>
        /// The Map's width and height
        /// </summary>
        public int Width;

        public int Height;

        /// <summary>
        /// The Map's tile width and height
        /// </summary>
        public int TileWidth;

        public int TileHeight;

        /// <summary>
        /// The Map's Tilesets
        /// </summary>
        public Dictionary<string, Tileset> Tilesets;

        /// <summary>
        /// The Map's Layers
        /// </summary>
        public Dictionary<string, Layer> Layers = new();

        /// <summary>
        /// The Map's Object Groups
        /// </summary>
        public Dictionary<string, ObjectGroup> ObjectGroups = new();

        /// <summary>
        /// The Map's properties
        /// </summary>
        public Dictionary<string, string> Properties = new();


        /// <summary>
        /// Draws the Map
        /// </summary>
        /// <param name="batch">The SpriteBatch to draw with</param>
        /// <param name="viewport">The Viewport to draw within</param>
        /// <param name="cameraPosition">The position of the viewport within the map</param>
        public void Draw(SpriteBatch batch, Rectangle viewport, Vector2 cameraPosition)
        {
            // Calculate the visible area in world coordinates
            Rectangle visibleArea = new(
                (int)cameraPosition.X - viewport.Width / 2,  // Left edge
                (int)cameraPosition.Y - viewport.Height / 2, // Top edge
                viewport.Width,
                viewport.Height
            );

            // Add padding to ensure we draw tiles just outside the visible area
            visibleArea.Inflate(TileWidth * 2, TileHeight * 2);

            // Draw the layers
            foreach (Layer layer in Layers.Values)
            {
                layer.Draw(batch, Tilesets.Values, visibleArea, cameraPosition, TileWidth, TileHeight);
            }

            // Draw the objects
            foreach (ObjectGroup objectGroup in ObjectGroups.Values)
            {
                objectGroup.Draw(this, batch, visibleArea, cameraPosition);
            }
        }
    }
}

