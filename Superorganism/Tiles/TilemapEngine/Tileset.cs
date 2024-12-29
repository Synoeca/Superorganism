using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Tiles.TilemapEngine
{
    /// <summary>
    /// A class representing a TileSet created with the Tiled map editor.
    /// </summary>
    public class Tileset
    {
        /// <summary>
        /// A class for holding a list of tile properties 
        /// </summary>
        /// <remarks>
        /// Essentially, a &lt;string, string&gt; dictionary
        /// </remarks>
        public class TilePropertyList : Dictionary<string, string>;

        public string Name;
        public int FirstTileId;
        public int TileWidth;
        public int TileHeight;
        public int Spacing;
        public int Margin;
        public Dictionary<int, TilePropertyList> TileProperties = new();
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
        /// Loads a Tileset from a TMX file
        /// </summary>
        /// <param name="reader">A reader processing the file</param>
        /// <returns>An initialized Tileset object</returns>
        internal static Tileset Load(XmlReader reader)
        {
            Tileset result = new()
            {
                Name = reader.GetAttribute("name"),
                FirstTileId = ParseIntAttribute(reader, "firstgid"),
                TileWidth = ParseIntAttribute(reader, "tilewidth"),
                TileHeight = ParseIntAttribute(reader, "tileheight"),
                Margin = ParseIntAttribute(reader, "margin"),
                Spacing = ParseIntAttribute(reader, "spacing")
            };

            int currentTileId = -1;

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
                                currentTileId = int.Parse(reader.GetAttribute("id") ?? throw new InvalidOperationException());
                                break;
                            case "property":
                                {
                                    if (!result.TileProperties.TryGetValue(currentTileId, out TilePropertyList props))
                                    {
                                        props = new TilePropertyList();
                                        result.TileProperties[currentTileId] = props;
                                    }

                                    props[reader.GetAttribute("name") ?? throw new InvalidOperationException()] = reader.GetAttribute("value");
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
        /// Gets the properties of the specified tile
        /// </summary>
        /// <param name="index">The index of the tile</param>
        /// <returns>A TilePropertyList for the tile</returns>
        public TilePropertyList GetTileProperties(int index)
        {
            index -= FirstTileId;

            if (index < 0)
                return null;

            TileProperties.TryGetValue(index, out TilePropertyList result);

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
}
