using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentPipeline
{
    public class BasicMapContent
    {
        /// <summary> The Map's Tilesets </summary>
        public SortedList<string, BasicTilesetContent> Tilesets = new();

        /// <summary> The Map's Layers </summary>
        public SortedList<string, BasicLayerContent> Layers = new();

        /// <summary> The Map's Object Groups </summary>
        public SortedList<string, BasicObjectGroupContent> ObjectGroups = new();

        /// <summary> The Map's properties </summary>
        public SortedList<string, string> Properties = new();

        /// <summary>The Map's width and height</summary>
        public int Width, Height;

        /// <summary>Map's tile width and height</summary>
        public int TileWidth, TileHeight;

        /// <summary>The map filename</summary>
        [ContentSerializerIgnore]
        public string FileName;
    }
}
