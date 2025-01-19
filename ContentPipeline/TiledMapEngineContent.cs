using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ContentPipeline
{
    [ContentSerializerRuntimeType("Superorganism.Tiles.TiledMap, Superorganism")]
    public class TiledMapEngineContent
    {
        public int Width;

        public int Height;

        public int TileWidth;

        public int TileHeight;

        [ContentSerializerIgnore]
        public string Filename;

        public Dictionary<string, string> Properties;

        public Dictionary<string, GroupContent> Groups;

        public Dictionary<string, LayerContent> Layers;


        public Dictionary<string, TilesetContent> Tilesets;
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.Tile, Superorganism")]
    public class TileContent
    {
        public int Id;
        
        public string Type;

        public float Probability;

        public Dictionary<string, string> Properties;
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.Tileset, Superorganism")]
    public class TilesetContent
    {
        public string Name;

        public int FirstTileId;

        public int TileWidth;

        public int TileHeight;

        public int Spacing;

        public int Margin;

        public Dictionary<int, TileContent> Tiles;

        public string Image;

        public Texture2DContent Texture;

        public int TexWidth;

        public int TexHeight;

        public Texture2DContent TileTexture;

        [ContentSerializerIgnore]
        public string Filename;
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.Layer, Superorganism")]
    public class LayerContent
    {
        public static uint FlippedHorizontallyFlag;
        public static uint FlippedVerticallyFlag;
        public static uint FlippedDiagonallyFlag;

        public static byte HorizontalFlipDrawFlag;
        public static byte VerticalFlipDrawFlag;
        public static byte DiagonallyFlipDrawFlag;

        public SortedList<string, string> Properties;

        [ContentSerializerRuntimeType("Superorganism.Tiles.Layer+TileInfo, Superorganism")]
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

    [ContentSerializerRuntimeType("Superorganism.Tiles.Object, Superorganism")]
    public class ObjectContent
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

    [ContentSerializerRuntimeType("Superorganism.Tiles.ObjectGroup, Superorganism")]
    public class ObjectGroupContent
    {
        public Dictionary<string, ObjectContent> Objects;
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

    [ContentSerializerRuntimeType("Superorganism.Tiles.Group, Superorganism")]
    public class GroupContent
    {
        public Dictionary<string, ObjectGroupContent> ObjectGroups;
        public Dictionary<string, LayerContent> Layers;
        public Dictionary<string, string> Properties;
        public string Name;
        public int Id;
        public bool Locked;
        [ContentSerializerIgnore]
        public string Filename;
    }
}
