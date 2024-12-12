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
        public Dictionary<int, BasicTilePropertyList> TileProperties = new();

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

        public SortedList<string, string> Properties = new();
        public struct TileInfo
        {
            public Texture2D Texture;
            public Rectangle Rectangle;
        }

        [ContentSerializerIgnore]
        public string Name;

        public int Width;
        public int Height;
        public float Opacity;
        public int[] Tiles;
        public byte[] FlipAndRotateFlags;
        public TileInfo[] TileInfoCache;
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.ObjectGroup, Superorganism")]
    public class BasicObjectGroup
    {
        [ContentSerializer]
        public SortedList<string, BasicObject> Objects = new();
        [ContentSerializer]
        public SortedList<string, string> Properties = new();

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
        public SortedList<string, string> Properties = new();

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
        public SortedList<string, BasicTileset> Tilesets = new();
        [ContentSerializer]
        public SortedList<string, BasicLayer> Layers = new();
        [ContentSerializer]
        public SortedList<string, BasicObjectGroup> ObjectGroups = new();
        [ContentSerializer]
        public SortedList<string, string> Properties = new();

        public int Width; 
        public int Height;
        public int TileWidth;
        public int TileHeight;

        [ContentSerializerIgnore]
        public string Filename;
    }
}