using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Superorganism.ContentPipeline
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
                                map.Width = int.Parse(reader.GetAttribute("width"));
                                map.Height = int.Parse(reader.GetAttribute("height"));
                                map.TileWidth = int.Parse(reader.GetAttribute("tilewidth"));
                                map.TileHeight = int.Parse(reader.GetAttribute("tileheight"));
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
                Name = reader.GetAttribute("name"),
                FirstTileId = int.Parse(reader.GetAttribute("firstgid")),
                TileWidth = int.Parse(reader.GetAttribute("tilewidth")),
                TileHeight = int.Parse(reader.GetAttribute("tileheight"))
            };

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "image")
                {
                    tileset.ImagePath = reader.GetAttribute("source");
                }
            }

            return tileset;
        }

        private Layer ImportLayer(XmlReader reader)
        {
            Layer layer = new()
            {
                Name = reader.GetAttribute("name"),
                Width = int.Parse(reader.GetAttribute("width")),
                Height = int.Parse(reader.GetAttribute("height")),
                Opacity = reader.GetAttribute("opacity") != null ? float.Parse(reader.GetAttribute("opacity")) : 1.0f
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
            string encoding = reader.GetAttribute("encoding");
            string compression = reader.GetAttribute("compression");

            if (encoding == "base64")
            {
                int dataSize = (layer.Width * layer.Height * 4) + 1024;
                byte[] buffer = new byte[dataSize];
                reader.ReadElementContentAsBase64(buffer, 0, dataSize);

                using Stream stream = new MemoryStream(buffer, false);
                Stream decompStream = compression switch
                {
                    "gzip" => new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress),
                    "zlib" => new MonoGame.Framework.Utilities.Deflate.ZlibStream(stream, MonoGame.Framework.Utilities.Deflate.CompressionMode.Decompress),
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
                    string name = reader.GetAttribute("name");
                    string value = reader.GetAttribute("value");
                    if (name != null && value != null)
                    {
                        properties.Add(name, value);
                    }
                }
            }
        }
    }
}
