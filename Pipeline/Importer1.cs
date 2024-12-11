using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline;
using TImport = System.String;

namespace Pipeline
{
    [ContentImporter(".tmx", DisplayName = "Tiled Map Importer", DefaultProcessor = "TiledMapProcessor")]
    public class TiledMapImporter : ContentImporter<BasicTilemapContent>
    {
        public override BasicTilemapContent Import(string filename, ContentImporterContext context)
        {
            context.Logger.LogMessage("Starting import of {0}", filename);
            BasicTilemapContent map = new();
            map.MapFilename = filename;

            XmlReaderSettings settings = new()
            {
                DtdProcessing = DtdProcessing.Parse
            };

            try
            {
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
                                    context.Logger.LogMessage("Processing map element");
                                    string? width = reader.GetAttribute("width");
                                    string? height = reader.GetAttribute("height");
                                    string? tileWidth = reader.GetAttribute("tilewidth");
                                    string? tileHeight = reader.GetAttribute("tileheight");

                                    if (width == null || height == null || tileWidth == null || tileHeight == null)
                                    {
                                        throw new InvalidContentException("Map is missing required attributes");
                                    }

                                    map.Width = int.Parse(width);
                                    map.Height = int.Parse(height);
                                    map.TileWidth = int.Parse(tileWidth);
                                    map.TileHeight = int.Parse(tileHeight);
                                    break;

                                case "tileset":
                                    context.Logger.LogMessage("Processing tileset");
                                    using (XmlReader tilesetReader = reader.ReadSubtree())
                                    {
                                        Tileset tileset = ImportTileset(tilesetReader);
                                        context.Logger.LogMessage($"Imported tileset: {tileset.Name}");
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
            }
            catch (Exception ex)
            {
                context.Logger.LogImportantMessage("Error importing map: {0}", ex.Message);
                throw;
            }

            return map;
        }

        private Tileset ImportTileset(XmlReader reader)
        {
            // Read to first element
            reader.Read();

            Tileset tileset = new()
            {
                Name = reader.GetAttribute("name"),
                FirstTileId = int.Parse(reader.GetAttribute("firstgid") ?? "0"),
                TileWidth = int.Parse(reader.GetAttribute("tilewidth") ?? "0"),
                TileHeight = int.Parse(reader.GetAttribute("tileheight") ?? "0")
            };

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "image")
                {
                    string? sourcePath = reader.GetAttribute("source");
                    if (sourcePath != null)
                    {
                        tileset.ImagePath = sourcePath;
                    }
                }
            }

            return tileset;
        }

        private Layer ImportLayer(XmlReader reader)
        {
            // Position at the first element
            reader.Read();

            Layer layer = new()
            {
                Name = reader.GetAttribute("name") ?? throw new InvalidContentException("Layer name cannot be null"),
                Width = int.Parse(reader.GetAttribute("width") ?? throw new InvalidContentException("Layer width cannot be null")),
                Height = int.Parse(reader.GetAttribute("height") ?? throw new InvalidContentException("Layer height cannot be null")),
                Opacity = reader.GetAttribute("opacity") != null ? float.Parse(reader.GetAttribute("opacity")) : 1.0f
            };

            // Initialize arrays
            layer.Tiles = new int[layer.Width * layer.Height];
            layer.FlipAndRotate = new byte[layer.Width * layer.Height];

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
                    "zlib" => new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress), // Use GZip for zlib too
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
