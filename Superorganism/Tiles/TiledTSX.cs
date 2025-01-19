using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Tiles
{
    public class TSXTile
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
        public TSXTile()
        {
            Properties = new Dictionary<string, string>();
        }

        /// <summary>
        /// Creates a new Tile instance with the specified ID
        /// </summary>
        /// <param name="id">The tile's ID</param>
        public TSXTile(int id) : this()
        {
            Id = id;
        }

        /// <summary>
        /// Creates a new Tile instance with the specified ID and type
        /// </summary>
        /// <param name="id">The tile's ID</param>
        /// <param name="type">The tile's type</param>
        public TSXTile(int id, string type) : this(id)
        {
            Type = type;
        }

        /// <summary>
        /// Creates a new Tile instance with the specified ID, type, and probability
        /// </summary>
        /// <param name="id">The tile's ID</param>
        /// <param name="type">The tile's type</param>
        /// <param name="probability">The tile's probability of being chosen for random placement</param>
        public TSXTile(int id, string type, float probability) : this(id, type)
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
            return Properties.TryGetValue(propertyName, out string value) ? value : defaultValue;
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

    public class TSXTileset
    {
        /// <summary>
        /// The name of the tileset
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The first tile ID in the tileset
        /// </summary>
        public int FirstTileId { get; set; }

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
        /// Dictionary of all tiles in this tileset, indexed by their local ID (index - FirstTileId)
        /// </summary>
        public Dictionary<int, TSXTile> Tiles { get; set; }

        /// <summary>
        /// The image source path for the tileset
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// The texture containing all tiles
        /// </summary>
        protected Texture2D Texture { get; set; }

        /// <summary>
        /// The width of the tileset texture
        /// </summary>
        protected int TexWidth { get; set; }

        /// <summary>
        /// The height of the tileset texture
        /// </summary>
        protected int TexHeight { get; set; }

        /// <summary>
        /// Initializes a new instance of the Tileset class
        /// </summary>
        public TSXTileset()
        {
            Tiles = new Dictionary<int, TSXTile>();
        }

        /// <summary>
        /// Gets a tile by its global index
        /// </summary>
        /// <param name="index">The global index of the tile</param>
        /// <returns>The tile at the specified index, or null if not found</returns>
        public TSXTile GetTile(int index)
        {
            int localIndex = index - FirstTileId;
            if (localIndex < 0)
                return null;

            Tiles.TryGetValue(localIndex, out TSXTile result);
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
        /// Adds a new tile to the tileset
        /// </summary>
        /// <param name="localId">The local ID of the tile (global ID - FirstTileId)</param>
        /// <param name="tile">The tile to add</param>
        public void AddTile(int localId, TSXTile tile)
        {
            Tiles[localId] = tile;
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
}
