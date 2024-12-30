using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ContentPipeline
{
    [ContentSerializerRuntimeType("Superorganism.Tiles.TilemapRuntime, Superorganism")]
    public class TilemapContent
    {
        public Dictionary<string, TilesetContent> Tilesets { get; set; } = new();
        public Dictionary<string, LayerContent> Layers { get; set; } = new();
        public Dictionary<string, ObjectGroupContent> ObjectGroups { get; set; } = new();
        public Dictionary<string, string> Properties { get; set; } = new();

        public int Width { get; set; }
        public int Height { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }

        [ContentSerializerIgnore]
        public string Filename { get; set; }
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.TilesetRuntime, Superorganism")]
    public class TilesetContent
    {
        public int FirstTileId { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int Spacing { get; set; }
        public int Margin { get; set; }
        public Dictionary<int, Dictionary<string, string>> TileProperties { get; set; } = new();
        public Texture2DContent TileTexture { get; set; }
        public int TexWidth { get; set; }
        public int TexHeight { get; set; }

        [ContentSerializerIgnore]
        public string Name { get; set; }

        [ContentSerializerIgnore]
        public string Image { get; set; }
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.LayerRuntime, Superorganism")]
    public class LayerContent
    {
        public Dictionary<string, string> LayerProperties { get; set; } = new();

        [ContentSerializerRuntimeType("Superorganism.Tiles.LayerRuntime+TileInfo, Superorganism")]
        public struct TileInfo
        {
            public Texture2DContent Texture;
            public Rectangle Rectangle;
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public float Opacity { get; set; }
        public int[] Tiles { get; set; }
        public byte[] FlipAndRotate { get; set; }
        public TileInfo[] TileInfoCache { get; set; }

        [ContentSerializerIgnore]
        public string Name { get; set; }
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.ObjectGroupRuntime, Superorganism")]
    public class ObjectGroupContent
    {
        public Dictionary<string, ObjectContent> Objects { get; set; } = new();
        public Dictionary<string, string> ObjectGroupProperties { get; set; } = new();
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public float Opacity { get; set; }

        [ContentSerializerIgnore]
        public string Name { get; set; }
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.ObjectRuntime, Superorganism")]
    public class ObjectContent
    {
        public Dictionary<string, string> ObjectProperties { get; set; } = new();
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Texture2DContent TileTexture { get; set; }

        [ContentSerializerIgnore]
        public string Name { get; set; }

        [ContentSerializerIgnore]
        public string Image { get; set; }
    }
}