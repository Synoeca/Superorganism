using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Superorganism.Collisions;
using Superorganism.Core.Managers;

namespace Superorganism.Tiles
{
    /// <summary>
    /// Provides static methods for analyzing and interacting with tile-based map physics, including 
    /// ground detection, tile conversions, and collision handling in a 2D tile map.
    /// </summary>
    public static class TilePhysicsInspector
    {
        /// <summary>
        /// Size of a single tile in pixels.
        /// </summary>
        public static int TileSize { get; set; }

        /// <summary>
        /// Total width of the map in tiles.
        /// </summary>
        public static int MapWidth { get; set; }

        /// <summary>
        /// Total height of the map in tiles.
        /// </summary>
        public static int MapHeight { get; set; }

        /// <summary>
        /// Checks whether the provided property dictionary contains valid slope information,
        /// specifically whether "SlopeLeft" and "SlopeRight" keys exist and are parsable as integers.
        /// </summary>
        /// <param name="property">Tile property dictionary.</param>
        /// <param name="slopeLeft">The parsed value of "SlopeLeft" if valid.</param>
        /// <param name="slopeRight">The parsed value of "SlopeRight" if valid.</param>
        /// <returns>True if both slope values are present and valid; otherwise, false.</returns>
        public static bool HasValidSlopeProperties(Dictionary<string, string> property, out int slopeLeft, out int slopeRight)
        {
            slopeLeft = 0;
            slopeRight = 0;

            return property.TryGetValue("SlopeLeft", out string slopeLeftStr) &&
                   property.TryGetValue("SlopeRight", out string slopeRightStr) &&
                   int.TryParse(slopeLeftStr, out slopeLeft) &&
                   int.TryParse(slopeRightStr, out slopeRight);
        }

        /// <summary>
        /// A dictionary that stores ground level data, indexed by tile IDs. 
        /// This is used to track the height or elevation of specific tiles, where the key represents the tile ID and 
        /// the value represents the ground level (or height) at that tile's position.
        /// </summary>
        private static readonly Dictionary<int, int> GroundLevels = new();

        /// <summary>
        /// Analyzes the map to determine the ground level (first solid tile) in each column.
        /// </summary>
        /// <param name="map">The tile map to analyze.</param>
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
        /// Converts tile coordinates to world space coordinates aligned with tile boundaries.
        /// </summary>
        /// <param name="tileX">The tile's X index.</param>
        /// <param name="tileY">The tile's Y index.</param>
        /// <returns>A Vector2 representing world space position.</returns>
        public static Vector2 TileToWorld(int tileX, int tileY)
        {
            // For X position: same as before
            float worldX = tileX * TileSize;

            // For Y position: align with top of tile since that's where collision should happen
            float worldY = tileY * TileSize;  // Subtract TileSize to align with top of the tile

            return new Vector2(worldX, worldY);
        }

        /// <summary>
        /// Converts world coordinates to tile coordinates.
        /// </summary>
        /// <param name="position">The position in world space.</param>
        /// <returns>A tuple representing the tile's X and Y indices.</returns>
        public static (int X, int Y) WorldToTile(Vector2 position)
        {
            return ((int)(position.X / TileSize), (int)(position.Y / TileSize));
        }

        /// <summary>
        /// Gets the Y-coordinate of the ground level in world space at the specified X position.
        /// </summary>
        /// <param name="map">The tile map to query.</param>
        /// <param name="worldX">The X-coordinate in world space.</param>
        /// <returns>Y-coordinate in world space of the ground level.</returns>
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
        /// Determines whether a point in world space is inside a solid tile.
        /// </summary>
        /// <param name="map">The tile map to check.</param>
        /// <param name="position">The point in world space.</param>
        /// <returns>True if the point is inside a solid tile, otherwise false.</returns>
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
        /// Returns the world space bounding rectangle of the entire tile map.
        /// </summary>
        /// <returns>A Rectangle representing the map bounds in world space.</returns>
        public static Rectangle GetMapWorldBounds()
        {
            return new Rectangle(0, 0, MapWidth * TileSize, MapHeight * TileSize);
        }

        /// <summary>
        /// Checks whether an entity intersects any solid tiles in the map.
        /// </summary>
        /// <param name="map">The tile map to check.</param>
        /// <param name="position">The center position of the entity.</param>
        /// <param name="size">The size of the entity.</param>
        /// <returns>True if a collision occurs, otherwise false.</returns>
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

        /// <summary>
        /// Computes the Y position of the ground under a specific X world coordinate, 
        /// accounting for diagonal tiles and collision bounds.
        /// </summary>
        /// <param name="map">The tile map to query.</param>
        /// <param name="worldX">The horizontal position in world space.</param>
        /// <param name="positionY">The current Y position of the entity.</param>
        /// <param name="entityHeight">The height of the entity.</param>
        /// <param name="collisionBounding">The collision bounding shape.</param>
        /// <param name="hitsDiagonalTile">Returns true if a diagonal tile is hit.</param>
        /// <param name="diagonalSlope">The slope of the diagonal tile if hit.</param>
        /// <returns>The Y-coordinate of the ground in world space.</returns>
        public static float GetGroundYPosition(TiledMap map, float worldX, float positionY, float entityHeight, 
            ICollisionBounding collisionBounding, ref bool hitsDiagonalTile, ref float diagonalSlope)
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
                            if (HasValidSlopeProperties(property, out int slopeLeft, out int slopeRight))
                            {
                                float tileLeft = tileX * TileSize;
                                float tileRight = tileLeft + TileSize;
                                float tileBottom = tileY * TileSize;
                                float slope = (slopeRight - slopeLeft) / (float)TileSize;

                                if (collisionBounding is BoundingRectangle brec)
                                {
                                    if (brec.Right >= tileLeft && brec.Left <= tileRight)
                                    {
                                        hitsDiagonalTile = true;
                                        float distanceFromLeft = collisionBounding.Center.X - tileLeft;
                                        float slopeY = tileBottom - (slopeLeft + (slope * Math.Abs(distanceFromLeft)));

                                        diagonalSlope = slope;
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

                            if (tileX == 61 && tileY == 21)
                            {

                            }

                            // Skip non-collidable tiles
                            if (property.TryGetValue("isCollidable", out string isCollidable) && isCollidable == "false")
                            {
                                continue;
                            }

                            // Handle diagonal tiles
                            if (property.TryGetValue("isDiagonal", out string isDiagonal) && isDiagonal == "true")
                            {

                                // Check for slope properties
                                if (HasValidSlopeProperties(property, out int slopeLeft, out int slopeRight))
                                {
                                    float tileLeft = tileX * TileSize;
                                    float tileRight = tileLeft + TileSize;
                                    float tileBottom = (tileY + 1) * TileSize;
                                    float slope = (slopeRight - slopeLeft) / (float)TileSize;

                                    // Check if worldX is within tile bounds
                                    if (worldX >= tileLeft && worldX <= tileRight)
                                    {
                                        float distance;
                                        float slopeY;

                                        if (slope > 0)
                                        {
                                            distance = collisionBounding.Center.X - tileLeft;
                                            if (distance > 64)
                                            {
                                                distance = 64;
                                            }

                                            if (distance < 0)
                                            {
                                                //return (tileY * TileSize)
                                            }
                                            slopeY = (tileBottom - slopeLeft) - (slope * distance);
                                        }
                                        else
                                        {
                                            distance = collisionBounding.Center.X - tileRight;
                                            if (distance < -64)
                                            {
                                                distance = -64;
                                            }
                                            slopeY = (tileBottom - slopeRight) - (slope * distance);
                                        }

                                        if ((positionY) < slopeY)
                                        {
                                            diagonalSlope = slope;
                                            hitsDiagonalTile = true;
                                            
                                        }
                                        return slopeY;
                                    }
                                }
                            }

                            // Found regular ground tile
                            return (tileY * TileSize);
                        }
                    }
                }
            }
            return MapHeight * TileSize;
        }

        /// <summary>
        /// Gets the custom properties of a tile based on its ID.
        /// </summary>
        /// <param name="tileId">The global tile ID.</param>
        /// <returns>A dictionary of property key-value pairs, or empty if none found.</returns>
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

        /// <summary>
        /// Handles collision detection and resolution against diagonal tiles.
        /// </summary>
        /// <param name="map">The tile map.</param>
        /// <param name="position">The current position of the entity.</param>
        /// <param name="proposedPosition">The proposed new position of the entity.</param>
        /// <param name="collisionBounding">The collision bounding shape of the entity.</param>
        /// <param name="velocity">The current velocity of the entity.</param>
        /// <param name="newPosY">Reference to the Y position to update based on collision.</param>
        /// <param name="xTileRec">Reference to the tile rectangle the entity may be colliding with.</param>
        /// <param name="hasLeftDiagonal">Returns true if a left-leaning diagonal is involved.</param>
        /// <param name="hasRightDiagonal">Returns true if a right-leaning diagonal is involved.</param>
        /// <returns>True if movement is allowed, otherwise false.</returns>
        public static bool HandleDiagonalCollision(TiledMap map, Vector2 position, Vector2 proposedPosition,
            ICollisionBounding collisionBounding, ref Vector2 velocity, ref float newPosY, ref BoundingRectangle xTileRec,
            ref bool hasLeftDiagonal, ref bool hasRightDiagonal)
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

                leftTile = (int)(currentBounds.Center.X / TileSize) - 1;
                rightTile = (int)Math.Ceiling(currentBounds.Center.X / TileSize);
                topTile = (int)(currentBounds.Center.Y / TileSize) - 1;
                bottomTile = (int)Math.Ceiling(currentBounds.Center.Y / TileSize);
            }
            else if (collisionBounding is BoundingCircle bc)
            {
                Vector2 currentCenter = new(position.X, position.Y);
                leftTile = (int)((currentCenter.X - bc.Radius) / TileSize);
                rightTile = (int)Math.Ceiling((currentCenter.X + bc.Radius) / TileSize);
                topTile = (int)((currentCenter.Y - bc.Radius) / TileSize);
                bottomTile = (int)Math.Ceiling((currentCenter.Y + bc.Radius) / TileSize);
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
                            if (x * TileSize < collisionBounding.Center.X)
                            {
                                hasLeftDiagonal = true;
                            }
                            else if (x * TileSize > collisionBounding.Center.X)
                            {
                                hasRightDiagonal = true;
                            }
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
                                if (x * TileSize < collisionBounding.Center.X)
                                {
                                    hasLeftDiagonal = true;
                                }
                                else if (x * TileSize > collisionBounding.Center.X)
                                {
                                    hasRightDiagonal = true;
                                }
                                isOnDiagonalTile = true;
                                break;
                            }
                        }
                    }
                }
            }

            // Now check proposed position for collisions
            int proposedLeftTile = 0, 
                proposedRightTile = 0, 
                proposedTopTile = 0, 
                proposedBottomTile = 0;
            ICollisionBounding cb = null;

            if (collisionBounding is BoundingRectangle br2)
            {
                BoundingRectangle proposedBounds = new(
                    proposedPosition.X,
                    proposedPosition.Y,
                    br2.Width,
                    br2.Height
                );

                proposedLeftTile = (int)(proposedBounds.Center.X / TileSize) - 1;
                proposedRightTile = (int)Math.Ceiling(proposedBounds.Center.X / TileSize);
                proposedTopTile = (int)(proposedBounds.Center.Y / TileSize) - 1;
                proposedBottomTile = (int)Math.Ceiling(proposedBounds.Center.Y / TileSize) - 1;
                cb = proposedBounds;
            }
            else if (collisionBounding is BoundingCircle bc2)
            {
                Vector2 proposedCenter = new(proposedPosition.X, proposedPosition.Y);
                proposedLeftTile = (int)((proposedCenter.X - bc2.Radius) / TileSize);
                proposedRightTile = (int)Math.Ceiling((proposedCenter.X + bc2.Radius) / TileSize);
                proposedTopTile = (int)((proposedCenter.Y - bc2.Radius) / TileSize);
                proposedBottomTile = (int)Math.Ceiling((proposedCenter.Y + bc2.Radius) / TileSize);
                cb = bc2;
            }

            // Clamp proposed position tile ranges
            proposedLeftTile = Math.Max(0, proposedLeftTile);
            proposedRightTile = Math.Min(MapWidth - 1, proposedRightTile);
            proposedTopTile = Math.Max(0, proposedTopTile);
            proposedBottomTile = Math.Min(MapHeight - 1, proposedBottomTile);

            // Check for collisions at proposed position
            bool hasCollisionAtProposedPos = false;
            bool isGoingRight = proposedPosition.X - position.X > 0;

            for (int y = proposedTopTile; y <= proposedBottomTile && !hasCollisionAtProposedPos; y++)
            {
                for (int x = proposedLeftTile; x <= proposedRightTile && !hasCollisionAtProposedPos; x++)
                {
                    foreach (Layer layer in map.Layers.Values)
                    {
                        if (CheckBlockingCollision(layer, x, y, cb, isOnDiagonalTile, isGoingRight))
                        {
                            hasCollisionAtProposedPos = true;
                            break;
                        }
                    }

                    foreach (Group group in map.Groups.Values)
                    {
                        foreach (Layer layer in group.Layers.Values)
                        {
                            if (CheckBlockingCollision(layer, x, y, cb, isOnDiagonalTile, isGoingRight))
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

        /// <summary>
        /// Checks whether a tile at a given position blocks movement based on the given collision bounding.
        /// </summary>
        /// <param name="layer">The layer containing the tile.</param>
        /// <param name="x">Tile X index.</param>
        /// <param name="y">Tile Y index.</param>
        /// <param name="collisionBounding">The shape used to detect collisions.</param>
        /// <param name="isOnDiagonalTile">Whether the entity is on a diagonal tile.</param>
        /// <param name="isGoingRight">True if the entity is moving right, otherwise false.</param>
        /// <returns>True if the tile blocks movement, otherwise false.</returns>
        private static bool CheckBlockingCollision(Layer layer, int x, int y, ICollisionBounding collisionBounding,
            bool isOnDiagonalTile, bool isGoingRight)
        {
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

                                if (br.Right >= tileRec.Left)
                                {
                                    if (br.Right - tileRec.Left <= 64)
                                    {
                                        if (br.Right - tileRec.Left > 2)
                                        {
                                            if (!isGoingRight)
                                            {
                                                return false;
                                            }
                                            return !(br.Bottom - (tileRec.Bottom - slopeLeft) < 20);
                                        }

                                        return false;

                                    }

                                }
                                if (br.Left <= tileRec.Right)
                                {
                                    if (tileRec.Right - br.Left <= 64)
                                    {
                                        if (tileRec.Right - br.Left > 5)
                                        {
                                            return !(br.Bottom - (tileRec.Bottom - slopeRight) < 20);
                                        }
                                        return false;
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
                    BoundingRectangle tileRec = new((float)x * TileSize - 3, (float)y * TileSize, TileSize, TileSize);

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

                                    if (isGoingRight)
                                    {
                                        if (tileRec.Right > br.Left || tileRec.Left > br.Right)
                                        {
                                            if (tileRec.Bottom < br.Bottom)
                                            {
                                                return true;
                                            }
                                            return false;
                                        }
                                        return false;
                                    }

                                    if (tileRec.Right > br.Right)
                                    {
                                        if (tileRec.Left > br.Left)
                                        {
                                            return false;
                                        }
                                    }
                                    return true;
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

        /// <summary>
        /// Checks if the specified tile is a diagonal tile and handles collision resolution.
        /// </summary>
        /// <param name="layer">The tile layer to check.</param>
        /// <param name="x">Tile X index.</param>
        /// <param name="y">Tile Y index.</param>
        /// <param name="newPosY">Ref to Y position adjusted during collision resolution.</param>
        /// <param name="collisionBounding">The collision bounding object.</param>
        /// <param name="position">The current position.</param>
        /// <param name="proposedPosition">The intended position to move to.</param>
        /// <param name="xTileRec">Ref to tile rectangle for further processing.</param>
        /// <returns>True if tile is a diagonal tile and has been handled, otherwise false.</returns>
        private static bool CheckDiagonalTile(Layer layer, int x, int y, ref float newPosY,
            ICollisionBounding collisionBounding, Vector2 position, Vector2 proposedPosition, ref BoundingRectangle xTileRec)
        {
            int tileId = layer.GetTile(x, y);
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
                        brec.X = proposedPosition.X;
                        brec.Y = proposedPosition.Y;
                        BoundingRectangle tileRec = new(
                            (float)x * TileSize, 
                            (float)y * TileSize, 
                            TileSize, 
                            TileSize
                        );
                        if (brec.CollidesWith(tileRec))
                        {
                            if (brec.Right > tileRec.Left && brec.Left < tileRec.Right)
                            {
                                float distanceFromLeft = -1;
                                float slopeY = -1;

                                if (brec.Left >= tileRec.Left)
                                {
                                    distanceFromLeft = collisionBounding.Center.X - tileRec.Left;
                                    if (slope > 0)
                                    {
                                        slopeY = tileRec.Bottom - (slopeLeft + (slope * Math.Abs(distanceFromLeft)));
                                    }
                                    else
                                    {
                                        slopeY = tileRec.Bottom + (slopeLeft + (slope * Math.Abs(distanceFromLeft)));
                                    }
                                }
                                else if (brec.Right <= tileRec.Right)
                                {
                                    distanceFromLeft = collisionBounding.Center.X - tileRec.Left;
                                    if (slope > 0)
                                    {
                                        slopeY = tileRec.Bottom - (slopeLeft + (slope * Math.Abs(distanceFromLeft)));
                                    }
                                    else
                                    {
                                        slopeY = tileRec.Bottom + (slopeLeft + (slope * Math.Abs(distanceFromLeft)));
                                    }
                                }

                                position.Y = position.Y;
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
                        const float slopeY = 0;
                        position.Y = slopeY - bc.Radius;
                        position.X = proposedPosition.X;
                    }
                }
            }
            return false;
        }
    }
}
