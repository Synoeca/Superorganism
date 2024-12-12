using Microsoft.Xna.Framework.Content;
using System;

namespace ContentPipeline
{
    public static class ContentReaders
    {
        public static void Initialize()
        {
            // Initialize any necessary resources here
        }

        public static void Register(ContentManager contentManager)
        {
            AddTypeReader(typeof(MapReader));
            AddTypeReader(typeof(TilesetReader));
            AddTypeReader(typeof(LayerReader));
            AddTypeReader(typeof(ObjectGroupReader));
            AddTypeReader(typeof(ObjectReader));
        }

        private static void AddTypeReader(Type readerType)
        {
            ContentTypeReaderManager.AddTypeCreator(readerType.FullName, () => (ContentTypeReader)Activator.CreateInstance(readerType));
        }
    }
}