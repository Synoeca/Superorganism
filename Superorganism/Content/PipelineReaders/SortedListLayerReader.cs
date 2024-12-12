using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Superorganism.Tiles;

namespace Superorganism.Content.PipelineReaders
{
    public class SortedListLayerReader : ContentTypeReader<SortedList<string, Layer>>
    {
        protected override SortedList<string, Layer> Read(ContentReader input, SortedList<string, Layer> existingInstance)
        {
            int count = input.ReadInt32();
            var sortedList = new SortedList<string, Layer>(count);

            for (int i = 0; i < count; i++)
            {
                string key = input.ReadString();
                Layer value = input.ReadObject<Layer>();
                sortedList.Add(key, value);
            }

            return sortedList;
        }
    }
}