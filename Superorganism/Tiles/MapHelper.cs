using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Superorganism.Tiles
{
    public static class MapHelper
    {
        public const int TileSize = 64;  // Each tile is 64x64 pixels
        public const int MapWidth = 200; // Width in tiles
        public const int MapHeight = 50; // Height in tiles

        private static readonly Dictionary<int, int> GroundLevels = new();

        public static void AnalyzeMapGround(BasicTiledMTLG map)
        {
            GroundLevels.Clear();

            // Scan each column from top to bottom to find first ground tile
            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    bool foundGround = false;
                    foreach (BasicLayerMTLG layer in map.Layers.Values)
                    {
                        if (layer.GetTile(x, y) != 0)
                        {
                            GroundLevels[x] = y;
                            foundGround = true;
                            break;
                        }
                    }
                    if (foundGround) break;
                }
            }
        }


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
        public static float GetGroundLevel(BasicTiledMTLG map, float worldX)
        {
            int tileX = (int)(worldX / TileSize);
            tileX = Math.Clamp(tileX, 0, MapWidth - 1);

            if (GroundLevels.TryGetValue(tileX, out int groundY))
            {
                return groundY * TileSize;
            }

            return MapHeight * TileSize;
        }

        /// <summary>
        /// Checks if a point is inside a solid tile
        /// </summary>
        public static bool IsInsideTile(BasicTiledMTLG map, Vector2 position)
        {
            (int tileX, int tileY) = WorldToTile(position);

            // Check bounds
            if (tileX < 0 || tileX >= MapWidth || tileY < 0 || tileY >= MapHeight)
                return false;

            // Check all layers
            foreach (BasicLayerMTLG layer in map.Layers.Values)
            {
                if (layer.GetTile(tileX, tileY) != 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the world bounds of the map
        /// </summary>
        public static Rectangle GetMapWorldBounds()
        {
            return new Rectangle(0, 0, MapWidth * TileSize, MapHeight * TileSize);
        }

        /// <summary>
        /// Checks collision with nearby tiles for an entity
        /// </summary>
        public static bool CheckEntityMapCollision(BasicTiledMTLG map, Vector2 position, Vector2 size)
        {
            // Get the tiles the entity might be intersecting with
            int startTileX = (int)((position.X - size.X / 2) / TileSize);
            int endTileX = (int)((position.X + size.X / 2) / TileSize);
            int startTileY = (int)((position.Y - size.Y / 2) / TileSize);
            int endTileY = (int)((position.Y + size.Y / 2) / TileSize);

            // Clamp to map bounds
            startTileX = Math.Max(0, startTileX);
            endTileX = Math.Min(MapWidth - 1, endTileX);
            startTileY = Math.Max(0, startTileY);
            endTileY = Math.Min(MapHeight - 1, endTileY);

            // Check each potentially colliding tile
            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    foreach (BasicLayerMTLG layer in map.Layers.Values)
                    {
                        if (layer.GetTile(x, y) != 0) // Non-empty tile
                        {
                            Rectangle tileRect = new(
                                x * TileSize,
                                y * TileSize,
                                TileSize,
                                TileSize
                            );

                            Rectangle entityRect = new(
                                (int)(position.X - size.X / 2),
                                (int)(position.Y - size.Y / 2),
                                (int)size.X,
                                (int)size.Y
                            );

                            if (tileRect.Intersects(entityRect))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the ground Y position at a given world X coordinate
        /// </summary>
        public static float GetGroundYPosition(BasicTiledMTLG map, float worldX, float positionY, float entityHeight)
        {
            // Convert world X to tile X
            int tileX = (int)(worldX / TileSize);
            int tileY = (int)(positionY / TileSize);
            if (tileX < 0 || tileX >= MapWidth) return MapHeight * TileSize;

            // Search downward until we find ground
            for (; tileY < MapHeight; tileY++)
            {
                foreach (BasicLayerMTLG layer in map.Layers.Values)
                {
                    if (layer.GetTile(tileX, tileY) != 0)
                    {
                        // Found ground - return the top of this tile minus entity height
                        return (tileY * TileSize);
                    }
                }
            }

            return MapHeight * TileSize;
        }
    }
}
