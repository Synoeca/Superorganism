using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ContentPipeline
{
    [ContentImporter(".tsx", DisplayName = "TSXImporter", DefaultProcessor = "TSXProcessor")]
    public class TSXImporter : ContentImporter<TSXTilesetContent>
    {
        public override TSXTilesetContent Import(string filename, ContentImporterContext context)
        {
            context.Logger.LogMessage($"=== Starting TSX Import for {filename} ===");

            TSXTilesetContent result = new()
            {
                Filename = Path.GetFullPath(filename),
                Tiles = new Dictionary<int, TSXTileContent>()
            };

            XmlReaderSettings settings = new()
            {
                DtdProcessing = DtdProcessing.Parse
            };

            using StreamReader stream = File.OpenText(filename);
            using XmlReader reader = XmlReader.Create(stream, settings);

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "tileset")
                {
                    // Read basic tileset attributes
                    result.Name = reader.GetAttribute("name");
                    result.TileWidth = ParseIntAttribute(reader, "tilewidth");
                    result.TileHeight = ParseIntAttribute(reader, "tileheight");
                    result.Spacing = ParseIntAttribute(reader, "spacing");
                    result.Margin = ParseIntAttribute(reader, "margin");

                    int tileCount = ParseIntAttribute(reader, "tilecount");

                    // Initialize tiles with default values
                    for (int i = 0; i < tileCount; i++)
                    {
                        result.Tiles[i] = new TSXTileContent
                        {
                            Id = i,
                            Type = null,
                            Probability = 1.0f,
                            Properties = new Dictionary<string, string>()
                        };
                    }

                    // Process child elements (image and tiles)
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            switch (reader.Name)
                            {
                                case "image":
                                    result.Image = reader.GetAttribute("source");
                                    result.TexWidth = ParseIntAttribute(reader, "width");
                                    result.TexHeight = ParseIntAttribute(reader, "height");
                                    break;

                                case "tile":
                                    int tileId = ParseIntAttribute(reader, "id");
                                    TSXTileContent tile = new()
                                    {
                                        Id = tileId,
                                        Type = reader.GetAttribute("type"),
                                        Probability = ParseFloatAttribute(reader, "probability", 1.0f),
                                        Properties = new Dictionary<string, string>()
                                    };

                                    // Read tile properties
                                    using (XmlReader tileReader = reader.ReadSubtree())
                                    {
                                        while (tileReader.Read())
                                        {
                                            if (tileReader.NodeType == XmlNodeType.Element &&
                                                tileReader.Name == "properties")
                                            {
                                                using XmlReader propsReader = tileReader.ReadSubtree();
                                                while (propsReader.Read())
                                                {
                                                    if (propsReader.NodeType == XmlNodeType.Element &&
                                                        propsReader.Name == "property")
                                                    {
                                                        string propName = propsReader.GetAttribute("name");
                                                        string propValue = propsReader.GetAttribute("value");
                                                        if (!string.IsNullOrEmpty(propName))
                                                        {
                                                            tile.Properties[propName] = propValue;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    result.Tiles[tileId] = tile;
                                    break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static int ParseIntAttribute(XmlReader reader, string attributeName, int defaultValue = 0)
        {
            string value = reader.GetAttribute(attributeName);
            return !string.IsNullOrEmpty(value) && int.TryParse(value, out int result) ? result : defaultValue;
        }

        private static float ParseFloatAttribute(XmlReader reader, string attributeName, float defaultValue = 0.0f)
        {
            string value = reader.GetAttribute(attributeName);
            return !string.IsNullOrEmpty(value) && float.TryParse(value, out float result) ? result : defaultValue;
        }
    }
}
