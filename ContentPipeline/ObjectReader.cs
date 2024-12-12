using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Tiles;
using Object = Superorganism.Tiles.Object;

namespace ContentPipeline
{
    public class ObjectReader : ContentTypeReader<Object>
    {
        protected override Object Read(ContentReader input, Object existingInstance)
        {
            string name = input.ReadString();
            int width = input.ReadInt32();
            int height = input.ReadInt32();
            int x = input.ReadInt32();
            int y = input.ReadInt32();
            Dictionary<string, string> properties = input.ReadObject<Dictionary<string, string>>();
            Texture2D texture = input.ReadExternalReference<Texture2D>();

            return new Object
            {
                Name = name,
                Width = width,
                Height = height,
                X = x,
                Y = y,
                Properties = properties,
                TileTexture = texture
            };
        }
    }
}