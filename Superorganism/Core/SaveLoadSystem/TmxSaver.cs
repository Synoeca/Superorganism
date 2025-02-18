using System;
using System.IO;
using System.IO.Compression;
using System.Xml;
using Superorganism.Tiles;

namespace Superorganism.Core.SaveLoadSystem;

public static class TmxSaver
{
    public static void SaveTmxFile(TiledMap map, string originalMapPath, string newMapPath)
    {
        // Load the original TMX as XML document
        XmlDocument doc = new();
        doc.Load(originalMapPath);

        // Find each layer in the XML
        foreach (XmlNode layerNode in doc.SelectNodes("//layer"))
        {
            string layerName = layerNode.Attributes?["name"]?.Value;
            if (layerName == null) continue;

            // Find corresponding layer in our map
            Layer layer = null;
            foreach (Group group in map.Groups.Values)
            {
                if (group.Layers.TryGetValue(layerName, out Layer foundLayer))
                {
                    layer = foundLayer;
                    break;
                }
            }
            if (layer == null && map.Layers.TryGetValue(layerName, out Layer mapLayer))
            {
                layer = mapLayer;
            }
            if (layer == null) continue;

            // Find the data node
            XmlNode dataNode = layerNode.SelectSingleNode(".//data");
            if (dataNode == null) continue;

            // Convert tile data to compressed Base64
            string encodedData = EncodeTileData(layer.Tiles, layer.FlipAndRotate);
            dataNode.InnerText = encodedData;
        }

        // Save the modified XML
        doc.Save(newMapPath);
    }

    private static string EncodeTileData(int[] tiles, byte[] flipAndRotate)
    {
        using MemoryStream ms = new();
        using (GZipStream gzip = new(ms, CompressionMode.Compress, true))
        using (BinaryWriter writer = new(gzip))
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                uint tileData = (uint)tiles[i];
                byte flags = flipAndRotate[i];

                // Add flip flags back
                if ((flags & Layer.HorizontalFlipDrawFlag) != 0)
                    tileData |= Layer.FlippedHorizontallyFlag;
                if ((flags & Layer.VerticalFlipDrawFlag) != 0)
                    tileData |= Layer.FlippedVerticallyFlag;
                if ((flags & Layer.DiagonallyFlipDrawFlag) != 0)
                    tileData |= Layer.FlippedDiagonallyFlag;

                writer.Write(tileData);
            }
        }

        return Convert.ToBase64String(ms.ToArray());
    }
}