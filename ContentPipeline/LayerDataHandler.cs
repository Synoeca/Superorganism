using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace ContentPipeline
{
    public static class LayerDataHandler
    {
        private const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        private const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
        private const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;

        public static void DecodeLayerData(XmlReader reader, BasicLayer layer)
        {
            string? encoding = reader.GetAttribute("encoding");
            string? compression = reader.GetAttribute("compression");

            if (encoding == "base64")
            {
                byte[] decodedData = Convert.FromBase64String(reader.ReadElementContentAsString().Trim());

                if (compression == "gzip")
                {
                    using MemoryStream memoryStream = new MemoryStream(decodedData);
                    using GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
                    using MemoryStream resultStream = new MemoryStream();
                    gzipStream.CopyTo(resultStream);
                    decodedData = resultStream.ToArray();
                }

                ProcessDecodedData(decodedData, layer);
            }
            else if (encoding == null)
            {
                // Handle unencoded XML data
                ProcessXmlData(reader, layer);
            }
        }

        private static void ProcessDecodedData(byte[] data, BasicLayer layer)
        {
            layer.Tiles = new int[layer.Width * layer.Height];
            layer.FlipAndRotateFlags = new byte[layer.Width * layer.Height];

            using BinaryReader reader = new BinaryReader(new MemoryStream(data));
            for (int i = 0; i < layer.Tiles.Length; i++)
            {
                uint tileData = reader.ReadUInt32();
                byte flipAndRotate = 0;

                // Extract flip flags
                if ((tileData & FLIPPED_HORIZONTALLY_FLAG) != 0)
                    flipAndRotate |= 1;
                if ((tileData & FLIPPED_VERTICALLY_FLAG) != 0)
                    flipAndRotate |= 2;
                if ((tileData & FLIPPED_DIAGONALLY_FLAG) != 0)
                    flipAndRotate |= 4;

                // Clear the flip bits
                tileData &= ~(FLIPPED_HORIZONTALLY_FLAG |
                            FLIPPED_VERTICALLY_FLAG |
                            FLIPPED_DIAGONALLY_FLAG);

                layer.Tiles[i] = (int)tileData;
                layer.FlipAndRotateFlags[i] = flipAndRotate;
            }
        }

        private static void ProcessXmlData(XmlReader reader, BasicLayer layer)
        {
            layer.Tiles = new int[layer.Width * layer.Height];
            layer.FlipAndRotateFlags = new byte[layer.Width * layer.Height];

            int i = 0;
            using XmlReader subtree = reader.ReadSubtree();
            while (subtree.Read())
            {
                if (subtree.NodeType == XmlNodeType.Element && subtree.Name == "tile")
                {
                    uint gid = uint.Parse(subtree.GetAttribute("gid") ?? "0");
                    byte flipAndRotate = 0;

                    // Extract flip flags
                    if ((gid & FLIPPED_HORIZONTALLY_FLAG) != 0)
                        flipAndRotate |= 1;
                    if ((gid & FLIPPED_VERTICALLY_FLAG) != 0)
                        flipAndRotate |= 2;
                    if ((gid & FLIPPED_DIAGONALLY_FLAG) != 0)
                        flipAndRotate |= 4;

                    // Clear the flip bits
                    gid &= ~(FLIPPED_HORIZONTALLY_FLAG |
                            FLIPPED_VERTICALLY_FLAG |
                            FLIPPED_DIAGONALLY_FLAG);

                    layer.Tiles[i] = (int)gid;
                    layer.FlipAndRotateFlags[i] = flipAndRotate;
                    i++;
                }
            }
        }
    }
}