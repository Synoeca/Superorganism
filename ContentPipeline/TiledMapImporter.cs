using System.Globalization;
using System.IO.Compression;
using System.Xml;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MonoGame.Extended.Content.Pipeline.Tiled;
using SharpDX;

namespace ContentPipeline
{
    [ContentImporter(".tmx", DefaultProcessor = "TilemapProcessor", DisplayName = "Tiled Map Importer")]
    public class TiledMapImporter : ContentImporter<BasicMap>
    {
        public static class ContentPaths
        {
            public const string TilesetDir = "Tileset";
            public const string MapDir = "Maps";

            public static string GetTilesetPath(string filename)
            {
                return Path.Combine(TilesetDir, filename);
            }

            public static string GetMapPath(string filename)
            {
                return Path.Combine(TilesetDir, MapDir, filename);
            }
        }

        public const uint FlippedHorizontallyFlag = 0x80000000;
        public const uint FlippedVerticallyFlag = 0x40000000;
        public const uint FlippedDiagonallyFlag = 0x20000000;

        public const byte HorizontalFlipDrawFlag = 1;
        public const byte VerticalFlipDrawFlag = 2;
        public const byte DiagonallyFlipDrawFlag = 4;

        public override BasicMap Import(string filename, ContentImporterContext context)
        {
            context.Logger.LogMessage($"=== Starting TMX Import for {filename} ===");
            BasicMap result = new() { Filename = Path.GetFullPath(filename) };

            XmlReaderSettings settings = new()
            {
                DtdProcessing = DtdProcessing.Parse
            };

            try
            {
                using StreamReader stream = File.OpenText(filename);
                using XmlReader reader = XmlReader.Create(stream, settings);

                context.Logger.LogMessage("XML Reader created successfully");

                while (reader.Read())
                {
                    string name = reader.Name;
                    context.Logger.LogMessage($"Processing node: {name} (Type: {reader.NodeType})");

                    switch (reader.NodeType)
                    {
                        case XmlNodeType.DocumentType:
                            if (name != "map")
                            {
                                context.Logger.LogImportantMessage("Invalid map format - document type is not 'map'");
                                throw new Exception("Invalid map format");
                            }
                            break;

                        case XmlNodeType.Element:
                            switch (name)
                            {
                                case "map":
                                    result.Width = int.Parse(reader.GetAttribute("width"));
                                    result.Height = int.Parse(reader.GetAttribute("height"));
                                    result.TileWidth = int.Parse(reader.GetAttribute("tilewidth"));
                                    result.TileHeight = int.Parse(reader.GetAttribute("tileheight"));

                                    context.Logger.LogMessage($"Map dimensions: {result.Width}x{result.Height}");
                                    context.Logger.LogMessage($"Tile dimensions: {result.TileWidth}x{result.TileHeight}");
                                    break;

                                case "tileset":
                                    using (XmlReader st = reader.ReadSubtree())
                                    {
                                        st.Read();
                                        context.Logger.LogMessage("Loading tileset...");
                                        BasicMap.BasicTileset tileset = LoadBasicTileset(st, context);
                                        result.Tilesets.Add(tileset.Name, tileset);
                                        context.Logger.LogMessage($"tileset.Name: {tileset.Name} (FirstTileId: {tileset.FirstTileId})");
                                        context.Logger.LogMessage($"Loaded tileset: {tileset.Name} (FirstTileId: {tileset.FirstTileId})");
                                    }
                                    break;

                                case "layer":
                                    using (XmlReader st = reader.ReadSubtree())
                                    {
                                        st.Read();
                                        context.Logger.LogMessage("Loading layer...");
                                        BasicMap.BasicLayer layer = LoadBasicLayer(st);
                                        if (layer != null)
                                        {
                                            result.Layers.Add(layer.Name, layer);
                                            context.Logger.LogMessage($"Loaded layer: {layer.Name} ({layer.Width}x{layer.Height})");
                                        }
                                    }
                                    break;

                                case "objectgroup":
                                    using (XmlReader st = reader.ReadSubtree())
                                    {
                                        st.Read();
                                        context.Logger.LogMessage("Loading object group...");
                                        BasicMap.BasicObjectGroup objectgroup = LoadBasicObjectGroup(st);
                                        result.ObjectGroups.Add(objectgroup.Name, objectgroup);
                                        context.Logger.LogMessage($"Loaded object group: {objectgroup.Name} (Objects: {objectgroup.Objects.Count})");
                                    }
                                    break;

                                case "properties":
                                    using (XmlReader st = reader.ReadSubtree())
                                    {
                                        context.Logger.LogMessage("Loading map properties...");
                                        int propertyCount = 0;
                                        while (!st.EOF)
                                        {
                                            if (st.NodeType == XmlNodeType.Element && st.Name == "property")
                                            {
                                                string propName = st.GetAttribute("name");
                                                string propValue = st.GetAttribute("value");
                                                if (propName != null)
                                                {
                                                    result.Properties.Add(propName, propValue);
                                                    propertyCount++;
                                                }
                                            }
                                            st.Read();
                                        }
                                        context.Logger.LogMessage($"Loaded {propertyCount} map properties");
                                    }
                                    break;
                            }
                            break;
                    }
                }

                // Log final statistics
                context.Logger.LogMessage("=== Import Summary ===");
                context.Logger.LogMessage($"Tilesets loaded: {result.Tilesets.Count}");
                context.Logger.LogMessage($"Layers loaded: {result.Layers.Count}");
                context.Logger.LogMessage($"Object groups loaded: {result.ObjectGroups.Count}");
                context.Logger.LogMessage($"Properties loaded: {result.Properties.Count}");

                return result;
            }
            catch (Exception ex)
            {
                context.Logger.LogImportantMessage($"Error importing TMX file: {ex.Message}");
                context.Logger.LogImportantMessage($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // Add context parameter to these methods and include logging
        private BasicMap.BasicTileset LoadBasicTileset(XmlReader reader, ContentImporterContext context)
        {
            context.Logger.LogMessage("\n=== Starting Tileset Loading ===");
            context.Logger.LogMessage("Reading initial attributes...");

            // Log raw attribute values before parsing
            context.Logger.LogMessage("Raw XML Attributes:");
            context.Logger.LogMessage($"  name: {reader.GetAttribute("name")}");
            context.Logger.LogMessage($"  firstgid: {reader.GetAttribute("firstgid")}");
            context.Logger.LogMessage($"  tilewidth: {reader.GetAttribute("tilewidth")}");
            context.Logger.LogMessage($"  tileheight: {reader.GetAttribute("tileheight")}");
            context.Logger.LogMessage($"  margin: {reader.GetAttribute("margin")}");
            context.Logger.LogMessage($"  spacing: {reader.GetAttribute("spacing")}");
            context.Logger.LogMessage($"  tilecount: {reader.GetAttribute("tilecount")}");
            context.Logger.LogMessage($"  columns: {reader.GetAttribute("columns")}");

            BasicMap.BasicTileset result = new()
            {
                Name = reader.GetAttribute("name")!,
                FirstTileId = ParseIntAttribute(reader, "firstgid"),
                TileWidth = ParseIntAttribute(reader, "tilewidth"),
                TileHeight = ParseIntAttribute(reader, "tileheight"),
                Margin = ParseIntAttribute(reader, "margin"),
                Spacing = ParseIntAttribute(reader, "spacing")
            };

            context.Logger.LogMessage("\nParsed initial values:");
            context.Logger.LogMessage($"  Name: {result.Name}");
            context.Logger.LogMessage($"  FirstTileId: {result.FirstTileId}");
            context.Logger.LogMessage($"  TileWidth: {result.TileWidth}");
            context.Logger.LogMessage($"  TileHeight: {result.TileHeight}");
            context.Logger.LogMessage($"  Margin: {result.Margin}");
            context.Logger.LogMessage($"  Spacing: {result.Spacing}");

            int currentTileId = -1;
            context.Logger.LogMessage("\nProcessing tileset child elements...");

            while (reader.Read())
            {
                context.Logger.LogMessage($"\nNode: {reader.Name} (Type: {reader.NodeType})");
                string name = reader.Name;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (name)
                        {
                            case "image":
                                string source = reader.GetAttribute("source");
                                string width = reader.GetAttribute("width");
                                string height = reader.GetAttribute("height");
                                result.Image = source;

                                context.Logger.LogMessage("Found image element:");
                                context.Logger.LogMessage($"  Source: {source}");
                                context.Logger.LogMessage($"  Width: {width}");
                                context.Logger.LogMessage($"  Height: {height}");
                                break;

                            case "tile":
                                string idAttr = reader.GetAttribute("id");
                                currentTileId = int.Parse(idAttr ?? throw new InvalidOperationException($"Tile missing id attribute"));
                                if (currentTileId != -1)
                                {
                                    string propName = reader.GetAttribute("name");
                                    string propValue = reader.GetAttribute("value");

                                    if (!result.TileProperties.TryGetValue(currentTileId, out Dictionary<string, string> props))
                                    {
                                        props = new Dictionary<string, string>();
                                        result.TileProperties[currentTileId] = props;
                                    }

                                    props[propName ?? throw new InvalidOperationException("Property missing name attribute")] = propValue;

                                    context.Logger.LogMessage($"Added property to tile {currentTileId}:");
                                    context.Logger.LogMessage($"  Name: {propName}");
                                    context.Logger.LogMessage($"  Value: {propValue}");
                                }
                                break;
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (name == "tile")
                        {
                            context.Logger.LogMessage($"Finished processing tile {currentTileId}");
                            currentTileId = -1;
                        }
                        else if (name == "tileset")
                        {
                            context.Logger.LogMessage("\nFinished processing tileset");
                            context.Logger.LogMessage($"Final image path: {result.Image}");
                            context.Logger.LogMessage($"Total properties: {result.TileProperties.Count}");
                            return result;
                        }
                        break;
                }
            }

            context.Logger.LogMessage("=== Completed Tileset Loading ===");
            return result;
        }

        private static int ParseIntAttribute(XmlReader reader, string attributeName, int defaultValue = 0)
        {
            string value = reader.GetAttribute(attributeName);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            if (int.TryParse(value, out int result))
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException($"Failed to parse {attributeName} attribute. Raw value: '{value}'");
            }
        }

        public BasicMap.BasicLayer LoadBasicLayer(XmlReader reader)
        {
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            BasicMap.BasicLayer result = new();

            if (reader.GetAttribute("name") != null)
            {
                result.Name = reader.GetAttribute("name");
            }

            if (reader.GetAttribute("width") != null)
            {
                result.Width = int.Parse(reader.GetAttribute("width") ?? throw new InvalidOperationException());
            }

            if (reader.GetAttribute("height") != null)
            {
                result.Height = int.Parse(reader.GetAttribute("height") ?? throw new InvalidOperationException());
            }

            if (reader.GetAttribute("opacity") != null)
            {
                result.Opacity = float.Parse(reader.GetAttribute("opacity") ?? throw new InvalidOperationException(),
                    NumberStyles.Any, ci);
            }

            result.Tiles = new int[result.Width * result.Height];
            result.FlipAndRotate = new byte[result.Width * result.Height];

            while (!reader.EOF)
            {
                string name = reader.Name;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (name)
                        {
                            case "data":
                                {
                                    if (reader.GetAttribute("encoding") == null)
                                    {
                                        using XmlReader st = reader.ReadSubtree();
                                        int i = 0;
                                        while (!st.EOF)
                                        {
                                            switch (st.NodeType)
                                            {
                                                case XmlNodeType.Element:
                                                    if (st.Name == "tile")
                                                    {
                                                        if (i < result.Tiles.Length)
                                                        {
                                                            result.Tiles[i] = int.Parse(st.GetAttribute("gid"));
                                                            i++;
                                                        }
                                                    }

                                                    break;
                                                case XmlNodeType.EndElement:
                                                    break;
                                            }

                                            st.Read();
                                        }
                                    }
                                    else
                                    {
                                        string encoding = reader.GetAttribute("encoding");
                                        string compressor = reader.GetAttribute("compression");
                                        switch (encoding)
                                        {
                                            case "base64":
                                                {
                                                    int dataSize = (result.Width * result.Height * 4) + 1024;
                                                    byte[] buffer = new byte[dataSize];
                                                    reader.ReadElementContentAsBase64(buffer, 0, dataSize);

                                                    Stream stream = new MemoryStream(buffer, false);
                                                    switch (compressor)
                                                    {
                                                        case "gzip":
                                                            stream = new GZipStream(stream, CompressionMode.Decompress, false);
                                                            break;
                                                        case "zlib":
                                                            stream = new GZipStream(stream, CompressionMode.Decompress, false);
                                                            break;
                                                    }

                                                    using (stream)
                                                    using (BinaryReader br = new(stream))
                                                    {
                                                        for (int i = 0; i < result.Tiles.Length; i++)
                                                        {
                                                            uint tileData = br.ReadUInt32();

                                                            // The data contain flip information as well as the tileset index
                                                            byte flipAndRotateFlags = 0;
                                                            if ((tileData & FlippedHorizontallyFlag) != 0)
                                                            {
                                                                flipAndRotateFlags |= HorizontalFlipDrawFlag;
                                                            }

                                                            if ((tileData & FlippedVerticallyFlag) != 0)
                                                            {
                                                                flipAndRotateFlags |= VerticalFlipDrawFlag;
                                                            }

                                                            if ((tileData & FlippedDiagonallyFlag) != 0)
                                                            {
                                                                flipAndRotateFlags |= DiagonallyFlipDrawFlag;
                                                            }

                                                            result.FlipAndRotate[i] = flipAndRotateFlags;

                                                            // Clear the flip bits before storing the tile data
                                                            tileData &= ~(FlippedHorizontallyFlag |
                                                                          FlippedVerticallyFlag |
                                                                          FlippedDiagonallyFlag);
                                                            result.Tiles[i] = (int)tileData;
                                                        }
                                                    }

                                                    continue;
                                                }

                                            default:
                                                throw new Exception("Unrecognized encoding.");
                                        }
                                    }

                                    Console.WriteLine("It made it!");
                                }
                                break;
                            case "properties":
                                {
                                    using XmlReader st = reader.ReadSubtree();
                                    while (!st.EOF)
                                    {
                                        switch (st.NodeType)
                                        {
                                            case XmlNodeType.Element:
                                                if (st.Name == "property")
                                                {
                                                    if (st.GetAttribute("name") != null)
                                                    {
                                                        result.Properties.Add(st.GetAttribute("name"),
                                                            st.GetAttribute("value"));
                                                    }
                                                }

                                                break;
                                            case XmlNodeType.EndElement:
                                                break;
                                        }

                                        st.Read();
                                    }
                                }
                                break;
                        }

                        break;
                    case XmlNodeType.EndElement:
                        break;
                }

                reader.Read();
            }

            return result;
        }

        public BasicMap.BasicObjectGroup LoadBasicObjectGroup(XmlReader reader)
        {
            BasicMap.BasicObjectGroup result = new();
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            if (reader.GetAttribute("name") != null)
                result.Name = reader.GetAttribute("name");
            if (reader.GetAttribute("width") != null)
                result.Width = int.Parse(reader.GetAttribute("width") ?? throw new InvalidOperationException());
            if (reader.GetAttribute("height") != null)
                result.Height = int.Parse(reader.GetAttribute("height") ?? throw new InvalidOperationException());
            if (reader.GetAttribute("x") != null)
                result.X = int.Parse(reader.GetAttribute("x") ?? throw new InvalidOperationException());
            if (reader.GetAttribute("y") != null)
                result.Y = int.Parse(reader.GetAttribute("y") ?? throw new InvalidOperationException());
            if (reader.GetAttribute("opacity") != null)
                result._opacity = float.Parse(reader.GetAttribute("opacity") ?? throw new InvalidOperationException(), NumberStyles.Any, ci);

            while (!reader.EOF)
            {
                string name = reader.Name;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (name)
                        {
                            case "object":
                                {
                                    using XmlReader st = reader.ReadSubtree();
                                    st.Read();
                                    BasicMap.BasicObject objects = LoadBasicObject(st);
                                    if (!result.Objects.TryAdd(objects.Name, objects))
                                    {
                                        int count = result.Objects.Keys.Count((item) => item.Equals(objects.Name));
                                        result.Objects.Add($"{objects.Name}{count}", objects);
                                    }
                                }
                                break;
                            case "properties":
                                {
                                    using XmlReader st = reader.ReadSubtree();
                                    while (!st.EOF)
                                    {
                                        switch (st.NodeType)
                                        {
                                            case XmlNodeType.Element:
                                                if (st.Name == "property")
                                                {
                                                    if (st.GetAttribute("name") != null)
                                                    {
                                                        result.Properties.Add(st.GetAttribute("name") ?? throw new InvalidOperationException(), st.GetAttribute("value"));
                                                    }
                                                }

                                                break;
                                            case XmlNodeType.EndElement:
                                                break;
                                        }

                                        st.Read();
                                    }
                                }
                                break;
                        }

                        break;
                    case XmlNodeType.EndElement:
                        break;
                }

                reader.Read();
            }

            return result;
        }

        public BasicMap.BasicObject LoadBasicObject(XmlReader reader)
        {
            BasicMap.BasicObject result = new()
            {
                Name = reader.GetAttribute("name"),
                X = int.Parse(reader.GetAttribute("x") ?? throw new InvalidOperationException()),
                Y = int.Parse(reader.GetAttribute("y") ?? throw new InvalidOperationException())
            };

            /*
             * Height and width are optional on objects
             */
            if (int.TryParse(reader.GetAttribute("width"), out int width))
            {
                result.Width = width;
            }

            if (int.TryParse(reader.GetAttribute("height"), out int height))
            {
                result.Height = height;
            }

            while (!reader.EOF)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "properties")
                        {
                            using XmlReader st = reader.ReadSubtree();
                            while (!st.EOF)
                            {
                                switch (st.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        if (st.Name == "property")
                                        {
                                            if (st.GetAttribute("name") != null)
                                            {
                                                result.Properties.Add(st.GetAttribute("name") ?? throw new InvalidOperationException(), st.GetAttribute("value"));
                                            }
                                        }

                                        break;
                                    case XmlNodeType.EndElement:
                                        break;
                                }

                                st.Read();
                            }
                        }
                        if (reader.Name == "image")
                        {
                            result.Image = reader.GetAttribute("source");
                        }

                        break;
                    case XmlNodeType.EndElement:
                        break;
                }

                reader.Read();
            }

            return result;
        }
    }

    public static class MapHelper
    {
        public const int TileSize = 64;  // Each tile is 64x64 pixels
        public const int MapWidth = 200; // Width in tiles
        public const int MapHeight = 50; // Height in tiles

        private static readonly Dictionary<int, int> GroundLevels = new();


        /// <summary>
        /// Converts tile coordinates to world coordinates, aligning with tile boundaries
        /// </summary>
        public static Vector2 TileToWorld(int tileX, int tileY)
        {
            // For X position: same as before
            float worldX = tileX * TileSize;

            // For Y position: align with top of tile since that's where collision should happen
            float worldY = tileY * TileSize;  // Subtract TileSize to align with top of the tile

            return new Vector2(worldX, worldY);
        }

        /// <summary>
        /// Converts world coordinates to tile coordinates
        /// </summary>
        public static (int X, int Y) WorldToTile(Vector2 position)
        {
            return ((int)(position.X / TileSize), (int)(position.Y / TileSize));
        }

        // Update GetGroundLevel to use our analyzed data
        public static float GetGroundLevel(BasicMap map, float worldX)
        {
            int tileX = (int)(worldX / TileSize);
            tileX = Math.Clamp(tileX, 0, MapWidth - 1);

            if (GroundLevels.TryGetValue(tileX, out int groundY))
            {
                return groundY * TileSize;
            }

            return MapHeight * TileSize;
        }
    }

}