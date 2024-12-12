using Microsoft.Xna.Framework.Content;
using Superorganism.Tiles;

namespace ContentPipeline
{
    public class MapReader : ContentTypeReader<Map>
    {
        protected override Map Read(ContentReader input, Map existingInstance)
        {
            string mapFilePath = input.ReadString();
            ContentManager content = input.ContentManager;
            return Map.Load(mapFilePath, content);
        }
    }
}