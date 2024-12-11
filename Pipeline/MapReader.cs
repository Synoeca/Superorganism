using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Tiles; // This is important!
using System.Collections.Generic;

namespace Pipeline
{
    public class MapReader : ContentTypeReader<Map>
    {
        protected override Map Read(ContentReader reader, Map existingInstance)
        {
            Map map = new()
            {
                Width = reader.ReadInt32(),
                Height = reader.ReadInt32(),
                TileWidth = reader.ReadInt32(),
                TileHeight = reader.ReadInt32()
            };

            // Read properties dictionary
            int propertyCount = reader.ReadInt32();
            for (int i = 0; i < propertyCount; i++)
            {
                string key = reader.ReadString();
                string value = reader.ReadString();
                map.Properties.Add(key, value);
            }

            // Read tilesets
            int tilesetCount = reader.ReadInt32();
            for (int i = 0; i < tilesetCount; i++)
            {
                Superorganism.Tiles.Tileset tileset = ReadTileset(reader);
                map.Tilesets.Add(tileset.Name, tileset);
            }

            // Read layers
            int layerCount = reader.ReadInt32();
            for (int i = 0; i < layerCount; i++)
            {
                Superorganism.Tiles.Layer layer = ReadLayer(reader);
                map.Layers.Add(layer.Name, layer);
            }

            return map;
        }

        private Superorganism.Tiles.Tileset ReadTileset(ContentReader reader)
        {
            Superorganism.Tiles.Tileset tileset = new();

            tileset.Name = reader.ReadString();
            tileset.FirstTileId = reader.ReadInt32();
            tileset.TileWidth = reader.ReadInt32();
            tileset.TileHeight = reader.ReadInt32();
            tileset.Spacing = reader.ReadInt32();
            tileset.Margin = reader.ReadInt32();
            tileset.TileTexture = reader.ReadExternalReference<Texture2D>();

            // Read tile properties - note the use of TilePropertyList from runtime Tileset
            int propCount = reader.ReadInt32();
            for (int i = 0; i < propCount; i++)
            {
                int tileId = reader.ReadInt32();
                var props = new Superorganism.Tiles.Tileset.TilePropertyList();
                int tilePropsCount = reader.ReadInt32();
                for (int j = 0; j < tilePropsCount; j++)
                {
                    props.Add(reader.ReadString(), reader.ReadString());
                }
                tileset.TileProperties.Add(tileId, props);
            }

            return tileset;
        }

        private Superorganism.Tiles.Layer ReadLayer(ContentReader reader)
        {
            Superorganism.Tiles.Layer layer = new();

            layer.Name = reader.ReadString();
            layer.Width = reader.ReadInt32();
            layer.Height = reader.ReadInt32();
            layer.Opacity = reader.ReadSingle();

            // Read tiles array
            int tileCount = reader.ReadInt32();
            layer.Tiles = new int[tileCount];
            for (int i = 0; i < tileCount; i++)
            {
                layer.Tiles[i] = reader.ReadInt32();
            }

            // Read flip and rotate array
            int flipCount = reader.ReadInt32();
            layer.FlipAndRotate = new byte[flipCount];
            for (int i = 0; i < flipCount; i++)
            {
                layer.FlipAndRotate[i] = reader.ReadByte();
            }

            // Read properties
            int propCount = reader.ReadInt32();
            for (int i = 0; i < propCount; i++)
            {
                layer.Properties.Add(reader.ReadString(), reader.ReadString());
            }

            return layer;
        }
    }
}