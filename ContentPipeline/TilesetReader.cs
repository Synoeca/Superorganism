﻿using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Tiles;

namespace ContentPipeline
{
    public class TilesetReader : ContentTypeReader<Tileset>
    {
        protected override Tileset Read(ContentReader input, Tileset existingInstance)
        {
            string name = input.ReadString();
            int firstTileId = input.ReadInt32();
            int tileWidth = input.ReadInt32();
            int tileHeight = input.ReadInt32();
            int spacing = input.ReadInt32();
            int margin = input.ReadInt32();
            string image = input.ReadString();
            Dictionary<string, string> tileProperties = input.ReadObject<Dictionary<string, string>>();

            Texture2D texture = input.ReadExternalReference<Texture2D>();

            return new Tileset
            {
                Name = name,
                FirstTileId = firstTileId,
                TileWidth = tileWidth,
                TileHeight = tileHeight,
                Spacing = spacing,
                Margin = margin,
                Image = image,
                TileProperties = tileProperties,
                TileTexture = texture
            };
        }
    }
}