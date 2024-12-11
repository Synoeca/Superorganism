using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace ContentPipeline
{
    [ContentSerializerRuntimeType("Superorganism.Tiles.Map, Superorganism")]
    public class BasicTilemapContent
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public SortedList<string, string> Properties { get; set; } = new();
        public SortedList<string, Layer> Layers { get; set; } = new();
        public SortedList<string, Tileset> Tilesets { get; set; } = new();

        [ContentSerializerIgnore]
        public string MapFilename { get; set; } = null!;
    }

    public class Layer
    {
        public string Name { get; set; } = null!;
        public int Width { get; set; }
        public int Height { get; set; }
        public float Opacity { get; set; }
        public int[] Tiles { get; set; } = null!;
        public byte[] FlipAndRotate { get; set; } = null!;
        public SortedList<string, string> Properties { get; set; } = new();
    }

    public class Tileset
    {
        public string Name { get; set; } = null!;
        public int FirstTileId { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int Spacing { get; set; }
        public int Margin { get; set; }
        public Dictionary<int, Dictionary<string, string>> TileProperties { get; set; } = new();
        public Texture2DContent TileTexture { get; set; } = null!;

        [ContentSerializerIgnore]
        public string ImagePath { get; set; } = null!;
    }
}
