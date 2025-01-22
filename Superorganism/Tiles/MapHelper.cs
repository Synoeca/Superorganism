using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Superorganism.Collisions;
using Superorganism.Core.Managers;

namespace Superorganism.Tiles
{
    public static class MapHelper
    {
        //public const int TileSize = 64;  // Each tile is 64x64 pixels
        //public const int MapWidth = 200; // Width in tiles
        //public const int MapHeight = 50; // Height in tiles

        public static int TileSize { get; set; }
        public static int MapWidth { get; set; }
        public static int MapHeight { get; set; }

        private static readonly Dictionary<int, int> GroundLevels = new();

        public static void AnalyzeMapGround(TiledMap map)
        {
            GroundLevels.Clear();

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
        public static float GetGroundLevel(TiledMap map, float worldX)
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
        public static bool IsInsideTile(TiledMap map, Vector2 position)
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
        public static bool CheckEntityMapCollision(TiledMap map, Vector2 position, Vector2 size)
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

                    foreach (Group group in GameState.CurrentMap.Groups.Values)
                    {
                        foreach (Layer layer in group.Layers.Values)
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
                                    (int)position.X,
                                    (int)(position.Y),
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
            }

            return false;
        }

        public static float GetGroundYPosition(TiledMap map, float worldX, float positionY, float entityHeight, ICollisionBounding collisionBounding)
        {
            // Convert world X to tile X
            int tileX = (int)(worldX / TileSize);
            int tileY = (int)(positionY / TileSize);
            if (tileX < 0 || tileX >= MapWidth) return MapHeight * TileSize;

            // Search downward until we find ground
            for (; tileY < MapHeight; tileY++)
            {
                foreach (Layer layer in map.Layers.Values)
                {
                    int tileId = layer.GetTile(tileX, tileY);
                    if (tileId != 0)
                    {
                        Dictionary<string, string> property = GetTileProperties(tileId);

                        // Skip non-collidable tiles
                        if (property.TryGetValue("isCollidable", out string isCollidable) && isCollidable == "false")
                        {
                            continue;
                        }

                        // Handle diagonal tiles
                        if (property.TryGetValue("isDiagonal", out string isDiagonal) && isDiagonal == "true")
                        {
                            // Check for slope properties
                            if (property.TryGetValue("SlopeLeft", out string slopeLeftStr) &&
                                property.TryGetValue("SlopeRight", out string slopeRightStr) &&
                                int.TryParse(slopeLeftStr, out int slopeLeft) &&
                                int.TryParse(slopeRightStr, out int slopeRight))
                            {
                                float tileLeft = tileX * TileSize;
                                float tileRight = tileLeft + TileSize;
                                float tileBottom = tileY * TileSize;
                                float slope = (slopeRight - slopeLeft) / (float)TileSize;

                                // Check if worldX is within tile bounds
                                if (worldX >= tileLeft && worldX <= tileRight)
                                {
                                    // Calculate Y position on slope at worldX
                                    float distanceFromLeft = worldX - tileLeft;
                                    float slopeY = tileBottom + slopeLeft + (slope * distanceFromLeft);
                                    return slopeY;
                                }
                            }
                            continue;
                        }

                        // Found regular ground tile
                        return (tileY * TileSize);
                    }
                }

                foreach (Group group in map.Groups.Values)
                {
                    foreach (Layer layer in group.Layers.Values)
                    {
                        int tileId = layer.GetTile(tileX, tileY);
                        if (tileId != 0)
                        {
                            Dictionary<string, string> property = GetTileProperties(tileId);

                            // Skip non-collidable tiles
                            if (property.TryGetValue("isCollidable", out string isCollidable) && isCollidable == "false")
                            {
                                continue;
                            }

                            // Handle diagonal tiles
                            if (property.TryGetValue("isDiagonal", out string isDiagonal) && isDiagonal == "true")
                            {
                                // Check for slope properties
                                if (property.TryGetValue("SlopeLeft", out string slopeLeftStr) &&
                                    property.TryGetValue("SlopeRight", out string slopeRightStr) &&
                                    int.TryParse(slopeLeftStr, out int slopeLeft) &&
                                    int.TryParse(slopeRightStr, out int slopeRight))
                                {
                                    float tileLeft = tileX * TileSize;
                                    float tileRight = tileLeft + TileSize;
                                    float tileBottom = (tileY + 1) * TileSize;
                                    float slope = (slopeRight - slopeLeft) / (float)TileSize;

                                    // Check if worldX is within tile bounds
                                    if (worldX >= tileLeft && worldX <= tileRight)
                                    {
                                        
                                        // Calculate Y position on slope at worldX
                                        float distanceFromLeft = collisionBounding.Center.X - tileLeft;
                                        float slopeY = tileBottom - (slopeLeft + (slope * distanceFromLeft));
                                        if (slopeY > 1200 && slopeY < 1260)
                                        {

                                        }

                                        return slopeY;
                                    }
                                }
                                //continue;
                            }

                            // Found regular ground tile
                            return (tileY * TileSize);
                        }
                    }
                }
            }
            return MapHeight * TileSize;
        }

        public static Dictionary<string, string> GetTileProperties(int tileId)
        {
            // Find the correct tileset based on FirstGid
            Tileset targetTileset = null;
            int localTileId = tileId;

            // Sort tilesets by FirstGid in descending order to find correct tileset
            List<Tileset> sortedTilesets = GameState.CurrentMap.Tilesets.Values
                .OrderByDescending(t => t.FirstGid)
                .ToList();

            foreach (Tileset tileset in sortedTilesets)
            {
                if (tileId >= tileset.FirstGid)
                {
                    targetTileset = tileset;
                    localTileId = tileId - tileset.FirstGid;
                    break;
                }
            }

            return targetTileset?.Tiles?.GetValueOrDefault(localTileId)?.Properties
                   ?? new Dictionary<string, string>();
        }
    }
}
