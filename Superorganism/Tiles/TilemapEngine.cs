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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using CompressionMode = System.IO.Compression.CompressionMode;
using GZipStream = System.IO.Compression.GZipStream;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Superorganism.Tiles
{
    /// <summary>
    /// A class representing a tile in a Tiled tileset
    /// </summary>
    /// <remarks>
    /// A tile is the basic building block of a tilemap. Each tile has a unique ID within its tileset,
    /// and can have additional properties such as type, probability for random placement, and custom
    /// properties. Tiles can be used to create the visual and functional elements of a map, from
    /// terrain and decorations to special gameplay elements.
    /// </remarks>
    public class Tile
    {
        /// <summary>
        /// The tile's ID within its tileset
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The tile's type (optional)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The probability of this tile being chosen when placing random tiles (optional)
        /// Default is 1.0 if not specified
        /// </summary>
        public float Probability { get; set; }

        /// <summary>
        /// Custom properties for the tile
        /// </summary>
        public Dictionary<string, string> Properties { get; set; }

        /// <summary>
        /// Creates a new Tile instance
        /// </summary>
        public Tile()
        {
            Properties = new Dictionary<string, string>();
        }

        /// <summary>
        /// Creates a new Tile instance with the specified ID
        /// </summary>
        /// <param name="id">The tile's ID</param>
        public Tile(int id) : this()
        {
            Id = id;
        }

        /// <summary>
        /// Creates a new Tile instance with the specified ID and type
        /// </summary>
        /// <param name="id">The tile's ID</param>
        /// <param name="type">The tile's type</param>
        public Tile(int id, string type) : this(id)
        {
            Type = type;
        }

        /// <summary>
        /// Creates a new Tile instance with the specified ID, type, and probability
        /// </summary>
        /// <param name="id">The tile's ID</param>
        /// <param name="type">The tile's type</param>
        /// <param name="probability">The tile's probability of being chosen for random placement</param>
        public Tile(int id, string type, float probability) : this(id, type)
        {
            Probability = probability;
        }

        /// <summary>
        /// Gets a boolean property value
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="defaultValue">The default value if the property doesn't exist or is invalid</param>
        /// <returns>The boolean value of the property</returns>
        public bool GetBoolProperty(string propertyName, bool defaultValue = false)
        {
            if (Properties.TryGetValue(propertyName, out string value))
            {
                return bool.TryParse(value, out bool result) ? result : defaultValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets an integer property value
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="defaultValue">The default value if the property doesn't exist or is invalid</param>
        /// <returns>The integer value of the property</returns>
        public int GetIntProperty(string propertyName, int defaultValue = 0)
        {
            if (Properties.TryGetValue(propertyName, out string value))
            {
                return int.TryParse(value, out int result) ? result : defaultValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets a float property value
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="defaultValue">The default value if the property doesn't exist or is invalid</param>
        /// <returns>The float value of the property</returns>
        public float GetFloatProperty(string propertyName, float defaultValue = 0.0f)
        {
            if (Properties.TryGetValue(propertyName, out string value))
            {
                return float.TryParse(value, out float result) ? result : defaultValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets a string property value
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="defaultValue">The default value if the property doesn't exist</param>
        /// <returns>The string value of the property</returns>
        public string GetStringProperty(string propertyName, string defaultValue = "")
        {
            return Properties.GetValueOrDefault(propertyName, defaultValue);
        }

        /// <summary>
        /// Checks if a property exists
        /// </summary>
        /// <param name="propertyName">The name of the property to check</param>
        /// <returns>True if the property exists, false otherwise</returns>
        public bool HasProperty(string propertyName)
        {
            return Properties.ContainsKey(propertyName);
        }
    }

    /// <summary>
    /// A class representing a TileSet created with the Tiled map editor.
    /// </summary>
    public class Tileset
    {
        /// <summary>
        /// The name of the tileset
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The first tile ID in the tileset
        /// </summary>
        public int FirstGid { get; set; }

        /// <summary>
        /// The width of each tile
        /// </summary>
        public int TileWidth { get; set; }

        /// <summary>
        /// The height of each tile
        /// </summary>
        public int TileHeight { get; set; }

        /// <summary>
        /// The spacing between tiles
        /// </summary>
        public int Spacing { get; set; }

        /// <summary>
        /// The margin around tiles
        /// </summary>
        public int Margin { get; set; }

        /// <summary>
        /// Dictionary of all tiles in this tileset, indexed by their local ID (index - FirstGid)
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();

        /// <summary>
        /// Dictionary of all tiles in this tileset, indexed by their local ID (index - FirstGid)
        /// </summary>
        public Dictionary<int, Tile> Tiles { get; set; } = new();

        /// <summary>
        /// The image source path for the tileset
        /// </summary>
        public string Image;

        /// <summary>
        /// The texture containing all tiles
        /// </summary>
        protected Texture2D Texture;

        /// <summary>
        /// The width of the tileset texture
        /// </summary>
        protected int TexWidth;

        /// <summary>
        /// The height of the tileset texture
        /// </summary>
        protected int TexHeight;

        /// <summary>
        /// Loads a Tileset from a TMX file
        /// </summary>
        /// <param name="reader">A reader processing the file</param>
        /// <returns>An initialized Tileset object</returns>
        internal static Tileset Load(XmlReader reader)
        {
            Tileset result = new()
            {
                Name = reader.GetAttribute("name"),
                FirstGid = XmlParsingUtilities.ParseIntAttribute(reader, "firstgid"),
                TileWidth = XmlParsingUtilities.ParseIntAttribute(reader, "tilewidth"),
                TileHeight = XmlParsingUtilities.ParseIntAttribute(reader, "tileheight"),
                Margin = XmlParsingUtilities.ParseIntAttribute(reader, "margin"),
                Spacing = XmlParsingUtilities.ParseIntAttribute(reader, "spacing")
            };

            while (reader.Read())
            {
                string name = reader.Name;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (name)
                        {
                            case "image":
                                result.Image = reader.GetAttribute("source");
                                break;
                            case "tile":
                                int tileId = XmlParsingUtilities.ParseIntAttribute(reader, "id");
                                Tile tile = new()
                                {
                                    Id = tileId,
                                    Type = reader.GetAttribute("type"),
                                    Probability = XmlParsingUtilities.ParseFloatAttribute(reader, "probability", 1.0f),
                                    Properties = new Dictionary<string, string>()
                                };

                                // Read tile properties
                                using (XmlReader tileReader = reader.ReadSubtree())
                                {
                                    while (tileReader.Read())
                                    {
                                        if (tileReader.NodeType == XmlNodeType.Element &&
                                            tileReader.Name == "properties")
                                        {
                                            using XmlReader propsReader = tileReader.ReadSubtree();
                                            while (propsReader.Read())
                                            {
                                                if (propsReader.NodeType == XmlNodeType.Element &&
                                                    propsReader.Name == "property")
                                                {
                                                    string propName = propsReader.GetAttribute("name");
                                                    string propValue = propsReader.GetAttribute("value");
                                                    if (!string.IsNullOrEmpty(propName))
                                                    {
                                                        tile.Properties[propName] = propValue;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                result.Tiles[tileId] = tile;
                                break;
                            case "property":
                            {
                                string propName = reader.GetAttribute("name");
                                string propValue = reader.GetAttribute("value");
                                if (!string.IsNullOrEmpty(propName))
                                {
                                    result.Properties[propName] = propValue;
                                }
                            }
                                break;
                        }

                        break;
                    case XmlNodeType.EndElement:
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// The texture containing the tileset image
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
            index -= FirstGid;

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
        // Bit flags for tile flipping in TMX format
        // These are stored in the high bits of the tile data
        public const uint FlippedHorizontallyFlag = 0x80000000;  // Leftmost bit
        public const uint FlippedVerticallyFlag = 0x40000000;    // Second from left
        public const uint FlippedDiagonallyFlag = 0x20000000;    // Third from left

        // Internal flags used for rendering flipped tiles
        internal const byte HorizontalFlipDrawFlag = 1;   // Flip along vertical axis
        internal const byte VerticalFlipDrawFlag = 2;     // Flip along horizontal axis
        internal const byte DiagonallyFlipDrawFlag = 4;   // Rotate 90 degrees

        // Layer properties stored as key-value pairs
        public SortedList<string, string> Properties = new();

        // Structure to cache texture and position information for each tile
        internal struct TileInfo
        {
            public Texture2D Texture;
            public Rectangle Rectangle;
        }

        /// <summary>
        /// The name of the layer as defined in Tiled
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The width of the layer in tiles
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the layer in tiles
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The opacity of the layer (0.0 to 1.0, where 0 is fully transparent and 1 is fully opaque)
        /// </summary>
        public float Opacity { get; set; } = 1;

        /// <summary>
        /// Array containing the global tile IDs for each position in the layer.
        /// The array length is Width * Height, with tiles stored in row-major order
        /// </summary>
        public int[] Tiles { get; set; }

        /// <summary>
        /// Array containing flip and rotation flags for each tile.
        /// Corresponds one-to-one with the Tiles array
        /// Each byte stores flags for horizontal, vertical, and diagonal flipping
        /// </summary>
        public byte[] FlipAndRotate { get; set; }

        /// <summary>
        /// Cache of texture and rectangle information for each tile type.
        /// Used to optimize rendering by storing pre-calculated tile source rectangles
        /// </summary>
        internal TileInfo[] TileInfoCache { get; set; }

        /// <summary>
        /// Loads the layer from a TMX file
        /// </summary>
        /// <param name="reader">A reader to the TMX file currently being processed</param>
        /// <returns>An initialized Layer object</returns>
        internal static Layer Load(XmlReader reader)
        {
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            Layer result = new();

            if (reader.GetAttribute("name") != null)
            {
                result.Name = reader.GetAttribute("name");
            }
            if (reader.GetAttribute("width") != null)
            {
                result.Width = int.Parse(reader.GetAttribute("width") ?? throw new InvalidOperationException());
            }
            if (reader.GetAttribute("height") != null)
            {
                result.Height = int.Parse(reader.GetAttribute("height") ?? throw new InvalidOperationException());
            }
            if (reader.GetAttribute("opacity") != null)
            {
                result.Opacity = float.Parse(reader.GetAttribute("opacity") ?? throw new InvalidOperationException(), NumberStyles.Any, ci);
            }
            result.Tiles = new int[result.Width * result.Height];
            result.FlipAndRotate = new byte[result.Width * result.Height];

            while (!reader.EOF)
            {
                string name = reader.Name;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (name)
                        {
                            case "data":
                                {
                                    if (reader.GetAttribute("encoding") == null)
                                    {
                                        using XmlReader st = reader.ReadSubtree();
                                        int i = 0;
                                        while (!st.EOF)
                                        {
                                            switch (st.NodeType)
                                            {
                                                case XmlNodeType.Element:
                                                    if (st.Name == "tile")
                                                    {
                                                        if (i < result.Tiles.Length)
                                                        {
                                                            result.Tiles[i] = int.Parse(st.GetAttribute("gid"));
                                                            i++;
                                                        }
                                                    }

                                                    break;
                                                case XmlNodeType.EndElement:
                                                    break;
                                            }

                                            st.Read();
                                        }
                                    }
                                    else
                                    {
                                        string encoding = reader.GetAttribute("encoding");
                                        string compressor = reader.GetAttribute("compression");
                                        switch (encoding)
                                        {
                                            case "base64":
                                                {
                                                    int dataSize = (result.Width * result.Height * 4) + 1024;
                                                    byte[] buffer = new byte[dataSize];
                                                    reader.ReadElementContentAsBase64(buffer, 0, dataSize);

                                                    Stream stream = new MemoryStream(buffer, false);
                                                    switch (compressor)
                                                    {
                                                        case "gzip":
                                                            stream = new GZipStream(stream, CompressionMode.Decompress, false);
                                                            break;
                                                        case "zlib":
                                                            stream = new GZipStream(stream, CompressionMode.Decompress, false);
                                                            break;
                                                    }

                                                    using (stream)
                                                    using (BinaryReader br = new(stream))
                                                    {
                                                        for (int i = 0; i < result.Tiles.Length; i++)
                                                        {
                                                            uint tileData = br.ReadUInt32();

                                                            // The data contain flip information as well as the tileset index
                                                            byte flipAndRotateFlags = 0;
                                                            if ((tileData & FlippedHorizontallyFlag) != 0)
                                                            {
                                                                flipAndRotateFlags |= HorizontalFlipDrawFlag;
                                                            }

                                                            if ((tileData & FlippedVerticallyFlag) != 0)
                                                            {
                                                                flipAndRotateFlags |= VerticalFlipDrawFlag;
                                                            }

                                                            if ((tileData & FlippedDiagonallyFlag) != 0)
                                                            {
                                                                flipAndRotateFlags |= DiagonallyFlipDrawFlag;
                                                            }

                                                            result.FlipAndRotate[i] = flipAndRotateFlags;

                                                            // Clear the flip bits before storing the tile data
                                                            tileData &= ~(FlippedHorizontallyFlag |
                                                                          FlippedVerticallyFlag |
                                                                          FlippedDiagonallyFlag);
                                                            result.Tiles[i] = (int)tileData;
                                                        }
                                                    }

                                                    continue;
                                                }

                                            default:
                                                throw new Exception("Unrecognized encoding.");
                                        }
                                    }

                                    Console.WriteLine("It made it!");
                                }
                                break;
                            case "properties":
                                {
                                    using XmlReader st = reader.ReadSubtree();
                                    while (!st.EOF)
                                    {
                                        switch (st.NodeType)
                                        {
                                            case XmlNodeType.Element:
                                                if (st.Name == "property")
                                                {
                                                    if (st.GetAttribute("name") != null)
                                                    {
                                                        result.Properties.Add(st.GetAttribute("name"), st.GetAttribute("value"));
                                                    }
                                                }

                                                break;
                                            case XmlNodeType.EndElement:
                                                break;
                                        }

                                        st.Read();
                                    }
                                }
                                break;
                        }

                        break;
                    case XmlNodeType.EndElement:
                        break;
                }

                reader.Read();
            }

            return result;
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

        /// <summary>
        /// Sets the tile at the specified position in the layer
        /// </summary>
        /// <param name="x">The x-coordinate of the tile</param>
        /// <param name="y">The y-coordinate of the tile</param>
        /// <param name="tileId">The new tile ID (0 for empty tile)</param>
        public void SetTile(int x, int y, int tileId)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                throw new InvalidOperationException("Tile coordinates out of bounds");

            int index = (y * Width) + x;
            Tiles[index] = tileId;
            FlipAndRotate[index] = 0; // Reset any flip/rotate flags
        }

        /// <summary>
        /// Caches the information about each specific tile in the layer
        /// (its texture and bounds within that texture) in a list indexed 
        /// by the tile index for quick retreival/processing
        /// </summary>
        /// <param name="tilesets">The list of tilesets containing tiles to cache</param>
        protected void BuildTileInfoCache(IList<Tileset> tilesets)
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
        public void Draw(SpriteBatch batch, IList<Tileset> tilesets, Rectangle rectangle, Vector2 viewportPosition, int tileWidth, int tileHeight)
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

                    int tileId = Tiles[i];
                    int index = tileId - 1;

                    if (index >= 0 && index < TileInfoCache!.Length)
                    {
                        TileInfo info = TileInfoCache[index];

                        // Get tile opacity from tileset
                        float tileOpacity = Opacity; // Default to layer opacity

                        // Find the tileset that contains this tile
                        foreach (Tileset tileset in tilesets)
                        {
                            if (tileId >= tileset.FirstGid && tileset.Tiles.ContainsKey(tileId - tileset.FirstGid))
                            {
                                Tile tile = tileset.Tiles[tileId - tileset.FirstGid];
                                if (tile.Properties.TryGetValue("Opacity", out string opacityStr))
                                {
                                    if (float.TryParse(opacityStr, out float tileSpecificOpacity))
                                    {
                                        // Combine layer opacity with tile opacity
                                        tileOpacity *= tileSpecificOpacity;
                                    }
                                }
                                break;
                            }
                        }

                        // Position tiles relative to ground level
                        Vector2 position = new(
                            x * tileWidth,    // X position is straightforward left-to-right
                            y * tileHeight    // Y position is top-to-bottom
                        );

                        batch.Draw(
                            info.Texture,
                            position,
                            info.Rectangle,
                            Color.White * tileOpacity,
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
        /// <summary>
        /// A dictionary of objects in this group, keyed by their names.
        /// If duplicate names exist, they are suffixed with a number
        /// </summary>
        public SortedList<string, Object> Objects { get; set; } = new();

        /// <summary>
        /// Custom properties defined for this object group in Tiled
        /// </summary>
        public SortedList<string, string> Properties { get; set; } = new();

        /// <summary>
        /// The name of the object group as defined in Tiled
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The width of the object group in pixels
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the object group in pixels
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The X coordinate of the object group's position in pixels
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The Y coordinate of the object group's position in pixels
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// The opacity of the object group (0.0 to 1.0, where 0 is fully transparent and 1 is fully opaque)
        /// </summary>
        public float Opacity { get; set; } = 1;

        /// <summary>
        /// Loads the object group from a TMX file
        /// </summary>
        /// <param name="reader">A reader to the TMX file being processed</param>
        /// <returns>An initialized ObjectGroup</returns>
        internal static ObjectGroup Load(XmlReader reader)
        {
            ObjectGroup result = new();
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            if (reader.GetAttribute("name") != null)
                result.Name = reader.GetAttribute("name");
            if (reader.GetAttribute("width") != null)
                result.Width = int.Parse(reader.GetAttribute("width") ?? throw new InvalidOperationException());
            if (reader.GetAttribute("height") != null)
                result.Height = int.Parse(reader.GetAttribute("height") ?? throw new InvalidOperationException());
            if (reader.GetAttribute("x") != null)
                result.X = int.Parse(reader.GetAttribute("x") ?? throw new InvalidOperationException());
            if (reader.GetAttribute("y") != null)
                result.Y = int.Parse(reader.GetAttribute("y") ?? throw new InvalidOperationException());
            if (reader.GetAttribute("opacity") != null)
                result.Opacity = float.Parse(reader.GetAttribute("opacity") ?? throw new InvalidOperationException(), NumberStyles.Any, ci);

            while (!reader.EOF)
            {
                string name = reader.Name;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (name)
                        {
                            case "object":
                                {
                                    using XmlReader st = reader.ReadSubtree();
                                    st.Read();
                                    Object objects = Object.Load(st);
                                    if (!result.Objects.TryAdd(objects.Name, objects))
                                    {
                                        int count = result.Objects.Keys.Count((item) => item.Equals(objects.Name));
                                        result.Objects.Add($"{objects.Name}{count}", objects);
                                    }
                                }
                                break;
                            case "properties":
                                {
                                    using XmlReader st = reader.ReadSubtree();
                                    while (!st.EOF)
                                    {
                                        switch (st.NodeType)
                                        {
                                            case XmlNodeType.Element:
                                                if (st.Name == "property")
                                                {
                                                    if (st.GetAttribute("name") != null)
                                                    {
                                                        result.Properties.Add(st.GetAttribute("name") ?? throw new InvalidOperationException(), st.GetAttribute("value"));
                                                    }
                                                }

                                                break;
                                            case XmlNodeType.EndElement:
                                                break;
                                        }

                                        st.Read();
                                    }
                                }
                                break;
                        }

                        break;
                    case XmlNodeType.EndElement:
                        break;
                }

                reader.Read();
            }

            return result;
        }

        public void Draw(TiledMap result, SpriteBatch batch, Rectangle rectangle, Vector2 viewportPosition)
        {
            foreach (Object objects in Objects.Values)
            {
                if (objects.TileTexture != null)
                {
                    objects.Draw(batch, rectangle, new Vector2(X * result.TileWidth, Y * result.TileHeight), viewportPosition, Opacity);
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
        /// <summary>
        /// Custom properties defined for this object in Tiled
        /// </summary>
        public SortedList<string, string> Properties { get; set; } = new();

        /// <summary>
        /// The name of the object as defined in Tiled
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The path to the image file associated with this object, if any
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// The width of the object in pixels
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the object in pixels
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The X coordinate of the object's position in pixels
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The Y coordinate of the object's position in pixels
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// The loaded texture for this object
        /// </summary>
        protected Texture2D Texture { get; set; }

        /// <summary>
        /// The width of the loaded texture in pixels
        /// </summary>
        protected int TexWidth { get; set; }

        /// <summary>
        /// The height of the loaded texture in pixels
        /// </summary>
        protected int TexHeight { get; set; }

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
        /// Loads a map object from a TMX file
        /// </summary>
        /// <param name="reader">A reader to the TMX file being processed</param>
        /// <returns>An anonymous object representing the map</returns>
        internal static Object Load(XmlReader reader)
        {
            Object result = new()
            {
                Name = reader.GetAttribute("name"),
                X = int.Parse(reader.GetAttribute("x") ?? throw new InvalidOperationException()),
                Y = int.Parse(reader.GetAttribute("y") ?? throw new InvalidOperationException())
            };

            /*
             * Height and width are optional on objects
             */
            if (int.TryParse(reader.GetAttribute("width"), out int width))
            {
                result.Width = width;
            }

            if (int.TryParse(reader.GetAttribute("height"), out int height))
            {
                result.Height = height;
            }

            while (!reader.EOF)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "properties")
                        {
                            using XmlReader st = reader.ReadSubtree();
                            while (!st.EOF)
                            {
                                switch (st.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        if (st.Name == "property")
                                        {
                                            if (st.GetAttribute("name") != null)
                                            {
                                                result.Properties.Add(st.GetAttribute("name") ?? throw new InvalidOperationException(), st.GetAttribute("value"));
                                            }
                                        }

                                        break;
                                    case XmlNodeType.EndElement:
                                        break;
                                }

                                st.Read();
                            }
                        }
                        if (reader.Name == "image")
                        {
                            result.Image = reader.GetAttribute("source");
                        }

                        break;
                    case XmlNodeType.EndElement:
                        break;
                }

                reader.Read();
            }

            return result;
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
    /// A class representing a group in a Tiled map
    /// </summary>
    /// <remarks>
    /// Groups can contain layers and object groups, allowing for organizational hierarchy 
    /// within the map structure. Groups can be used to collectively manage multiple layers 
    /// and object groups, such as applying shared properties or handling them as a single 
    /// unit for rendering and manipulation.
    /// </remarks>
    public class Group
    {
        /// <summary>
        /// Collection of object groups within this group, keyed by their names
        /// </summary>
        public SortedList<string, ObjectGroup> ObjectGroups { get; set; }

        /// <summary>
        /// Collection of layers within this group, keyed by their names
        /// </summary>
        public SortedList<string, Layer> Layers { get; set; }

        /// <summary>
        /// Custom properties defined for this group in Tiled
        /// </summary>
        public Dictionary<string, string> Properties { get; set; }

        /// <summary>
        /// The name of the group as defined in Tiled
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The unique identifier of the group
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Whether the group is locked for editing in Tiled
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        /// Loads a group and its contents from a TMX file
        /// </summary>
        /// <param name="reader">The XML reader currently processing the TMX file</param>
        /// <param name="filename">The name of the TMX file being loaded</param>
        /// <returns>An initialized Group containing loaded layers and object groups</returns>
        public Group Load(XmlReader reader, string filename)
        {
            Group group = new()
            {
                ObjectGroups = new SortedList<string, ObjectGroup>(),
                Layers = new SortedList<string, Layer>(),
                Properties = new Dictionary<string, string>(),
                Id = XmlParsingUtilities.ParseIntAttribute(reader, "id"),
                Name = reader.GetAttribute("name") ?? string.Empty,
                Locked = XmlParsingUtilities.ParseBoolAttribute(reader, "locked")
            };

            while (!reader.EOF)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "properties":
                                using (XmlReader st = reader.ReadSubtree())
                                {
                                    st.Read();
                                    XmlParsingUtilities.LoadProperties(st, group.Properties);
                                }
                                break;

                            case "layer":
                                using (XmlReader st = reader.ReadSubtree())
                                {
                                    st.Read();
                                    Layer layer = Layer.Load(st);
                                    if (null != layer)
                                    {
                                        group.Layers[layer.Name] = layer;
                                    }
                                }
                                break;

                            case "objectgroup":
                                using (XmlReader st = reader.ReadSubtree())
                                {
                                    st.Read();
                                    ObjectGroup objectGroup = ObjectGroup.Load(st);
                                    if (objectGroup != null)
                                    {
                                        group.ObjectGroups[objectGroup.Name] = objectGroup;
                                    }
                                }
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "group")
                        {
                            return group;
                        }
                        break;
                }

                reader.Read();
            }

            return group;
        }

        /// <summary>
        /// Draws all contents of the group
        /// </summary>
        /// <param name="result">The TiledMap this group belongs to</param>
        /// <param name="batch">The SpriteBatch to draw with</param>
        /// <param name="visibleArea">The portion of the map currently visible</param>
        /// <param name="viewportPosition">The position of the viewport in the world</param>
        /// <param name="tilesets">Collection of tilesets used by the map</param>
        /// <param name="rectangle">The viewport bounds</param>
        /// <param name="cameraPosition">The current camera position</param>
        /// <param name="tileWidth">The width of a single tile</param>
        /// <param name="tileHeight">The height of a single tile</param>
        public void Draw(TiledMap result, SpriteBatch batch, Rectangle visibleArea, Vector2 viewportPosition, SortedList<string, Tileset> tilesets, Rectangle rectangle, Vector2 cameraPosition, int tileWidth, int tileHeight)
        {
            foreach (ObjectGroup objectGroup in ObjectGroups.Values)
            {
                objectGroup.Draw(result, batch, rectangle, viewportPosition);
            }

            foreach (Layer layer in Layers.Values)
            {
                layer.Draw(batch, tilesets.Values, visibleArea, cameraPosition, tileWidth, tileHeight);
            }
        }
    }

    /// <summary>
    /// A class representing a map created by the Tiled editor
    /// </summary>
    public class TiledMap
    {
        /// <summary>
        /// The Map's Tilesets
        /// </summary>
        public SortedList<string, Tileset> Tilesets { get; set; } = new();

        /// <summary>
        /// The Map's Layers
        /// </summary>
        public SortedList<string, Layer> Layers { get; set; } = new();

        /// <summary>
        /// The Map's Object Groups
        /// </summary>
        public SortedList<string, ObjectGroup> ObjectGroups { get; set; } = new();

        /// <summary>
        /// The Map's Groups
        /// </summary>
        public SortedList<string, Group> Groups { get; set; } = new();

        /// <summary>
        /// The Map's properties
        /// </summary>
        public SortedList<string, string> Properties { get; set; } = new();

        /// <summary>
        /// The Map's width and height
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The Map's width and height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The Map's tile width and height
        /// </summary>
        public int TileWidth { get; set; }

        /// <summary>
        /// The Map's tile width and height
        /// </summary>
        public int TileHeight { get; set; }

        /// <summary>
        /// The tileset's first global id
        /// </summary>
        public Dictionary<string, int> TilesetFirstGid { get; set; } = new();

        /// <summary>
        /// The Map's .tmx file name
        /// </summary>
        public string MapFileName { get; set; }

        /// <summary>
        /// Loads a TMX file into a Map object
        /// </summary>
        /// <param name="filename">The filename of the TMX file</param>
        /// <param name="content">The ContentManager to load textures with</param>
        /// <returns>The loaded map</returns>
        public TiledMap Load(string filename, ContentManager content)
        {
            TiledMap result = new();
            XmlReaderSettings settings = new()
            {
                DtdProcessing = DtdProcessing.Parse
            };

            using (StreamReader stream = File.OpenText(filename))
            using (XmlReader reader = XmlReader.Create(stream, settings))
                while (reader.Read())
                {
                    string name = reader.Name;

                    switch (reader.NodeType)
                    {
                        case XmlNodeType.DocumentType:
                            if (name != "map")
                                throw new Exception("Invalid map format");
                            break;
                        case XmlNodeType.Element:
                            switch (name)
                            {
                                case "map":
                                    {
                                        result.Width = int.Parse(reader.GetAttribute("width") ?? throw new InvalidOperationException());
                                        result.Height = int.Parse(reader.GetAttribute("height") ?? throw new InvalidOperationException());
                                        result.TileWidth = int.Parse(reader.GetAttribute("tilewidth") ?? throw new InvalidOperationException());
                                        result.TileHeight = int.Parse(reader.GetAttribute("tileheight") ?? throw new InvalidOperationException());
                                    }
                                    break;
                                case "tileset":
                                    {
                                        using XmlReader st = reader.ReadSubtree();
                                        st.Read();
                                        Tileset tileset = Tileset.Load(st);
                                        result.Tilesets.Add(tileset.Name, tileset);
                                    }
                                    break;
                                case "layer":
                                    {
                                        using XmlReader st = reader.ReadSubtree();
                                        st.Read();
                                        Layer layer = Layer.Load(st);
                                        if (null != layer)
                                        {
                                            result.Layers.Add(layer.Name, layer);
                                        }
                                    }
                                    break;
                                case "objectgroup":
                                    {
                                        using XmlReader st = reader.ReadSubtree();
                                        st.Read();
                                        ObjectGroup objectgroup = ObjectGroup.Load(st);
                                        result.ObjectGroups.Add(objectgroup.Name, objectgroup);
                                    }
                                    break;
                                case "group":
                                    using (XmlReader st = reader.ReadSubtree())
                                    {
                                        st.Read();
                                        //Group group = LoadGroup(st, filename);
                                        Group group = new();
                                        group = group.Load(st, filename);
                                        result.Groups.Add(group.Name, group);
                                    }
                                    break;
                                case "properties":
                                    {
                                        using XmlReader st = reader.ReadSubtree();
                                        while (!st.EOF)
                                        {
                                            switch (st.NodeType)
                                            {
                                                case XmlNodeType.Element:
                                                    if (st.Name == "property")
                                                    {
                                                        if (st.GetAttribute("name") != null)
                                                        {
                                                            result.Properties.Add(st.GetAttribute("name") ?? throw new InvalidOperationException(), st.GetAttribute("value"));
                                                        }
                                                    }

                                                    break;
                                                case XmlNodeType.EndElement:
                                                    break;
                                            }

                                            st.Read();
                                        }
                                    }
                                    break;
                            }
                            break;
                        case XmlNodeType.EndElement:
                            break;
                        case XmlNodeType.Whitespace:
                            break;
                    }
                }

            foreach (Tileset tileset in result.Tilesets.Values)
            {
                string relativePath = ContentPaths.GetTexturePath(tileset.Image, filename);
                tileset.TileTexture = content.Load<Texture2D>(relativePath);
            }

            foreach (ObjectGroup objects in result.ObjectGroups.Values)
            {
                foreach (Object item in objects.Objects.Values)
                {
                    if (item.Image != null)
                    {
                        string relativePath = ContentPaths.GetTilesetPath(Path.GetFileNameWithoutExtension(item.Image));
                        item.TileTexture = content.Load<Texture2D>(relativePath);
                    }
                }
            }
            MapHelper.AnalyzeMapGround(result);
            return result;
        }

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

            foreach (Group group in Groups.Values)
            {
                group.Draw(this, batch, visibleArea, cameraPosition, Tilesets, visibleArea, cameraPosition, TileWidth, TileHeight);
            }
        }
    }
}
