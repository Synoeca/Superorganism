using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace Superorganism.Content.PipelineReaders
{
    public class SortedListStringReader : ContentTypeReader<SortedList<string, string>>
    {
        protected override SortedList<string, string> Read(ContentReader input, SortedList<string, string> existingInstance)
        {
            int count = input.ReadInt32();
            var sortedList = new SortedList<string, string>(count);

            for (int i = 0; i < count; i++)
            {
                string key = input.ReadString();
                string value = input.ReadString();
                sortedList.Add(key, value);
            }

            return sortedList;
        }
    }
}