using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Superorganism.Tiles;

namespace Superorganism.Content.PipelineReaders
{
    public class SortedListObjectGroupReader : ContentTypeReader<SortedList<string, ObjectGroup>>
    {
        protected override SortedList<string, ObjectGroup> Read(ContentReader input, SortedList<string, ObjectGroup> existingInstance)
        {
            int count = input.ReadInt32();
            var sortedList = new SortedList<string, ObjectGroup>(count);

            for (int i = 0; i < count; i++)
            {
                string key = input.ReadString();
                ObjectGroup value = input.ReadObject<ObjectGroup>();
                sortedList.Add(key, value);
            }

            return sortedList;
        }
    }
}