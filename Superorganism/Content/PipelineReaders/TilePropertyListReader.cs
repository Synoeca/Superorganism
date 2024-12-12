using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Superorganism.Tiles;

namespace Superorganism.Content.PipelineReaders
{
    public class TilePropertyListReader : ContentTypeReader<Dictionary<int, Tileset.TilePropertyList>>
    {
        protected override Dictionary<int, Tileset.TilePropertyList> Read(ContentReader input, Dictionary<int, Tileset.TilePropertyList> existingInstance)
        {
            int count = input.ReadInt32();
            var dictionary = new Dictionary<int, Tileset.TilePropertyList>(count);

            for (int i = 0; i < count; i++)
            {
                int key = input.ReadInt32();
                Tileset.TilePropertyList value = input.ReadObject<Tileset.TilePropertyList>();
                dictionary.Add(key, value);
            }

            return dictionary;
        }
    }
}