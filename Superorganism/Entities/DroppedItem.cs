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
            _velocity = initialVelocity;
            _isOnGround = false;
            _hasLandedOnGround = false;
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

                        CanBeCollected = true;
                    }
                    else
                    {
                        // Still falling - apply gravity
                        _velocity.Y += _gravity;
                        Position += _velocity;
                    }
                }
                else
                {
                    // No map or texture info - simple physics
                    _velocity.Y += _gravity;
                    Position += _velocity;

                    // Arbitrary ground level for testing
                    if (Position.Y > 2000)
                    {
                        Position = new Vector2(Position.X, 2000);
                        _isOnGround = true;
                        _hasLandedOnGround = true;
                        _originalPosition = Position;
                        _positionInitialized = true;
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
                Vector2 origin = new Vector2(SourceRectangle.Width / 2f, SourceRectangle.Height / 2f);

                spriteBatch.Draw(
                    Texture,
                    Position,
                    SourceRectangle,
                    Color,
                    0f,         // rotation
                    origin,     // origin at center of source rectangle
                    scale,      // scale factor from TextureInfo
                    SpriteEffects.None,
                    0f);        // layer depth
            }
            else
            {
                // Calculate origin to center the texture
                Vector2 origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);

                spriteBatch.Draw(
                    Texture,
                    Position,
                    null,       // no source rectangle = use full texture
                    Color,
                    0f,         // rotation
                    origin,     // origin at center of texture
                    scale,      // scale factor from TextureInfo
                    SpriteEffects.None,
                    0f);        // layer depth
            }
        }
    }
}