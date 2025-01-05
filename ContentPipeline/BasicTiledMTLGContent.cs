using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content;
using Color = Microsoft.Xna.Framework.Color;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ContentPipeline
{
    [ContentSerializerRuntimeType("Superorganism.Tiles.BasicTiledMTLG, Superorganism")]
    public class BasicTiledMTLGContent
    {
        public int Width;

        public int Height;

        public int TileWidth;

        public int TileHeight;

        [ContentSerializerIgnore]
        public string Filename;

        public Dictionary<string, string> Properties;

        public Dictionary<string, BasicTiledGroupMTLGContent> Groups;

        public Dictionary<string, BasicLayerMTLGContent> Layers;

        public Dictionary<string, BasicTilesetMTLGContent> Tilesets;
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.BasicTilesetMTLG, Superorganism")]
    public class BasicTilesetMTLGContent
    {
        [ContentSerializerRuntimeType("Superorganism.Tiles.BasicTilesetMTLG+TilePropertyList, Superorganism")]
        public class TilePropertyList : Dictionary<string, string>;

        public string Name;

        public int FirstTileId;

        public int TileWidth;

        public int TileHeight;

        public int Spacing;

        public int Margin;

        public Dictionary<int, TilePropertyList> TileProperties;

        public string Image;

        public Texture2DContent Texture;

        public int TexWidth;

        public int TexHeight;

        public Texture2DContent TileTexture;

        [ContentSerializerIgnore]
        public string Filename;
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.BasicLayerMTLG, Superorganism")]
    public class BasicLayerMTLGContent
    {
        public static uint FlippedHorizontallyFlag;
        public static uint FlippedVerticallyFlag;
        public static uint FlippedDiagonallyFlag;

        public static byte HorizontalFlipDrawFlag;
        public static byte VerticalFlipDrawFlag;
        public static byte DiagonallyFlipDrawFlag;

        public SortedList<string, string> Properties;

        [ContentSerializerRuntimeType("Superorganism.Tiles.BasicLayerMTLG+TileInfo, Superorganism")]
        public struct TileInfo
        {
            public Texture2DContent Texture;
            public Rectangle Rectangle;
        }

        public string Name;

        public int Width;

        public int Height;

        public float Opacity;

        public int[] Tiles;

        public byte[] FlipAndRotate;

        public TileInfo[] TileInfoCache;

        [ContentSerializerIgnore]
        public string Filename;
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.BasicTiledObjectMTLG, Superorganism")]
    public class BasicTiledObjectMTLGContent
    {
        public Dictionary<string, string> Properties;

        public string Name;
        public string Image;
        public int Width;
        public int Height;
        public int X;
        public int Y;

        public Texture2DContent Texture;
        public int TexWidth;
        public int TexHeight;

        public Texture2DContent TileTexture;
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.BasicTiledObjectGroupMTLG, Superorganism")]
    public class BasicTiledObjectGroupMTLGContent
    {
        public Dictionary<string, BasicTiledObjectMTLGContent> Objects;
        public Dictionary<string, string> ObjectProperties;

        public string Name;
        public string Class;
        public float Opacity;
        public float OffsetX;
        public float OffsetY;
        public float ParallaxX;
        public float ParallaxY;
        public Color? Color;
        public Color? TintColor;
        public bool Visible;
        public bool Locked;
        public int Id;
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.BasicTiledGroupMTLG, Superorganism")]
    public class BasicTiledGroupMTLGContent
    {
        public Dictionary<string, BasicTiledObjectGroupMTLGContent> ObjectGroups;
        public Dictionary<string, string> Properties;
        public string Name;
        public int Id;
        public bool Locked;
        [ContentSerializerIgnore]
        public string Filename;
    }
}
