using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Superorganism.Tiles;
using Object = Superorganism.Tiles.Object;

namespace Superorganism.Content.PipelineReaders
{
    public class SortedListObjectReader : ContentTypeReader<SortedList<string, Object>>
    {
        protected override SortedList<string, Object> Read(ContentReader input, SortedList<string, Object> existingInstance)
        {
            int count = input.ReadInt32();
            SortedList<string, Object> sortedList = new SortedList<string, Object>(count);

            for (int i = 0; i < count; i++)
            {
                string key = input.ReadString();
                Object value = input.ReadObject<Object>();
                sortedList.Add(key, value);
            }

            return sortedList;
        }
    }
}