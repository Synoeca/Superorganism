using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Extended.Content.Pipeline;

namespace ContentPipeline
{
    [ContentImporter(".tmx", DisplayName = "Tiled Map Importer", DefaultProcessor = "TiledMapProcessor")]
    public class TiledMapImporter : ContentImporter<BasicTilemapContent>
    {
        public override BasicTilemapContent Import(string filename, ContentImporterContext context)
        {
            BasicTilemapContent map = new()
            {
                MapFilename = filename
            };

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
                                map.Width = int.Parse(reader.GetAttribute("width") ?? throw new InvalidOperationException());
                                map.Height = int.Parse(reader.GetAttribute("height") ?? throw new InvalidOperationException());
                                map.TileWidth = int.Parse(reader.GetAttribute("tilewidth") ?? throw new InvalidOperationException());
                                map.TileHeight = int.Parse(reader.GetAttribute("tileheight") ?? throw new InvalidOperationException());
                                break;

                            case "tileset":
                                using (XmlReader tilesetReader = reader.ReadSubtree())
                                {
                                    Tileset tileset = ImportTileset(tilesetReader);
                                    map.Tilesets.Add(tileset.Name, tileset);
                                }
                                break;

                            case "layer":
                                using (XmlReader layerReader = reader.ReadSubtree())
                                {
                                    Layer layer = ImportLayer(layerReader);
                                    map.Layers.Add(layer.Name, layer);
                                }
                                break;

                            case "properties":
                                using (XmlReader propsReader = reader.ReadSubtree())
                                {
                                    ImportProperties(propsReader, map.Properties);
                                }
                                break;
                        }
                        break;
                }
            }

            return map;
        }

        private Tileset ImportTileset(XmlReader reader)
        {
            Tileset tileset = new()
            {
                Name = reader.GetAttribute("name") ?? throw new InvalidOperationException(),
                FirstTileId = int.Parse(reader.GetAttribute("firstgid") ?? throw new InvalidOperationException()),
                TileWidth = int.Parse(reader.GetAttribute("tilewidth") ?? throw new InvalidOperationException()),
                TileHeight = int.Parse(reader.GetAttribute("tileheight") ?? throw new InvalidOperationException())
            };

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "image")
                {
                    tileset.ImagePath = reader.GetAttribute("source") ?? throw new InvalidOperationException();
                }
            }

            return tileset;
        }

        private Layer ImportLayer(XmlReader reader)
        {
            Layer layer = new()
            {
                Name = reader.GetAttribute("name") ?? throw new InvalidOperationException(),
                Width = int.Parse(reader.GetAttribute("width") ?? throw new InvalidOperationException()),
                Height = int.Parse(reader.GetAttribute("height") ?? throw new InvalidOperationException()),
                Opacity = reader.GetAttribute("opacity") != null ? float.Parse(reader.GetAttribute("opacity") ?? throw new InvalidOperationException()) : 1.0f
            };

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "data":
                            ImportLayerData(reader, layer);
                            break;
                        case "properties":
                            using (XmlReader propsReader = reader.ReadSubtree())
                            {
                                ImportProperties(propsReader, layer.Properties);
                            }
                            break;
                    }
                }
            }

            return layer;
        }

        private void ImportLayerData(XmlReader reader, Layer layer)
        {
            string encoding = reader.GetAttribute("encoding") ?? throw new InvalidOperationException();
            string compression = reader.GetAttribute("compression") ?? throw new InvalidOperationException();

            if (encoding == "base64")
            {
                int dataSize = (layer.Width * layer.Height * 4) + 1024;
                byte[] buffer = new byte[dataSize];
                reader.ReadElementContentAsBase64(buffer, 0, dataSize);

                using Stream stream = new MemoryStream(buffer, false);
                Stream decompStream = compression switch
                {
                    "gzip" => new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress),
                    "zlib" => new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress),
                    _ => stream
                };

                using BinaryReader br = new(decompStream);
                layer.Tiles = new int[layer.Width * layer.Height];
                layer.FlipAndRotate = new byte[layer.Width * layer.Height];

                for (int i = 0; i < layer.Tiles.Length; i++)
                {
                    uint tileData = br.ReadUInt32();

                    // Handle flip flags
                    byte flipAndRotate = 0;
                    if ((tileData & 0x80000000) != 0) flipAndRotate |= 1;
                    if ((tileData & 0x40000000) != 0) flipAndRotate |= 2;
                    if ((tileData & 0x20000000) != 0) flipAndRotate |= 4;

                    layer.FlipAndRotate[i] = flipAndRotate;
                    layer.Tiles[i] = (int)(tileData & 0x1FFFFFFF);
                }
            }
        }

        private void ImportProperties(XmlReader reader, SortedList<string, string> properties)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "property")
                {
                    string name = reader.GetAttribute("name") ?? throw new InvalidOperationException();
                    string value = reader.GetAttribute("value") ?? throw new InvalidOperationException();
                    if (name != null && value != null)
                    {
                        properties.Add(name, value);
                    }
                }
            }
        }
    }
}
