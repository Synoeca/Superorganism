using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MonoGame.Extended.Content.Pipeline.Tiled;
using System.Xml;
using static ContentPipeline.BasicTileset;

namespace ContentPipeline
{
    [ContentImporter(".tmx", DefaultProcessor = "TilemapProcessor", DisplayName = "Tiled Map Importer")]
    public class TiledMapImporter : ContentImporter<BasicMap>
    {
        public override BasicMap Import(string filename, ContentImporterContext context)
        {
            BasicMap map = new() { Filename = Path.GetFullPath(filename) };

            XmlReaderSettings settings = new()
            {
                DtdProcessing = DtdProcessing.Parse
            };

            using StreamReader stream = File.OpenText(filename);
            using XmlReader reader = XmlReader.Create(stream, settings);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "map":
                                ImportMapAttributes(reader, map);
                                break;
                            case "tileset":
                                using (XmlReader? st = reader.ReadSubtree())
                                {
                                    st.Read();
                                    ImportTileset(st, map);
                                }
                                break;
                            case "layer":
                                using (XmlReader? st = reader.ReadSubtree())
                                {
                                    st.Read();
                                    ImportLayer(st, map);
                                }
                                break;
                            case "objectgroup":
                                using (XmlReader? st = reader.ReadSubtree())
                                {
                                    st.Read();
                                    ImportObjectGroup(st, map);
                                }
                                break;
                            case "properties":
                                ImportProperties(reader, map.Properties);
                                break;
                        }
                        break;
                }
            }

            return map;
        }

        private void ImportMapAttributes(XmlReader reader, BasicMap map)
        {
            map.Width = int.Parse(reader.GetAttribute("width") ?? "0");
            map.Height = int.Parse(reader.GetAttribute("height") ?? "0");
            map.TileWidth = int.Parse(reader.GetAttribute("tilewidth") ?? "0");
            map.TileHeight = int.Parse(reader.GetAttribute("tileheight") ?? "0");
        }

        private void ImportTileset(XmlReader reader, BasicMap map)
        {
            BasicTileset tileset = new()
            {
                Name = reader.GetAttribute("name") ?? "",
                FirstTileId = int.Parse(reader.GetAttribute("firstgid") ?? "1"),
                TileWidth = int.Parse(reader.GetAttribute("tilewidth") ?? "0"),
                TileHeight = int.Parse(reader.GetAttribute("tileheight") ?? "0"),
                Spacing = int.Parse(reader.GetAttribute("spacing") ?? "0"),
                Margin = int.Parse(reader.GetAttribute("margin") ?? "0")
            };

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "image":
                            tileset.Image = reader.GetAttribute("source") ?? "";
                            break;

                        case "tile":
                            int id = int.Parse(reader.GetAttribute("id") ?? "-1");
                            if (id >= 0)
                            {
                                using XmlReader? tileReader = reader.ReadSubtree();
                                while (tileReader.Read())
                                {
                                    if (tileReader.NodeType == XmlNodeType.Element &&
                                        tileReader.Name == "properties")
                                    {
                                        SortedList<string, string> properties = PropertyImporter.ImportProperties(tileReader);
                                        if (!tileset.TileProperties.TryGetValue(id, out BasicTileset.BasicTilePropertyList? tilePropertyList))
                                        {
                                            tilePropertyList = new BasicTilePropertyList();
                                            tileset.TileProperties[id] = tilePropertyList;
                                        }
                                        foreach (KeyValuePair<string, string> kvp in properties)
                                        {
                                            tilePropertyList[kvp.Key] = kvp.Value;
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            map.Tilesets.Add(tileset.Name, tileset);
        }

        private void ImportObjectGroup(XmlReader reader, BasicMap map)
        {
            BasicObjectGroup objectGroup = new()
            {
                Name = reader.GetAttribute("name") ?? ""
            };

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "object":
                            ImportObject(reader, objectGroup);
                            break;

                        case "properties":
                            objectGroup.Properties = PropertyImporter.ImportProperties(reader);
                            break;
                    }
                }
            }

            map.ObjectGroups.Add(objectGroup.Name, objectGroup);
        }

        private void ImportObject(XmlReader reader, BasicObjectGroup objectGroup)
        {
            BasicObject obj = new()
            {
                Name = reader.GetAttribute("name") ?? "",
                X = int.Parse(reader.GetAttribute("x") ?? "0"),
                Y = int.Parse(reader.GetAttribute("y") ?? "0")
            };

            // Width and height are optional for objects
            string? width = reader.GetAttribute("width");
            string? height = reader.GetAttribute("height");

            if (width != null) obj.Width = int.Parse(width);
            if (height != null) obj.Height = int.Parse(height);

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "properties":
                            obj.Properties = PropertyImporter.ImportProperties(reader);
                            break;

                        case "image":
                            obj.Image = reader.GetAttribute("source") ?? throw new InvalidOperationException();
                            break;
                    }
                }
            }

            objectGroup.Objects.Add(obj.Name, obj);
        }

        private void ImportLayer(XmlReader reader, BasicMap map)
        {
            BasicLayer layer = new()
            {
                Name = reader.GetAttribute("name") ?? "",
                Width = int.Parse(reader.GetAttribute("width") ?? "0"),
                Height = int.Parse(reader.GetAttribute("height") ?? "0"),
                Opacity = float.Parse(reader.GetAttribute("opacity") ?? "1.0")
            };

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "data":
                            LayerDataHandler.DecodeLayerData(reader, layer);
                            break;

                        case "properties":
                            layer.Properties = PropertyImporter.ImportProperties(reader);
                            break;
                    }
                }
            }

            map.Layers.Add(layer.Name, layer);
        }

        private void ImportProperties(XmlReader reader, SortedList<string, string> properties)
        {
            using XmlReader? subtree = reader.ReadSubtree();
            while (subtree.Read())
            {
                if (subtree.NodeType == XmlNodeType.Element && subtree.Name == "property")
                {
                    string name = subtree.GetAttribute("name") ?? throw new ContentLoadException("Property missing name attribute");
                    string value = subtree.GetAttribute("value") ?? string.Empty;
                    properties[name] = value;
                }
            }
        }
    }
}