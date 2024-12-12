using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MonoGame.Extended.Content.Pipeline;
using System.Collections.Generic;

namespace ContentPipeline
{


    [ContentSerializerRuntimeType("Superorganism.Tiles.Map, Superorganism")]
    public class BasicMap
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new();
        public Dictionary<string, TilesetContent> Tilesets { get; set; } = new();
        public Dictionary<string, LayerContent> Layers { get; set; } = new();
        public Dictionary<string, ObjectGroupContent> ObjectGroups { get; set; } = new();

        [ContentSerializerIgnore]
        public string Filename { get; set; }
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.Tileset, Superorganism")]
    public class TilesetContent
    {
        public string Name { get; set; }
        public int FirstTileId { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int Spacing { get; set; }
        public int Margin { get; set; }

        public Dictionary<int, TilePropertyListContent> TileProperties { get; set; } = new();
        public Texture2DContent Texture { get; set; }

        [ContentSerializerIgnore]
        public string ImageSource { get; set; }
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.Layer, Superorganism")]
    public class LayerContent
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Opacity { get; set; }
        public SortedList<string, string> Properties { get; set; } = new();
        public int[] TileIndices { get; set; }
        public byte[] FlipAndRotateFlags { get; set; }
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.ObjectGroup, Superorganism")]
    public class ObjectGroupContent
    {
        public string Name { get; set; }
        public Dictionary<string, ObjectContent> Objects { get; set; } = new();
        public SortedList<string, string> Properties { get; set; } = new();
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.Object, Superorganism")]
    public class ObjectContent
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public SortedList<string, string> Properties { get; set; } = new();
        public Texture2DContent Texture { get; set; }

        [ContentSerializerIgnore]
        public string ImageSource { get; set; }
    }

    [ContentSerializerRuntimeType("Superorganism.Tiles.Tileset+TilePropertyList, Superorganism")]
    public class TilePropertyListContent : Dictionary<string, string>
    {
    }
}