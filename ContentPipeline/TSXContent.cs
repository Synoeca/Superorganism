using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ContentPipeline
{
    [ContentSerializerRuntimeType("Superorganism.Tiles.Tile, Superorganism")]
    public class TSXTileContent
    {
        public int Id;

        public string Type;

        public float Probability;

        public Dictionary<string, string> Properties;
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.Tileset, Superorganism")]
    public class TSXTilesetContent
    {
        public string Name;

        public int FirstTileId;

        public int TileWidth;

        public int TileHeight;

        public int Spacing;

        public int Margin;

        public Dictionary<int, TSXTileContent> Tiles;

        public string Image;

        public Texture2DContent Texture;

        public int TexWidth;

        public int TexHeight;

        public Texture2DContent TileTexture;

        [ContentSerializerIgnore]
        public string Filename;
    }
}
