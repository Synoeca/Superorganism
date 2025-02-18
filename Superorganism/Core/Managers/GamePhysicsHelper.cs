using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Superorganism.Collisions;
using Superorganism.Common;
using Superorganism.Tiles;

namespace Superorganism.Core.Managers;

public static class GamePhysicsHelper
{
    public static void HandleMovement(KeyboardState keyboardState, GamePadState gamePadState, GameTime gameTime, 
        ref float movementSpeed, ref float animationSpeed, ref bool flipped, ref bool isOnGround, ref Vector2 velocity, float friction, 
        ref float soundTimer, float jumpStrength, ref bool isJumping, SoundEffect moveSound, SoundEffect jumpSound, 
        float moveSoundInterval, float shiftSoundInterval, ref Vector2 position, ref ICollisionBounding collisionBounding, 
        float gravity, TextureInfo textureInfo, ref float jumpDiagonalPosY, ref bool isCenterOnDiagonal)
    {
        gamePadState = GamePad.GetState(0);
        keyboardState = Keyboard.GetState();

        // Update movement speed based on shift key
        if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
        {
            movementSpeed = 4.5f;
            animationSpeed = 0.1f;
        }
        else
        {
            movementSpeed = 1.0f;
            animationSpeed = 0.15f;
        }

        float proposedXVelocity = 0;

        // Calculate proposed horizontal movement
        if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
        {
            proposedXVelocity = -movementSpeed;
            flipped = true;
        }
        else if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
        {
            proposedXVelocity = movementSpeed;
            flipped = false;
        }
        else if (isOnGround)
        {
            proposedXVelocity = velocity.X * friction;
            if (Math.Abs(proposedXVelocity) < 0.1f)
            {
                proposedXVelocity = 0;
                soundTimer = 0f;
            }
        }

        // Handle jumping
        bool startingJump = isOnGround && keyboardState.IsKeyDown(Keys.Space);
        if (startingJump)
        {
            velocity.Y = jumpStrength;
            isOnGround = false;
            isJumping = true;
            jumpSound?.Play();
        }
        else if (!isJumping) // Only check for falling if we're not in a jump
        {
            bool diagonal = false;
            bool isCenterOnDiagonalSlope = false;
            bool hasGroundBelow = CheckCollisionAtPosition(position, GameState.CurrentMap, collisionBounding, ref diagonal, ref isCenterOnDiagonalSlope);
            isCenterOnDiagonal = isCenterOnDiagonalSlope;
            if (!hasGroundBelow || diagonal)
            {
                isOnGround = false;
                if (velocity.Y >= 0) // Only apply gravity if we're not moving upward
                {
                    //_velocity.Y += Gravity;
                }
            }
            velocity.Y += gravity;
        }
        else
        {
            // in the middle of a jump, apply gravity
            velocity.Y += gravity;
        }

        // Try X movement first
        Vector2 proposedXPosition = position + new Vector2(proposedXVelocity, 0);
        bool diagonalX = false;
        bool isCenterOnDiagonalTile = false;
        bool hasXCollision = CheckCollisionAtPosition(proposedXPosition, GameState.CurrentMap, collisionBounding, ref diagonalX, ref isCenterOnDiagonalTile);

        // Apply X movement if no collision
        if (!hasXCollision)
        {
            position.X = proposedXPosition.X;
            velocity.X = proposedXVelocity;
            if (Math.Abs(velocity.X) > 0.1f && !isJumping)
            {
                PlayMoveSound(gameTime, ref soundTimer, moveSound, keyboardState, moveSoundInterval, shiftSoundInterval);
            }
        }
        else
        {
            float newPosY = 0;
            bool hasLeftDiagonal = false;
            bool hasRightDiagonal = false;
            BoundingRectangle xTileRec = new();
            // Check if the collision is with a diagonal tile
            if (MapHelper.HandleDiagonalCollision(GameState.CurrentMap, position, proposedXPosition,
                    collisionBounding, ref velocity, ref newPosY, ref xTileRec, ref hasLeftDiagonal, ref hasRightDiagonal))
            {
                position.X = proposedXPosition.X;
                velocity.X = proposedXVelocity;
                if (newPosY != 0)
                {
                    if (!isJumping)
                    {
                        if (velocity.Y == 0)
                        {
                            isOnGround = true;
                        }
                    }
                }

                if (Math.Abs(velocity.X) > 0.1f && !isJumping)
                {
                    PlayMoveSound(gameTime, ref soundTimer, moveSound, keyboardState, moveSoundInterval, shiftSoundInterval);
                }
            }
            else
            {
                // If it's not a diagonal tile, handle as normal collision
                if (!isCenterOnDiagonalTile)
                {
                    //_velocity.X = 0;
                }
                velocity.X = 0;

            }
        }

        // Then try Y movement
        if (velocity.Y != 0)
        {
            bool isDiagonal = false;
            bool isCenterOnDiagonalSlope = false;
            Vector2 proposedYPosition = position + new Vector2(0, velocity.Y);
            bool hasYCollision = CheckCollisionAtPosition(proposedYPosition, GameState.CurrentMap, collisionBounding, ref isDiagonal, ref isCenterOnDiagonalSlope);
            if (!hasYCollision && !isDiagonal)
            {
                position.Y = proposedYPosition.Y;
                isOnGround = false;
            }
            else
            {
                if (Math.Abs(velocity.Y) > 0) // Moving downward
                {
                    if (velocity.Y > 0)
                    {
                        bool leftHitsDiagonal = false;
                        bool rightHitsDiagonal = false;
                        float leftSlope = 0;
                        float rightSlope = 0;
                        // Check ground at both bottom corners
                        float leftGroundY = MapHelper.GetGroundYPosition(
                            GameState.CurrentMap,
                            position.X,
                            position.Y,
                            textureInfo.UnitTextureHeight * textureInfo.SizeScale,
                            collisionBounding,
                            ref leftHitsDiagonal,
                            ref leftSlope
                        );

                        float rightGroundY = MapHelper.GetGroundYPosition(
                            GameState.CurrentMap,
                            position.X + (textureInfo.UnitTextureWidth * textureInfo.SizeScale),
                            position.Y,
                            textureInfo.UnitTextureHeight * textureInfo.SizeScale,
                            collisionBounding,
                            ref rightHitsDiagonal,
                            ref rightSlope
                        );

                        float leftPos = leftGroundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                        float rightPos = rightGroundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale);

                        if (!isDiagonal)
                        { }

                        float groundY = 0;

                        if (!leftHitsDiagonal && rightHitsDiagonal)
                        {

                        }

                        if (isCenterOnDiagonal)
                        {
                            if (leftHitsDiagonal && rightHitsDiagonal)
                            {
                                groundY = Math.Min(leftGroundY, rightGroundY);
                            }
                            else if (leftHitsDiagonal)
                            {
                                groundY = leftGroundY;  // Use the diagonal Y
                            }
                            else if (rightHitsDiagonal)
                            {
                                groundY = rightGroundY;  // Use the diagonal Y
                            }

                        }
                        else
                        {
                            groundY = Math.Min(leftGroundY, rightGroundY);
                        }

                        float newGroundY;
                        float bottom = 0;
                        if (collisionBounding is BoundingRectangle br)
                        {
                            bottom = br.Bottom;
                        }
                        else if (collisionBounding is BoundingCircle bcc)
                        {
                            bottom = bcc.Center.Y - bcc.Radius;
                        }
                        if (groundY < position.Y)
                        {
                            newGroundY = Math.Max(leftGroundY, rightGroundY) -
                                         (textureInfo.UnitTextureHeight * textureInfo.SizeScale);

                            float distanceToNewGround = Math.Abs(position.Y - newGroundY);
                            if (newGroundY - position.Y < 5)
                            {
                                if (newGroundY >= 1254 && newGroundY < 1255.5)
                                {

                                }
                                position.Y = newGroundY;
                                isOnGround = true;
                                if (isJumping) isJumping = false;
                            }
                            else
                            {
                                position.Y = proposedYPosition.Y;
                            }


                        }
                        else
                        {
                            newGroundY = groundY - (textureInfo.UnitTextureHeight * textureInfo.SizeScale);

                            // For landing on mixed diagonal/flat tiles
                            if (leftHitsDiagonal || rightHitsDiagonal)
                            {
                                if (leftHitsDiagonal && rightHitsDiagonal)
                                {
                                    newGroundY = Math.Min(leftGroundY, rightGroundY) -
                                                 (textureInfo.UnitTextureHeight * textureInfo.SizeScale);

                                }
                                else if (leftHitsDiagonal)
                                {
                                    if (leftSlope < 0)  // Downward slope (\)
                                    {
                                        if (leftGroundY > rightGroundY && leftGroundY - rightGroundY < 64)
                                        {
                                            newGroundY = rightGroundY -
                                                         (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                        }
                                        else
                                        {
                                            newGroundY = leftGroundY -
                                                         (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                        }
                                    }
                                    else  // Upward slope (/)
                                    {
                                        if (leftGroundY < rightGroundY && rightGroundY - leftGroundY < 64)
                                        {
                                            if (rightGroundY - leftGroundY < 2)
                                            {
                                                newGroundY = rightGroundY -
                                                             (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                            }
                                            else
                                            {
                                                newGroundY = leftGroundY -
                                                             (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                            }

                                        }
                                        else
                                        {
                                            newGroundY = leftGroundY -
                                                         (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                        }
                                    }
                                }
                                else if (rightHitsDiagonal)
                                {
                                    if (rightSlope < 0)  // Downward slope (\)
                                    {
                                        if (leftGroundY < rightGroundY && rightGroundY - leftGroundY < 64)
                                        {
                                            newGroundY = rightGroundY -
                                                         (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                        }
                                        else
                                        {
                                            if (leftGroundY < position.Y)
                                            {
                                                newGroundY = rightGroundY -
                                                             (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                            }
                                            else
                                            {
                                                if (leftGroundY - position.Y < 2)
                                                {
                                                    newGroundY = leftGroundY -
                                                                 (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                                }
                                                else
                                                {
                                                    newGroundY = rightGroundY -
                                                                 (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                                }

                                            }

                                        }
                                    }
                                    else  // Upward slope (/)
                                    {
                                        if (leftGroundY < rightGroundY && (rightGroundY - leftGroundY) < 64)
                                        {
                                            newGroundY = leftGroundY -
                                                         (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                        }
                                        else
                                        {
                                            newGroundY = rightGroundY -
                                                         (textureInfo.UnitTextureHeight * textureInfo.SizeScale);
                                        }
                                    }
                                }
                            }

                            // Rest remains the same
                            if (jumpDiagonalPosY == 0 ||
                                (leftHitsDiagonal || rightHitsDiagonal) ||
                                newGroundY < jumpDiagonalPosY)
                            {
                                jumpDiagonalPosY = newGroundY;
                            }

                            if (position.Y < jumpDiagonalPosY)
                            {
                                position.Y = proposedYPosition.Y;
                                isOnGround = false;
                            }


                            else
                            {
                                if (newGroundY < 1255)
                                {

                                }

                                position.Y = newGroundY;
                                isOnGround = true;
                                if (isJumping) isJumping = false;
                                velocity.Y = 0;
                                jumpDiagonalPosY = 0;
                            }
                        }
                    }
                    else
                    {
                        position.Y = proposedYPosition.Y;
                        isOnGround = false;
                    }
                }

            }
        }

        // Check map bounds
        Rectangle mapBounds = MapHelper.GetMapWorldBounds();
        position.X = MathHelper.Clamp(position.X,
            (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f,
            mapBounds.Width - (textureInfo.UnitTextureWidth * textureInfo.SizeScale) / 2f);

        // Clamp velocity
        velocity.X = MathHelper.Clamp(velocity.X, -movementSpeed * 2, movementSpeed * 2);
        isCenterOnDiagonal = false;
    }

    private static bool CheckCollisionAtPosition(Vector2 position, TiledMap map, ICollisionBounding collisionBounding,
        ref bool isDiagonal, ref bool isCenterOnDiagonal)
    {
        int leftTile = 0;
        int rightTile = 0;
        int topTile = 0;
        int bottomTile = 0;

        // Update collision bounds for test position
        if (collisionBounding is BoundingRectangle br)
        {
            BoundingRectangle testBounds = new(
                position.X,
                position.Y,
                br.Width,
                br.Height
            );

            leftTile = (int)(testBounds.Left / MapHelper.TileSize) - 1;
            rightTile = (int)Math.Ceiling(testBounds.Right / MapHelper.TileSize);
            topTile = (int)(testBounds.Top / MapHelper.TileSize) - 1;
            bottomTile = (int)Math.Ceiling(testBounds.Bottom / MapHelper.TileSize) - 1;
        }
        else if (collisionBounding is BoundingCircle bc)
        {
            Vector2 testCenter = new(position.X, position.Y);
            leftTile = (int)((testCenter.X - bc.Radius) / MapHelper.TileSize);
            rightTile = (int)Math.Ceiling((testCenter.X + bc.Radius) / MapHelper.TileSize);
            topTile = (int)((testCenter.Y - bc.Radius) / MapHelper.TileSize);
            bottomTile = (int)Math.Ceiling((testCenter.Y + bc.Radius) / MapHelper.TileSize);
        }

        // Check collision with map layers
        foreach (Layer layer in map.Layers.Values)
        {
            if (CheckLayerCollision(layer, leftTile, rightTile, topTile, bottomTile, position, ref collisionBounding, ref isDiagonal, ref isCenterOnDiagonal))
                return true;
        }

        // Check collision with group layers
        foreach (Group group in map.Groups.Values)
        {
            foreach (Layer layer in group.Layers.Values)
            {
                if (CheckLayerCollision(layer, leftTile, rightTile, topTile, bottomTile, position, ref collisionBounding, ref isDiagonal, ref isCenterOnDiagonal))
                    return true;
            }
        }

        return false;
    }

    private static bool CheckLayerCollision(Layer layer, int leftTile, int rightTile, int topTile, int bottomTile,
        Vector2 position, ref ICollisionBounding collisionBounding, ref bool isThisDiagonalTile,
        ref bool isCenterOnDiagonal)
    {
        int tilex = (int)(collisionBounding.Center.X / MapHelper.TileSize);
        int tiley = (int)(collisionBounding.Center.Y / MapHelper.TileSize);

        if (leftTile < 0 || rightTile < 0) { leftTile = rightTile = 0; }
        if (leftTile >= MapHelper.MapWidth || rightTile >= MapHelper.MapWidth) { leftTile = rightTile = MapHelper.MapWidth - 1; }
        if (topTile < 0 || bottomTile < 0) { topTile = bottomTile = 0; }
        if (topTile >= MapHelper.MapHeight || bottomTile >= MapHelper.MapHeight) { topTile = bottomTile = MapHelper.MapHeight - 1; }

        if (tilex == 52)
        {
            if (tiley == 20)
            {

            }
        }


        for (int y = topTile; y <= bottomTile; y++)
        {
            for (int x = leftTile; x <= rightTile; x++)
            {
                //int tileId = layer.GetTile(x, y);
                int tileId = layer.GetTile(x, y);

                if (tileId != 0)
                {
                    if (x == 52)
                    {
                        if (y == 20)
                        {

                        }
                    }

                    Dictionary<string, string> property = MapHelper.GetTileProperties(tileId);

                    // Skip non-collidable tiles
                    if (property.TryGetValue("isCollidable", out string isCollidable) && isCollidable == "false")
                    {
                        continue;
                    }

                    BoundingRectangle tileRect = new(
                        x * MapHelper.TileSize,
                        y * MapHelper.TileSize,
                        MapHelper.TileSize - 5,
                        MapHelper.TileSize - 5
                    );

                    bool isDiagonalTile = false;

                    // Check for diagonal tile
                    if (property.TryGetValue("isDiagonal", out string isDiagonal) && isDiagonal == "true")
                    {
                        // Get slope values
                        if (property.TryGetValue("SlopeLeft", out string slopeLeftStr) &&
                            property.TryGetValue("SlopeRight", out string slopeRightStr) &&
                            int.TryParse(slopeLeftStr, out int slopeLeft) &&
                            int.TryParse(slopeRightStr, out int slopeRight))
                        {
                            if (collisionBounding is BoundingRectangle br)
                            {
                                float tileLeft = x * MapHelper.TileSize;
                                float slope = (slopeRight - slopeLeft) / (float)MapHelper.TileSize;
                                float distanceFromLeft = collisionBounding.Center.X - tileLeft;
                                float tileBottom = (y + 1) * MapHelper.TileSize;

                                float slopeY;
                                if (slope > 0)
                                {
                                    slopeY = (tileBottom - slopeLeft) - (slope * distanceFromLeft);
                                }
                                else
                                {
                                    slopeY = (tileBottom - slopeRight) + (slope * (MapHelper.TileSize - Math.Abs(distanceFromLeft)));
                                }

                                if (position.Y < slopeY)
                                {
                                    isDiagonalTile = true;
                                }

                            }
                            else if (collisionBounding is BoundingCircle bc)
                            {

                            }

                        }
                    }
                    isThisDiagonalTile = isDiagonalTile;
                    if (x == tilex && y == 0 && !isDiagonalTile)
                    {

                        continue;
                    }

                    // Regular collision check for non-diagonal tiles
                    ICollisionBounding testBounds;
                    if (collisionBounding is BoundingRectangle bra)
                    {
                        testBounds = new BoundingRectangle(position.X, position.Y, bra.Width, bra.Height);
                        collisionBounding = testBounds;
                    }
                    else if (collisionBounding is BoundingCircle bc)
                    {
                        testBounds = new BoundingCircle(new Vector2(position.X, position.Y), bc.Radius);
                        collisionBounding = testBounds;
                    }
                    else
                    {
                        continue;
                    }

                    if (collisionBounding.CollidesWith(tileRect))
                    {
                        if (collisionBounding.Center.X <= tileRect.Right && collisionBounding.Center.X >= tileRect.Left)
                        {
                            if (isDiagonalTile)
                            {
                                isCenterOnDiagonal = true;
                            }

                        }
                        else
                        {
                            isCenterOnDiagonal = false;
                        }
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private static void PlayMoveSound(GameTime gameTime, ref float soundTimer, SoundEffect moveSound, KeyboardState keyboardState, float moveSoundInterval, float shiftMoveSoundInterval)
    {
        soundTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (soundTimer >= GetMoveSoundInterval(keyboardState, moveSoundInterval, shiftMoveSoundInterval))
        {
            moveSound?.Play();
            soundTimer = 0f;
        }
    }

    private static float GetMoveSoundInterval(KeyboardState keyboardState, float moveSoundInterval, float shiftMoveSoundInterval)
    {
        return (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
            ? shiftMoveSoundInterval
            : moveSoundInterval;
    }
}