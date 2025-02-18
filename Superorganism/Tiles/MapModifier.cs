using System;
using Microsoft.Xna.Framework;

namespace Superorganism.Tiles;

public static class MapModifier
{
    public static void ModifyTileBelowPlayer(TiledMap map, Vector2 playerPosition, bool isBottom)
    {
        // Get player's tile position
        int tileX = (int)(playerPosition.X / map.TileWidth);
        int tileY = 0;
        if (isBottom)
        {
            tileY = (int)(playerPosition.Y / map.TileHeight) + 1; // +1 to get tile below
        }
        else
        {
            tileY = (int)(playerPosition.Y / map.TileHeight);
        }
        

        // Process main layers
        foreach (Layer layer in map.Layers.Values)
        {
            ModifyTileInLayer(layer, tileX, tileY);
        }

        // Process layers in groups
        foreach (Group group in map.Groups.Values)
        {
            foreach (Layer layer in group.Layers.Values)
            {
                ModifyTileInLayer(layer, tileX, tileY);
            }
        }
    }

    private static void ModifyTileInLayer(Layer layer, int tileX, int tileY)
    {
        try
        {
            int currentTile = layer.GetTile(tileX, tileY);
            if (currentTile != 0)
            {
                layer.SetTile(tileX, tileY, 0);
            }
        }
        catch (InvalidOperationException)
        {
            // Skip if coordinates are out of bounds
        }
    }
}