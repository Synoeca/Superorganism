﻿using Microsoft.Xna.Framework.Content.Pipeline;
using System.Globalization;
using System.IO.Compression;
using System.Xml;
using Color = Microsoft.Xna.Framework.Color;

namespace ContentPipeline
{
    [ContentImporter(".tmx", DisplayName = "TiledMapEngineContentImporter", DefaultProcessor = "TiledMapEngineContentProcessor")]
    public class TiledMapEngineContentImporter : ContentImporter<TiledMapEngineContent>
    {
        public override TiledMapEngineContent Import(string filename, ContentImporterContext context)
        {
            context.Logger.LogMessage($"=== Starting TMX Import for {filename} ===");
            TiledMapEngineContent result = new()
            {
                Filename = Path.GetFullPath(filename),
                Properties = new Dictionary<string, string>(),
                Tilesets = new Dictionary<string, TilesetContent>(),
                TilesetFirstGid = new Dictionary<string, int>(),
                Layers = new Dictionary<string, LayerContent>(),
                Groups = new Dictionary<string, GroupContent>()
            };

            XmlReaderSettings settings = new()
            {
                DtdProcessing = DtdProcessing.Parse
            };

            using StreamReader stream = File.OpenText(filename);
            using XmlReader reader = XmlReader.Create(stream, settings);
            while (reader.Read())
            {
                string name = reader.Name;

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
                                    TilesetContent tileset = LoadBasicTileset(st, context);
                                    result.TilesetFirstGid.Add(tileset.Source, tileset.FirstGid);
                                    context.Logger.LogMessage($"Loaded source: {tileset.Name} (Source: {tileset.Source})");
                                    context.Logger.LogMessage($"Loaded firstgid: {tileset.Name} (FirstGid: {tileset.FirstGid})");
                                }
                                break;


                            case "layer":
                                using (XmlReader layerReader = reader.ReadSubtree())
                                {
                                    layerReader.Read();
                                    context.Logger.LogMessage("Loading layer...");
                                    LayerContent layer = LoadBasicLayer(layerReader, filename, context);
                                    if (layer != null)
                                    {
                                        result.Layers[layer.Name] = layer;
                                        context.Logger.LogMessage($"Loaded layer: {layer.Name} ({layer.Width}x{layer.Height})");
                                    }
                                    else
                                    {
                                        context.Logger.LogMessage("Couldn't load layer!");
                                    }
                                }
                                break;

                            case "group":
                                using (XmlReader st = reader.ReadSubtree())
                                {
                                    st.Read();
                                    context.Logger.LogMessage("Loading group...");
                                    GroupContent group = LoadGroup(st, context, filename);
                                    result.Groups.Add(group.Name, group);
                                    context.Logger.LogMessage($"Loaded group: {group.Name}");
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

            
            result.MapFileName = Path.GetFileNameWithoutExtension(filename);
            return result;
        }

        private TilesetContent LoadBasicTileset(XmlReader reader, ContentImporterContext context)
        {
            context.Logger.LogMessage("\n=== Starting Tileset Loading ===");
            context.Logger.LogMessage("Reading initial attributes...");

            // Log raw attribute values before parsing
            context.Logger.LogMessage("Raw XML Attributes:");
            context.Logger.LogMessage($"  firstgid: {reader.GetAttribute("firstgid")}");
            context.Logger.LogMessage($"  source: {reader.GetAttribute("source")}");

            TilesetContent result = new()
            {
                FirstGid = ParseIntAttribute(reader, "firstgid"),
                Source = Path.GetFileNameWithoutExtension(reader.GetAttribute("source")),
            };

            context.Logger.LogMessage("\nParsed initial values:");
            context.Logger.LogMessage($"  FirstGid: {result.FirstGid}");
            context.Logger.LogMessage($"  Source: {result.Source}");

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

        private LayerContent LoadBasicLayer(XmlReader reader, string filename, ContentImporterContext context)
        {
            context.Logger.LogMessage("\n=== Starting Layer Loading ===");
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            LayerContent result = new();
            LayerContent.FlippedHorizontallyFlag = 0x80000000;
            LayerContent.FlippedVerticallyFlag = 0x40000000;
            LayerContent.FlippedDiagonallyFlag = 0x20000000;
            LayerContent.HorizontalFlipDrawFlag = 1;
            LayerContent.VerticalFlipDrawFlag = 2;
            LayerContent.DiagonallyFlipDrawFlag = 4;
            result.Filename = Path.GetFullPath(filename);

            context.Logger.LogMessage("Reading initial attributes...");
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
                result.Opacity = float.Parse(reader.GetAttribute("opacity") ?? throw new InvalidOperationException(), NumberStyles.Any, ci);
                context.Logger.LogMessage($"Layer Opacity: {result.Opacity}");
            }
            else
            {
                result.Opacity = 1.0f;
                context.Logger.LogMessage("Layer Opacity not specified, using default value: 1.0");
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
                                                            if ((tileData & LayerContent.FlippedHorizontallyFlag) != 0)
                                                            {
                                                                flipAndRotateFlags |= LayerContent.HorizontalFlipDrawFlag;
                                                            }

                                                            if ((tileData & LayerContent.FlippedVerticallyFlag) != 0)
                                                            {
                                                                flipAndRotateFlags |= LayerContent.VerticalFlipDrawFlag;
                                                            }

                                                            if ((tileData & LayerContent.FlippedDiagonallyFlag) != 0)
                                                            {
                                                                flipAndRotateFlags |= LayerContent.DiagonallyFlipDrawFlag;
                                                            }

                                                            result.FlipAndRotate[i] = flipAndRotateFlags;

                                                            // Clear the flip bits before storing the tile data
                                                            tileData &= ~(LayerContent.FlippedHorizontallyFlag |
                                                                          LayerContent.FlippedVerticallyFlag |
                                                                          LayerContent.FlippedDiagonallyFlag);
                                                            result.Tiles[i] = (int)tileData;
                                                        }
                                                    }

                                                    continue;
                                                }

                                            default:
                                                throw new Exception("Unrecognized encoding.");
                                        }
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
                                                        result.Properties.Add(st.GetAttribute("name"), st.GetAttribute("value"));
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

        public GroupContent LoadGroup(XmlReader reader, ContentImporterContext context, string filename)
        {
            context.Logger.LogMessage("Loading group...");

            GroupContent group = new()
            {
                ObjectGroups = new Dictionary<string, ObjectGroupContent>(),
                Layers = new Dictionary<string, LayerContent>(),
                Properties = new Dictionary<string, string>()
            };

            group.Id = ParseIntAttribute(reader, "id");
            group.Name = reader.GetAttribute("name") ?? string.Empty;
            group.Locked = ParseBoolAttribute(reader, "locked");

            while (!reader.EOF)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "properties":
                                using (XmlReader st = reader.ReadSubtree())
                                {
                                    st.Read();
                                    LoadProperties(st, group.Properties, context);
                                }
                                break;

                            case "layer":
                                using (XmlReader layerReader = reader.ReadSubtree())
                                {
                                    layerReader.Read();
                                    context.Logger.LogMessage("Loading layer...");
                                    LayerContent layer = LoadBasicLayer(layerReader, filename, context);
                                    if (layer != null)
                                    {
                                        group.Layers[layer.Name] = layer;
                                        context.Logger.LogMessage($"Loaded layer: {layer.Name} ({layer.Width}x{layer.Height})");
                                    }
                                    else
                                    {
                                        context.Logger.LogMessage("Couldn't load layer!");
                                    }
                                }
                                break;

                            case "objectgroup":
                                using (XmlReader st = reader.ReadSubtree())
                                {
                                    st.Read();
                                    context.Logger.LogMessage("Loading object group...");
                                    ObjectGroupContent objectGroup = LoadBasicObjectGroup(st, context);
                                    if (objectGroup != null)
                                    {
                                        group.ObjectGroups[objectGroup.Name] = objectGroup;
                                        context.Logger.LogMessage($"Added object group: {objectGroup.Name}");
                                    }
                                }
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "group")
                        {
                            return group;
                        }
                        break;
                }

                reader.Read();
            }

            return group;
        }

        public ObjectGroupContent LoadBasicObjectGroup(XmlReader reader, ContentImporterContext context)
        {
            context.Logger.LogMessage("\n=== Starting LoadBasicObjectGroup ===");

            ObjectGroupContent result = new()
            {
                Objects = new Dictionary<string, ObjectContent>(),
                ObjectProperties = new Dictionary<string, string>()
            };

            // Read and log all attributes
            context.Logger.LogMessage("Reading objectgroup attributes:");

            // ID
            result.Id = ParseIntAttribute(reader, "id");
            context.Logger.LogMessage($"  id: {result.Id}");

            // Name
            result.Name = reader.GetAttribute("name") ?? string.Empty;
            context.Logger.LogMessage($"  name: {result.Name}");

            // Class
            result.Class = reader.GetAttribute("class");
            context.Logger.LogMessage($"  class: {result.Class}");

            // Color
            string colorStr = reader.GetAttribute("color");
            result.Color = ParseColor(colorStr);
            if (result.Color != null)
            {
                context.Logger.LogMessage($"  color A: {colorStr} -> {result.Color.Value.A}");
                context.Logger.LogMessage($"  color B: {colorStr} -> {result.Color.Value.B}");
                context.Logger.LogMessage($"  color G: {colorStr} -> {result.Color.Value.G}");
                context.Logger.LogMessage($"  color R: {colorStr} -> {result.Color.Value.R}");
            }
            else
            {
                context.Logger.LogMessage($"  color is null!!");
            }


            // Tint Color
            string tintColorStr = reader.GetAttribute("tintcolor");
            result.TintColor = ParseColor(tintColorStr);
            if (result.TintColor != null)
            {
                context.Logger.LogMessage($"  color A: {tintColorStr} -> {result.TintColor.Value.A}");
                context.Logger.LogMessage($"  color B: {tintColorStr} -> {result.TintColor.Value.B}");
                context.Logger.LogMessage($"  color G: {tintColorStr} -> {result.TintColor.Value.G}");
                context.Logger.LogMessage($"  color R: {tintColorStr} -> {result.TintColor.Value.R}");
            }
            else
            {
                context.Logger.LogMessage($"  tint color is null!!");
            }


            // Visible (0 = false, 1 = true)
            result.Visible = ParseBoolAttribute(reader, "visible", true);  // Note: reversed logic in TMX
            context.Logger.LogMessage($"  visible: {result.Visible} -> {result.Visible}");

            // Locked
            result.Locked = ParseBoolAttribute(reader, "locked");
            context.Logger.LogMessage($"  locked: {result.Locked}");

            // Opacity
            result.Opacity = ParseFloatAttribute(reader, "opacity", 1.0f);
            context.Logger.LogMessage($"  opacity: {result.Opacity}");

            // Offset
            result.OffsetX = ParseFloatAttribute(reader, "offsetx", 0.0f);
            result.OffsetY = ParseFloatAttribute(reader, "offsety", 0.0f);
            context.Logger.LogMessage($"  offset: ({result.OffsetX}, {result.OffsetY})");

            // Parallax
            result.ParallaxX = ParseFloatAttribute(reader, "parallaxx", 1.0f);
            result.ParallaxY = ParseFloatAttribute(reader, "parallaxy", 1.0f);
            context.Logger.LogMessage($"  parallax: ({result.ParallaxX}, {result.ParallaxY})");

            // Process child elements (properties and objects)
            while (!reader.EOF)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "properties":
                            using (XmlReader st = reader.ReadSubtree())
                            {
                                st.Read();
                                context.Logger.LogMessage("Processing properties:");
                                LoadProperties(st, result.ObjectProperties, context);
                            }
                            break;

                        case "object":
                            using (XmlReader st = reader.ReadSubtree())
                            {
                                st.Read();
                                context.Logger.LogMessage("Processing object:");
                                ObjectContent obj = LoadBasicObject(st);
                                if (!result.Objects.TryAdd(obj.Name, obj))
                                {
                                    int count = result.Objects.Keys.Count(k => k.StartsWith(obj.Name));
                                    string newName = $"{obj.Name}_{count}";
                                    result.Objects.Add(newName, obj);
                                    context.Logger.LogMessage($"  renamed duplicate object to: {newName}");
                                }
                                context.Logger.LogMessage($"  added object: {obj.Name}");
                            }
                            break;
                    }
                }
                reader.Read();
            }

            context.Logger.LogMessage("\n=== Object Group Summary ===");
            context.Logger.LogMessage($"Name: {result.Name}");
            context.Logger.LogMessage($"ID: {result.Id}");
            context.Logger.LogMessage($"Position: ({result.OffsetX}, {result.OffsetY})");
            context.Logger.LogMessage($"Parallax: ({result.ParallaxX}, {result.ParallaxY})");
            context.Logger.LogMessage($"Opacity: {result.Opacity}");
            context.Logger.LogMessage($"Visible: {result.Visible}");
            context.Logger.LogMessage($"Locked: {result.Locked}");
            if (result.Color != null)
            {
                context.Logger.LogMessage($"  color A: {colorStr} -> {result.Color.Value.A}");
                context.Logger.LogMessage($"  color B: {colorStr} -> {result.Color.Value.B}");
                context.Logger.LogMessage($"  color G: {colorStr} -> {result.Color.Value.G}");
                context.Logger.LogMessage($"  color R: {colorStr} -> {result.Color.Value.R}");
            }
            else
            {
                context.Logger.LogMessage($"  color is null!!");
            }
            if (result.TintColor != null)
            {
                context.Logger.LogMessage($"  Tint color A: {tintColorStr} -> {result.TintColor.Value.A}");
                context.Logger.LogMessage($"  Tint color B: {tintColorStr} -> {result.TintColor.Value.B}");
                context.Logger.LogMessage($"  Tint color G: {tintColorStr} -> {result.TintColor.Value.G}");
                context.Logger.LogMessage($"  Tint color R: {tintColorStr} -> {result.TintColor.Value.R}");
            }
            else
            {
                context.Logger.LogMessage($"  tint color is null!!");
            }
            context.Logger.LogMessage($"Properties Count: {result.ObjectProperties.Count}");
            context.Logger.LogMessage($"Objects Count: {result.Objects.Count}");

            return result;
        }

        public ObjectContent LoadBasicObject(XmlReader reader)
        {
            ObjectContent result = new()
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

        private void LoadProperties(XmlReader reader, Dictionary<string, string> properties, ContentImporterContext context)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "property")
                {
                    string name = reader.GetAttribute("name");
                    string value = reader.GetAttribute("value");
                    if (!string.IsNullOrEmpty(name))
                    {
                        properties[name] = value ?? string.Empty;
                        context.Logger.LogMessage($"Added property: {name} = {value}");
                    }
                }
            }
        }

        private static float ParseFloatAttribute(XmlReader reader, string attributeName, float defaultValue)
        {
            string value = reader.GetAttribute(attributeName);
            return string.IsNullOrEmpty(value) ? defaultValue :
                float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private static bool ParseBoolAttribute(XmlReader reader, string attributeName, bool defaultValue = false)
        {
            string value = reader.GetAttribute(attributeName);
            if (string.IsNullOrEmpty(value)) return defaultValue;

            // Handle numeric boolean values (1 = true, 0 = false)
            if (value == "1") return true;
            if (value == "0") return false;

            // Fall back to standard boolean parsing for "true"/"false" strings
            return bool.Parse(value);
        }

        private static Color? ParseColor(string colorStr)
        {
            if (string.IsNullOrEmpty(colorStr))
                return null;

            if (colorStr.StartsWith("#"))
                colorStr = colorStr.Substring(1);

            if (colorStr.Length == 6)
            {
                // RGB format
                int r = Convert.ToInt32(colorStr.Substring(0, 2), 16);
                int g = Convert.ToInt32(colorStr.Substring(2, 2), 16);
                int b = Convert.ToInt32(colorStr.Substring(4, 2), 16);
                return new Color(r, g, b);
            }
            else if (colorStr.Length == 8)
            {
                // ARGB format
                int a = Convert.ToInt32(colorStr.Substring(0, 2), 16);
                int r = Convert.ToInt32(colorStr.Substring(2, 2), 16);
                int g = Convert.ToInt32(colorStr.Substring(4, 2), 16);
                int b = Convert.ToInt32(colorStr.Substring(6, 2), 16);
                return new Color(r, g, b, a);
            }

            return null;
        }
    }
}
