using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Superorganism.Tiles;
using Object = Superorganism.Tiles.Object;

namespace Superorganism.Content.PipelineReaders
{
    public class ObjectGroupReader : ContentTypeReader<ObjectGroup>
    {
        protected override ObjectGroup Read(ContentReader input, ObjectGroup existingInstance)
        {
            string name = input.ReadString();
            int width = input.ReadInt32();
            int height = input.ReadInt32();
            int x = input.ReadInt32();
            int y = input.ReadInt32();
            float opacity = input.ReadSingle();
            SortedList<string, string> properties = input.ReadObject<SortedList<string, string>>();
            SortedList<string, Object> objects = input.ReadObject<SortedList<string, Object>>();

            return new ObjectGroup
            {
                Name = name,
                Width = width,
                Height = height,
                X = x,
                Y = y,
                Properties = properties,
                Objects = objects
            };
        }
    }
}