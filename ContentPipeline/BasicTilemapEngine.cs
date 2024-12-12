using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MonoGame.Extended.Content.Pipeline;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using System.Globalization;
using System.IO.Compression;
using Microsoft.Xna.Framework;

namespace ContentPipeline
{
    [ContentSerializerRuntimeType("Superorganism.Tiles.Tileset, Superorganism")]
    public class BasicTileset
    {
        [ContentSerializerIgnore]
        public string Name;

        [ContentSerializer(ElementName = "FirstTileId")]
        public int FirstTileId;

        [ContentSerializer(ElementName = "TileWidth")]
        public int TileWidth;

        [ContentSerializer(ElementName = "TileHeight")]
        public int TileHeight;

        [ContentSerializer(ElementName = "Spacing")]
        public int Spacing;

        [ContentSerializer(ElementName = "Margin")]
        public int Margin;

        // This is the key change - make it more explicit
        [ContentSerializer(ElementName = "Properties", Optional = true)]
        private Dictionary<int, Dictionary<string, string>> _tileProperties = new();

        [ContentSerializerIgnore]
        public Dictionary<int, Dictionary<string, string>> TileProperties
        {
            get => _tileProperties;
            set => _tileProperties = value ?? new Dictionary<int, Dictionary<string, string>>();
        }

        [ContentSerializerIgnore]
        public string Image;

        [ContentSerializer(ElementName = "Texture")]
        public Texture2DContent TileTexture { get; set; }

        [ContentSerializer(ElementName = "TexWidth")]
        public int TexWidth { get; set; }

        [ContentSerializer(ElementName = "TexHeight")]
        public int TexHeight { get; set; }
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.Layer, Superorganism")]
    public class BasicLayer
    {
        [ContentSerializer]
        public Dictionary<string, string> Properties { get; set; } = new();

        [ContentSerializerRuntimeType("Superorganism.Tiles.Layer+TileInfo, Superorganism")]
        public struct TileInfo
        {
            public Texture2DContent Texture;
            public Rectangle Rectangle;
        }

        [ContentSerializerIgnore]
        public string Name;

        [ContentSerializer]
        public int Width { get; set; }
        [ContentSerializer]
        public int Height { get; set; }
        [ContentSerializer]
        public float Opacity { get; set; }

        [ContentSerializer]
        public int[] Tiles;
        [ContentSerializer]
        public byte[] FlipAndRotateFlags;

        [ContentSerializerIgnore]
        public TileInfo[] TileInfoCache;
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.ObjectGroup, Superorganism")]
    public class BasicObjectGroup
    {
        [ContentSerializer]
        public Dictionary<string, BasicObject> Objects { get; set; } = new();
        [ContentSerializer]
        public Dictionary<string, string> Properties { get; set; } = new();

        [ContentSerializerIgnore]
        public string Name;

        public int Width;
        public int Height;
        public int X;
        public int Y;
        public float _opacity;
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.Object, Superorganism")]
    public class BasicObject
    {
        [ContentSerializer]
        public Dictionary<string, string> Properties = new();

        [ContentSerializerIgnore]
        public string Name;

        [ContentSerializerIgnore]
        public string Image;

        public int Width;  
        public int Height;
        public int X; 
        public int Y;

        public Texture2DContent Texture;
        public int TexWidth;
        public int TexHeight;

        public Texture2DContent TileTexture { get; set; }
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.Map, Superorganism")]
    public class BasicMap
    {
        [ContentSerializer]
        public Dictionary<string, BasicTileset> Tilesets { get; set; } = new();

        [ContentSerializer]
        public Dictionary<string, BasicLayer> Layers { get; set; } = new();

        [ContentSerializer]
        public Dictionary<string, BasicObjectGroup> ObjectGroups { get; set; } = new();

        [ContentSerializer]
        public Dictionary<string, string> Properties { get; set; } = new();

        [ContentSerializer]
        public int Width;

        [ContentSerializer]
        public int Height;

        [ContentSerializer]
        public int TileWidth;

        [ContentSerializer]
        public int TileHeight;

        [ContentSerializerIgnore]
        public string Filename;
    }
}