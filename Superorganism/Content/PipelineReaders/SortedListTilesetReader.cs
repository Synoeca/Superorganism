using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Superorganism.Tiles;

namespace Superorganism.Content.PipelineReaders
{
    public class SortedListTilesetReader : ContentTypeReader<SortedList<string, Tileset>>
    {
        protected override SortedList<string, Tileset> Read(ContentReader input, SortedList<string, Tileset> existingInstance)
        {
            int count = input.ReadInt32();
            SortedList<string, Tileset> sortedList = new SortedList<string, Tileset>(count);

            for (int i = 0; i < count; i++)
            {
                string key = input.ReadString();
                Tileset value = input.ReadObject<Tileset>();
                sortedList.Add(key, value);
            }

            return sortedList;
        }
    }
}