using System;
using ContentPipeline;
using Microsoft.Xna.Framework.Content;

namespace Superorganism.Content.PipelineReaders
{
    public static class ContentReaders
    {
        public static void Initialize()
        {
            // Initialize any necessary resources here
        }

        public static void Register(ContentManager contentManager)
        {
            AddTypeReader(typeof(SortedListStringTilesetContentReader));
            AddTypeReader(typeof(SortedListStringLayerContentReader));
            AddTypeReader(typeof(SortedListStringObjectGroupContentReader));
        }

        private static void AddTypeReader(Type readerType)
        {
            ContentTypeReaderManager.AddTypeCreator(readerType.FullName, () => (ContentTypeReader)Activator.CreateInstance(readerType));
        }
    }
}