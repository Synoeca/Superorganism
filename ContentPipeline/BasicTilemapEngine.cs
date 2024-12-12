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
        [ContentSerializerRuntimeType("Superorganism.Tiles.Tileset.TilePropertyList, Superorganism")]
        public class BasicTilePropertyList : Dictionary<string, string> { }

        [ContentSerializerIgnore]
        public string Name;
        public int FirstTileId;
        public int TileWidth;
        public int TileHeight;
        public int Spacing;
        public int Margin;

        [ContentSerializer]
        public Dictionary<int, Dictionary<string, string>> TileProperties { get; set; } = new();

        [ContentSerializerIgnore]
        public string Image;

        public Texture2DContent Texture;

        public int TexWidth;
        public int TexHeight;

        public Texture2DContent TileTexture { get; set; }
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.Layer, Superorganism")]
    public class BasicLayer
    {
        public const uint FlippedHorizontallyFlag = 0x80000000;
        public const uint FlippedVerticallyFlag = 0x40000000;
        public const uint FlippedDiagonallyFlag = 0x20000000;

        public const byte HorizontalFlipDrawFlag = 1;
        public const byte VerticalFlipDrawFlag = 2;
        public const byte DiagonallyFlipDrawFlag = 4;

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

        public int Width;
        public int Height;
        public float Opacity;
        [ContentSerializer]
        public int[] Tiles;
        [ContentSerializer]
        public byte[] FlipAndRotateFlags;
        [ContentSerializer]
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