using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using MonoGame.Extended.Collisions.Layers;

namespace ContentPipeline
{
    public class SortedListStringLayerContentReader : ContentTypeReader<SortedList<string, Layer>>
    {
        protected override SortedList<string, Layer> Read(ContentReader input, SortedList<string, Layer> existingInstance)
        {
            int count = input.ReadInt32();
            SortedList<string, Layer> sortedList = new SortedList<string, Layer>(count);

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