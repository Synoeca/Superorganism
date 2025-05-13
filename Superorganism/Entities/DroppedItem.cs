using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.AI;
using Superorganism.Collisions;
using Superorganism.Common;
using Superorganism.Core.Managers;
using Superorganism.Tiles;
using System;

namespace Superorganism.Entities
{
    public class DroppedItem : StaticAnimatedCollectableEntity
    {
        // Item properties
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public Rectangle SourceRectangle { get; set; }
        public bool IsFromTileset { get; set; }
        public int TilesetIndex { get; set; }
        public int TileIndex { get; set; }

        // Bobbing animation properties
        private float _bobTimer = 0f;
        private float _bobSpeed = 2.0f;
        private float _bobAmount = 5.0f;
        private Vector2 _originalPosition;
        private bool _positionInitialized = false;
        private bool _hasLandedOnGround = false;
        private float _groundY = 0; // Store the detected ground Y position

        // Physics properties
        private bool _isOnGround = false;
        private bool _isJumping = false;
        private float _jumpDiagonalPosY = 0;
        private bool _isCenterOnDiagonal = false;
        private const float _gravity = 0.5f;
        private bool _flipped = false;

        // Random properties for launch direction
        private static Random _random = new();
        private const float _initialVerticalVelocityMin = -4.0f; // Upward velocity (negative Y)
        private const float _initialVerticalVelocityMax = -6.0f; // Stronger upward launch
        private const float _horizontalVelocityMin = -2.0f; // Left/right randomization
        private const float _horizontalVelocityMax = 2.0f;
        private const float _launchAngleVariance = 0.3f; // Controls spread of launch angle

        public bool CanBeCollected { get; set; } = false;

        // Constructor
        public DroppedItem()
        {
            // Set default animation properties
            IsSpriteAtlas = true;
            HasDirection = false;
            DirectionIndex = 0;
            AnimationSpeed = 0f;
            AnimationFrame = 0;

            // Initialize entity status if needed
            EntityStatus ??= new EntityStatus();
        }

        // Public method to initialize physics properties
        public void InitializePhysics(Vector2 initialVelocity)
        {
            // Generate random launch velocity
            GenerateRandomLaunchVelocity(initialVelocity);

            _isOnGround = false;
            _hasLandedOnGround = false;
        }

        // Generates a random launch velocity with an upward component
        private void GenerateRandomLaunchVelocity(Vector2 baseVelocity)
        {
            // Generate random vertical velocity (always upward)
            float verticalVelocity = (float)(_random.NextDouble() *
                (_initialVerticalVelocityMax - _initialVerticalVelocityMin) +
                _initialVerticalVelocityMin);

            // Generate random horizontal velocity
            float horizontalVelocity = (float)(_random.NextDouble() *
                (_horizontalVelocityMax - _horizontalVelocityMin) +
                _horizontalVelocityMin);

            // Add a bit of randomness to the base velocity if it's provided
            float baseHorizontalModifier = 0f;
            if (baseVelocity != Vector2.Zero)
            {
                // Get direction from base velocity if it exists, but reduce its impact
                baseHorizontalModifier = baseVelocity.X * 0.3f;
            }

            // Set the new velocity with combined random and base components
            _velocity = new Vector2(
                horizontalVelocity + baseHorizontalModifier,
                verticalVelocity
            );

            // Add a small rotation effect (for visual interest if you implement it later)
            _flipped = _random.Next(2) == 0;
        }

        public override void Update(GameTime gameTime)
        {
            // Check if we need to apply physics (falling)
            if (!_hasLandedOnGround)
            {
                // Manual check for ground to better position the item
                if (GameState.CurrentMap != null && TextureInfo != null)
                {
                    // Calculate entity's height
                    float height = SourceRectangle.Height > 0 ?
                        SourceRectangle.Height * TextureInfo.SizeScale :
                        TextureInfo.UnitTextureHeight * TextureInfo.SizeScale;

                    // Check if we're near the ground
                    bool hitsDiagonal = false;
                    float slope = 0;

                    // Get the ground Y position directly
                    _groundY = TilePhysicsInspector.GetGroundYPosition(
                        GameState.CurrentMap,
                        Position.X,
                        Position.Y + height, // Check a bit below the position
                        height,
                        CollisionBounding,
                        ref hitsDiagonal,
                        ref slope);

                    // Check if we're on or below the ground
                    if (Position.Y + height / 2 >= _groundY - height / 2)
                    {
                        // Position item on the ground
                        Position = new Vector2(Position.X, _groundY - height / 2);
                        _isOnGround = true;
                        _hasLandedOnGround = true;
                        _originalPosition = Position;
                        _positionInitialized = true;
                        _velocity = Vector2.Zero;

                        // Add small bounce effect when landing
                        float bounceChance = 0.7f; // 70% chance of bouncing
                        if (_random.NextDouble() < bounceChance)
                        {
                            // Small bounce when landing
                            _velocity.Y = -(_gravity * 2); // Just enough to create a small bounce
                            _hasLandedOnGround = false; // Allow one more bounce
                            _isOnGround = false;
                        }
                        else
                        {
                            // No bounce, allow collection immediately
                            CanBeCollected = true;
                        }
                    }
                    else
                    {
                        // Still falling - apply gravity
                        _velocity.Y += _gravity;

                        // Apply horizontal drag to slow down horizontal movement
                        if (Math.Abs(_velocity.X) > 0.1f)
                        {
                            _velocity.X *= 0.97f; // Air resistance
                        }
                        else
                        {
                            _velocity.X = 0; // Stop tiny movements
                        }

                        Position += _velocity;

                        // Check for bouncing on side walls (optional)
                        // This would require a GetWallXPosition method, similar to GetGroundYPosition
                        // If implemented, it would allow items to bounce off walls
                    }
                }
                else
                {
                    // No map or texture info - simple physics
                    _velocity.Y += _gravity;
                    Position += _velocity;

                    // Apply horizontal drag
                    if (Math.Abs(_velocity.X) > 0.1f)
                    {
                        _velocity.X *= 0.97f;
                    }
                    else
                    {
                        _velocity.X = 0;
                    }

                    // Arbitrary ground level for testing
                    if (Position.Y > 2000)
                    {
                        Position = new Vector2(Position.X, 2000);
                        _isOnGround = true;
                        _hasLandedOnGround = true;
                        _originalPosition = Position;
                        _positionInitialized = true;
                        CanBeCollected = true;
                    }
                }

                // Update collision bounds
                UpdateCollisionBounding();
            }
            else
            {
                // We've landed, do the bobbing animation
                _bobTimer += (float)gameTime.ElapsedGameTime.TotalSeconds * _bobSpeed;

                // Calculate bobbing offset
                float bobOffset = (float)Math.Sin(_bobTimer) * _bobAmount;

                // Apply offset to Y position only
                Position = new Vector2(_originalPosition.X, _originalPosition.Y + bobOffset);

                // Update collision bounds
                UpdateCollisionBounding();
            }
        }

        private void UpdateCollisionBounding()
        {
            if (CollisionBounding == null)
                return;

            // Update collision bounding to match the new position
            if (CollisionBounding is BoundingCircle bc)
            {
                bc.Center = Position;
                CollisionBounding = bc;
            }
            else if (CollisionBounding is BoundingRectangle br)
            {
                // Create a new rectangle centered on the position
                br = new BoundingRectangle(
                    Position.X - (br.Width / 2),
                    Position.Y - (br.Height / 2),
                    br.Width,
                    br.Height);
                CollisionBounding = br;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (Collected || Texture == null)
                return;

            // Determine scale from TextureInfo
            float scale = TextureInfo?.SizeScale ?? 1.0f;

            if (SourceRectangle != Rectangle.Empty)
            {
                // Calculate origin to center the sprite
                Vector2 origin = new(SourceRectangle.Width / 2f, SourceRectangle.Height / 2f);

                // Add flip effect based on horizontal velocity
                SpriteEffects effect = SpriteEffects.None;
                if (_flipped)
                {
                    effect = SpriteEffects.FlipHorizontally;
                }

                spriteBatch.Draw(
                    Texture,
                    Position,
                    SourceRectangle,
                    Color,
                    0f,         // rotation
                    origin,     // origin at center of source rectangle
                    scale,      // scale factor from TextureInfo
                    effect,     // flip effect based on velocity
                    0f);        // layer depth
            }
            else
            {
                // Calculate origin to center the texture
                Vector2 origin = new(Texture.Width / 2f, Texture.Height / 2f);

                // Add flip effect based on horizontal velocity
                SpriteEffects effect = SpriteEffects.None;
                if (_flipped)
                {
                    effect = SpriteEffects.FlipHorizontally;
                }

                spriteBatch.Draw(
                    Texture,
                    Position,
                    null,       // no source rectangle = use full texture
                    Color,
                    0f,         // rotation
                    origin,     // origin at center of texture
                    scale,      // scale factor from TextureInfo
                    effect,     // flip effect based on velocity
                    0f);        // layer depth
            }
        }
    }
}