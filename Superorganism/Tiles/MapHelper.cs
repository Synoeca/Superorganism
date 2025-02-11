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
                                //if (worldX >= tileLeft && worldX <= tileRight)
                                //{
                                //    // Calculate Y position on slope at worldX
                                //    float distanceFromLeft = worldX - tileLeft;
                                //    float slopeY = tileBottom + slopeLeft + (slope * distanceFromLeft);
                                //    return slopeY;
                                //}

                                if (collisionBounding is BoundingRectangle brec)
                                {
                                    if (brec.Right >= tileLeft && brec.Left <= tileRight)
                                    {
                                        float distanceFromLeft = collisionBounding.Center.X - tileLeft;

                                        float slopeY = tileBottom - (slopeLeft + (slope * Math.Abs(distanceFromLeft)));

                                        //if (collisionBounding is BoundingRectangle br)
                                        //{
                                        //    //position.Y = position.Y;
                                        //    //newPosY = position.Y;
                                        //    //newPosY = slopeY - br.Height / 2;
                                        //    if (distanceFromLeft > 0)
                                        //    {
                                        //        newPosY = slopeY - br.Height;
                                        //    }

                                        //    position.X = proposedPosition.X;
                                        //}
                                        //else if (collisionBounding is BoundingCircle bc)
                                        //{
                                        //    position.Y = slopeY - bc.Radius;
                                        //    position.X = proposedPosition.X;
                                        //}
                                        return slopeY;
                                    }
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
                                        if (distanceFromLeft > 64)
                                        {
                                            //return -1;
                                            distanceFromLeft = 64;
                                        }
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
        public static bool HandleDiagonalCollision(TiledMap map, Vector2 position, Vector2 proposedPosition,
            ICollisionBounding collisionBounding, ref Vector2 velocity, ref float newPosY, ref BoundingRectangle xTileRec)
        {
            // Calculate the range of tiles to check based on the collision bounds
            int leftTile, rightTile, topTile, bottomTile;

            // Get current position tile range
            if (collisionBounding is BoundingRectangle br)
            {
                BoundingRectangle currentBounds = new(
                    position.X,
                    position.Y,
                    br.Width,
                    br.Height
                );

                leftTile = (int)(currentBounds.Center.X / MapHelper.TileSize) - 1;
                rightTile = (int)Math.Ceiling(currentBounds.Center.X / MapHelper.TileSize);
                topTile = (int)(currentBounds.Center.Y / MapHelper.TileSize) - 1;
                bottomTile = (int)Math.Ceiling(currentBounds.Center.Y / MapHelper.TileSize);
            }
            else if (collisionBounding is BoundingCircle bc)
            {
                Vector2 currentCenter = new(position.X, position.Y);
                leftTile = (int)((currentCenter.X - bc.Radius) / MapHelper.TileSize);
                rightTile = (int)Math.Ceiling((currentCenter.X + bc.Radius) / MapHelper.TileSize);
                topTile = (int)((currentCenter.Y - bc.Radius) / MapHelper.TileSize);
                bottomTile = (int)Math.Ceiling((currentCenter.Y + bc.Radius) / MapHelper.TileSize);
            }
            else
            {
                return false;
            }

            // Clamp current position tile ranges
            leftTile = Math.Max(0, leftTile);
            rightTile = Math.Min(MapWidth - 1, rightTile);
            topTile = Math.Max(0, topTile);
            bottomTile = Math.Min(MapHeight - 1, bottomTile);

            // Check if entity is currently on a diagonal tile
            bool isOnDiagonalTile = false;
            for (int y = topTile; y <= bottomTile && !isOnDiagonalTile; y++)
            {
                for (int x = leftTile; x <= rightTile && !isOnDiagonalTile; x++)
                {
                    foreach (Layer layer in map.Layers.Values)
                    {
                        if (CheckDiagonalTile(layer, x, y, ref newPosY, collisionBounding, position, proposedPosition, ref xTileRec))
                        {
                            isOnDiagonalTile = true;
                            break;
                        }
                    }

                    foreach (Group group in map.Groups.Values)
                    {
                        foreach (Layer layer in group.Layers.Values)
                        {
                            if (CheckDiagonalTile(layer, x, y, ref newPosY, collisionBounding, position, proposedPosition, ref xTileRec))
                            {
                                isOnDiagonalTile = true;
                                break;
                            }
                        }
                    }
                }
            }

            // Now check proposed position for collisions
            int proposedLeftTile, proposedRightTile, proposedTopTile, proposedBottomTile;

            if (collisionBounding is BoundingRectangle br2)
            {
                BoundingRectangle proposedBounds = new(
                    proposedPosition.X,
                    proposedPosition.Y,
                    br2.Width,
                    br2.Height
                );

                proposedLeftTile = (int)(proposedBounds.Center.X / MapHelper.TileSize) - 1;
                proposedRightTile = (int)Math.Ceiling(proposedBounds.Center.X / MapHelper.TileSize);
                proposedTopTile = (int)(proposedBounds.Center.Y / MapHelper.TileSize) - 1;
                proposedBottomTile = (int)Math.Ceiling(proposedBounds.Center.Y / MapHelper.TileSize) - 1;
            }
            else if (collisionBounding is BoundingCircle bc2)
            {
                Vector2 proposedCenter = new(proposedPosition.X, proposedPosition.Y);
                proposedLeftTile = (int)((proposedCenter.X - bc2.Radius) / MapHelper.TileSize);
                proposedRightTile = (int)Math.Ceiling((proposedCenter.X + bc2.Radius) / MapHelper.TileSize);
                proposedTopTile = (int)((proposedCenter.Y - bc2.Radius) / MapHelper.TileSize);
                proposedBottomTile = (int)Math.Ceiling((proposedCenter.Y + bc2.Radius) / MapHelper.TileSize);
            }
            else
            {
                return false;
            }

            // Clamp proposed position tile ranges
            proposedLeftTile = Math.Max(0, proposedLeftTile);
            proposedRightTile = Math.Min(MapWidth - 1, proposedRightTile);
            proposedTopTile = Math.Max(0, proposedTopTile);
            proposedBottomTile = Math.Min(MapHeight - 1, proposedBottomTile);

            // Check for collisions at proposed position
            bool hasCollisionAtProposedPos = false;
            for (int y = proposedTopTile; y <= proposedBottomTile && !hasCollisionAtProposedPos; y++)
            {
                for (int x = proposedLeftTile; x <= proposedRightTile && !hasCollisionAtProposedPos; x++)
                {
                    if (x == 74)
                    {
                        if (y == 19)
                        {

                        }
                    }
                    // Skip checking the current diagonal tile we're standing on
                    //if (isOnDiagonalTile && IsCurrentDiagonalTile(x, y, leftTile, rightTile, topTile, bottomTile))
                    //{
                    //    continue;
                    //}

                    foreach (Layer layer in map.Layers.Values)
                    {
                        if (CheckBlockingCollision(layer, x, y, collisionBounding, isOnDiagonalTile))
                        {
                            hasCollisionAtProposedPos = true;
                            break;
                        }
                    }

                    foreach (Group group in map.Groups.Values)
                    {
                        foreach (Layer layer in group.Layers.Values)
                        {
                            if (CheckBlockingCollision(layer, x, y, collisionBounding, isOnDiagonalTile))
                            {
                                hasCollisionAtProposedPos = true;
                                break;
                            }
                        }
                    }
                }
            }

            // Allow movement only if we're either on a diagonal tile AND there's no collision at proposed position
            return isOnDiagonalTile && !hasCollisionAtProposedPos;
        }



        private static bool CheckBlockingCollision(Layer layer, int x, int y, ICollisionBounding collisionBounding,
            bool isOnDiagonalTile)
        {
            if (x == 74 && y == 19)
            {

            }
            int tileId = layer.GetTile(x, y);
            if (tileId == 0) return false;

            Dictionary<string, string> properties = GetTileProperties(tileId);

            // Check if this is a collidable tile that should block movement
            if (properties.TryGetValue("isCollidable", out string isSolid) && isSolid == "true")
            {
                if (properties.TryGetValue("isDiagonal", out string isDiagonal) && isSolid == "true")
                {
                    BoundingRectangle tileRec = new(
                        (float)x * TileSize - 1, 
                        (float)y * TileSize, 
                        TileSize, 
                        TileSize
                    );
                    if (collisionBounding is BoundingRectangle br)
                    {
                        if (br.CollidesWith(tileRec))
                        {
                            if (properties.TryGetValue("SlopeLeft", out string slopeLeftStr) &&
                                properties.TryGetValue("SlopeRight", out string slopeRightStr) &&
                                int.TryParse(slopeLeftStr, out int slopeLeft) &&
                                int.TryParse(slopeRightStr, out int slopeRight))
                            {
                                if (collisionBounding.Center.X >= tileRec.Left &&
                                    collisionBounding.Center.X <= tileRec.Right)
                                {
                                    return false;
                                }
                                else
                                {
                                    if (br.Right >= tileRec.Left)
                                    {
                                        if (br.Right - tileRec.Left <= 64)
                                        {
                                            //return !(br.Bottom >= tileRec.Bottom - slopeLeft);
                                            return !(br.Bottom - (tileRec.Bottom - slopeLeft) < 20);
                                        }

                                    }
                                    if (br.Left <= tileRec.Right)
                                    {
                                        if (tileRec.Right - br.Left <= 64)
                                        {
                                            //return !(br.Bottom >= tileRec.Bottom - slopeRight);
                                            return !(br.Bottom - (tileRec.Bottom - slopeRight) < 20);
                                        }
                                    }
                                }


                            }
                        }

                    }
                    else if (collisionBounding is BoundingCircle bc)
                    {
                        if (bc.CollidesWith(tileRec))
                        {

                        }
                    }
                }
                else
                {
                    BoundingRectangle tileRec = new((float)x * TileSize, (float)y * TileSize, TileSize, TileSize);

                    if (collisionBounding is BoundingRectangle br)
                    {
                        if (br.CollidesWith(tileRec))
                        {
                            if (isOnDiagonalTile)
                            {
                                if (br.Bottom > tileRec.Top)
                                {
                                    if (br.Bottom - tileRec.Top < 35)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                }
                            }
                        }

                    }
                    else if (collisionBounding is BoundingCircle bc)
                    {
                        return bc.CollidesWith(tileRec);
                    }
                }

            }

            return false;
        }

        private static bool IsCurrentDiagonalTile(int x, int y, int currentLeftTile, int currentRightTile,
            int currentTopTile, int currentBottomTile)
        {
            return x >= currentLeftTile && x <= currentRightTile &&
                   y >= currentTopTile && y <= currentBottomTile;
        }

        private static bool CheckDiagonalTile(Layer layer, int x, int y, ref float newPosY,
            ICollisionBounding collisionBounding, Vector2 position, Vector2 proposedPosition, ref BoundingRectangle xTileRec)
        {
            int tilex = (int)(collisionBounding.Center.X / MapHelper.TileSize);
            int tiley = (int)(collisionBounding.Center.Y / MapHelper.TileSize);
            //if (x == tilex && y == tiley) 
            //{
            //    return false;
            //}

            int tileId = layer.GetTile(x, y);
            int tile1 = layer.GetTile(64, 19);
            int tile2 = layer.GetTile(62, 19);
            if (x == 63 && y == 19)
            {
                
            }
            if (x == 74 && y == 19)
            {

            }
            if (tileId == 0) return false;

            Dictionary<string, string> properties = GetTileProperties(tileId);

            if (properties.TryGetValue("isDiagonal", out string isDiagonal) && isDiagonal == "true")
            {
                if (properties.TryGetValue("SlopeLeft", out string slopeLeftStr) &&
                    properties.TryGetValue("SlopeRight", out string slopeRightStr) &&
                    int.TryParse(slopeLeftStr, out int slopeLeft) &&
                    int.TryParse(slopeRightStr, out int slopeRight))
                {
                    float slope = (slopeRight - slopeLeft) / (float)TileSize;

                    if (collisionBounding is BoundingRectangle brec)
                    {
                        BoundingRectangle tileRec = new(
                            (float)x * TileSize - 2, 
                            (float)y * TileSize, 
                            TileSize, 
                            TileSize
                        );
                        if (brec.CollidesWith(tileRec))
                        {
                            if (brec.Right >= tileRec.Left && brec.Left <= tileRec.Right)
                            {
                                float distanceFromLeft = collisionBounding.Center.X - tileRec.Left;

                                float slopeY = tileRec.Bottom - (slopeLeft + (slope * Math.Abs(distanceFromLeft)));

                                //if (collisionBounding is BoundingRectangle br)
                                //{

                                //}
                                position.Y = position.Y;
                                //newPosY = position.Y;
                                //newPosY = slopeY - br.Height / 2;
                                if (distanceFromLeft > 0)
                                {
                                    newPosY = slopeY - brec.Height;
                                }

                                position.X = proposedPosition.X;
                                xTileRec = tileRec;

                                return true;
                            }
                        }
                    }
                    else if (collisionBounding is BoundingCircle bc)
                    {
                        float slopeY = 0;
                        position.Y = slopeY - bc.Radius;
                        position.X = proposedPosition.X;
                    }
                }
            }
            return false;
        }
    }
}
