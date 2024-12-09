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

        private static Dictionary<int, int> _groundLevels = new();

        public static void AnalyzeMapGround(Map map)
        {
            _groundLevels.Clear();

            // Scan each column from top to bottom to find first ground tile
            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    bool foundGround = false;
                    foreach (Layer layer in map.Layers.Values)
                    {
                        if (layer.GetTile(x, y) != 0)
                        {
                            _groundLevels[x] = y;
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
            float worldY = tileY * TileSize - TileSize;  // Subtract TileSize to align with top of the tile

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
        public static float GetGroundLevel(Map map, float worldX)
        {
            int tileX = (int)(worldX / TileSize);
            tileX = Math.Clamp(tileX, 0, MapWidth - 1);

            if (_groundLevels.TryGetValue(tileX, out int groundY))
            {
                return groundY * TileSize;
            }

            return MapHeight * TileSize;
        }

        /// <summary>
        /// Checks if a point is inside a solid tile
        /// </summary>
        public static bool IsInsideTile(Map map, Vector2 position)
        {
            (int tileX, int tileY) = WorldToTile(position);

            // Check bounds
            if (tileX < 0 || tileX >= MapWidth || tileY < 0 || tileY >= MapHeight)
                return false;

            // Check all layers
            foreach (Layer layer in map.Layers.Values)
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
        public static bool CheckEntityMapCollision(Map map, Vector2 position, Vector2 size)
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
                    foreach (Layer layer in map.Layers.Values)
                    {
                        if (layer.GetTile(x, y) != 0) // Non-empty tile
                        {
                            Rectangle tileRect = new Rectangle(
                                x * TileSize,
                                y * TileSize,
                                TileSize,
                                TileSize
                            );

                            Rectangle entityRect = new Rectangle(
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
        public static float GetGroundYPosition(Map map, float worldX, float entityWidth)
        {
            // Check a few points along the entity's width
            float leftX = worldX - entityWidth / 2;
            float rightX = worldX + entityWidth / 2;
            float centerX = worldX;
            float highestGround = -1;

            // Check ground at left, center, and right points
            foreach (float x in new[] { leftX, centerX, rightX })
            {
                int tileX = (int)(x / TileSize);
                if (tileX < 0 || tileX >= MapWidth) continue;

                // Search from top to bottom
                for (int tileY = 0; tileY < MapHeight; tileY++)
                {
                    foreach (Layer layer in map.Layers.Values)
                    {
                        if (layer.GetTile(tileX, tileY) != 0)
                        {
                            // Return the top of the tile by multiplying tileY by TileSize
                            float groundY = (tileY) * TileSize;  // Removed any offset adjustments
                            if (highestGround == -1 || groundY < highestGround)
                            {
                                highestGround = groundY;
                            }
                            break;
                        }
                    }
                }
            }

            return highestGround == -1 ? MapHeight * TileSize : highestGround;
        }
    }
}
