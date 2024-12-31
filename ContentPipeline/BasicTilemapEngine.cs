using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ContentPipeline
{
    [ContentSerializerRuntimeType("Superorganism.Tiles.Map, Superorganism")]
    public class BasicMap
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public int TileWidth { get; set; }
        public int TileHeight { get; set; }

        public Dictionary<string, BasicTileset> Tilesets { get; set; } = new();

        public Dictionary<string, BasicLayer> Layers { get; set; } = new();

        public Dictionary<string, BasicObjectGroup> ObjectGroups { get; set; } = new();

        public Dictionary<string, string> Properties { get; set; } = new();

        [ContentSerializerIgnore]
        public string Filename;
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.Tileset, Superorganism")]
    public class BasicTileset
    {
        [ContentSerializerIgnore]
        public string Name { get; set; }

        public int FirstTileId;

        public int TileWidth;

        public int TileHeight;

        [ContentSerializer(Optional = true)]
        public int Spacing;

        [ContentSerializer(Optional = true)]
        public int Margin;

        public Dictionary<int, Dictionary<string, string>> TileProperties = new();

        public string Image;

        public Texture2DContent Texture { get; set; }

        public int TexWidth { get; set; }

        public int TexHeight { get; set; }

        public Texture2DContent TileTexture { get; set; }
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.Layer, Superorganism")]
    public class BasicLayer
    {
        public Dictionary<string, string> Properties { get; set; } = new();

        [ContentSerializerRuntimeType("Superorganism.Tiles.Layer+TileInfo, Superorganism")]
        public struct TileInfo
        {
            public Texture2DContent Texture;
            public Rectangle Rectangle;
        }


        public string Name;

        public int Width;
        public int Height;

        public float Opacity { get; set; }

        public int[] Tiles;

        public byte[] FlipAndRotate;

        public TileInfo[] TileInfoCache;



        public void BuildTileInfoCache(Dictionary<string, BasicTileset>.ValueCollection tilesets, ContentProcessorContext context)
        {
            Rectangle rect = new();
            List<TileInfo> cache = new();
            int i = 1;

            while (true)
            {
                bool found = false;
                foreach (BasicTileset ts in tilesets)
                {
                    if (MapTileToRect(ts, i, ref rect))
                    {
                        if (ts.Texture == null)
                        {
                            context.Logger.LogWarning("", new ContentIdentity(), "Tileset texture is null for index {0}", i);
                            continue;
                        }

                        cache.Add(new TileInfo
                        {
                            Texture = ts.TileTexture,
                            Rectangle = rect
                        });
                        i++;
                        found = true;
                        break;
                    }
                }
                if (!found) break;
            }

            TileInfoCache = cache.ToArray();
        }

        private bool MapTileToRect(BasicTileset tileset, int index, ref Rectangle rect)
        {
            index -= tileset.FirstTileId;

            if (index < 0)
                return false;

            int rowSize = tileset.TexWidth / (tileset.TileWidth + tileset.Spacing);
            int row = index / rowSize;
            int numRows = tileset.TexHeight / (tileset.TileHeight + tileset.Spacing);

            if (row >= numRows)
                return false;

            int col = index % rowSize;

            rect.X = col * tileset.TileWidth + col * tileset.Spacing + tileset.Margin;
            rect.Y = row * tileset.TileHeight + row * tileset.Spacing + tileset.Margin;
            rect.Width = tileset.TileWidth;
            rect.Height = tileset.TileHeight;

            return true;
        }
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.ObjectGroup, Superorganism")]
    public class BasicObjectGroup
    {
        public Dictionary<string, BasicObject> Objects { get; set; } = new();

        public Dictionary<string, string> Properties { get; set; } = new();



        public int Width;
        public int Height;
        public int X;
        public int Y;
        public float _opacity;

        [ContentSerializerIgnore]
        public string Name;
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.Object, Superorganism")]
    public class BasicObject
    {
        public Dictionary<string, string> Properties = new();

        public int Width;  
        public int Height;
        public int X; 
        public int Y;

        public Texture2DContent Texture;
        public int TexWidth;
        public int TexHeight;

        public Texture2DContent TileTexture { get; set; }

        [ContentSerializerIgnore]
        public string Name;

        [ContentSerializerIgnore]
        public string Image;
    }
}