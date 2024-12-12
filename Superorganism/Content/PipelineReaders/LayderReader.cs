using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Superorganism.Tiles;

namespace Superorganism.Content.PipelineReaders
{
    public class LayerReader : ContentTypeReader<Layer>
    {
        protected override Layer Read(ContentReader input, Layer existingInstance)
        {
            string name = input.ReadString();
            int width = input.ReadInt32();
            int height = input.ReadInt32();
            float opacity = input.ReadSingle();
            SortedList<string, string> properties = input.ReadObject<SortedList<string, string>>();
            int[] tiles = input.ReadObject<int[]>();
            byte[] flipAndRotate = input.ReadObject<byte[]>();

            return new Layer
            {
                Name = name,
                Width = width,
                Height = height,
                Opacity = opacity,
                Properties = properties,
                Tiles = tiles,
                FlipAndRotate = flipAndRotate
            };
        }
    }
}