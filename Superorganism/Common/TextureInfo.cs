using Microsoft.Xna.Framework;
using Superorganism.Collisions;

namespace Superorganism.Common
{
    /// <summary>
    /// Contains information about entity textures including dimensions, sprite arrangement, scaling, and collision properties
    /// </summary>
    public class TextureInfo
    {
        /// <summary>
        /// Total width of the entire texture in pixels
        /// </summary>
        public float TextureWidth { get; set; }

        /// <summary>
        /// Total height of the entire texture in pixels
        /// </summary>
        public float TextureHeight { get; set; }

        /// <summary>
        /// Number of columns in the sprite sheet
        /// Used to divide the texture into individual sprite frames horizontally
        /// </summary>
        public int NumOfSpriteCols { get; set; }

        /// <summary>
        /// Number of rows in the sprite sheet
        /// Used to divide the texture into individual sprite frames vertically
        /// </summary>
        public int NumOfSpriteRows { get; set; }

        /// <summary>
        /// Width of a single sprite frame in pixels
        /// Calculated as TextureWidth divided by NumOfSpriteCols
        /// </summary>
        public float UnitTextureWidth => TextureWidth / NumOfSpriteCols;

        /// <summary>
        /// Height of a single sprite frame in pixels
        /// Calculated as TextureHeight divided by NumOfSpriteRows
        /// </summary>
        public float UnitTextureHeight => TextureHeight / NumOfSpriteRows;

        /// <summary>
        /// The center point of a single sprite frame
        /// Used for positioning and rotation calculations
        /// </summary>
        public Vector2 Center { get; set; }

        /// <summary>
        /// Scale factor to apply to the texture when rendering
        /// A value of 1.0 renders at original size, 0.5 renders at half size, 2.0 renders at double size
        /// </summary>
        public float SizeScale { get; set; } = 1;

        /// <summary>
        /// The collision boundary definition for this texture
        /// Can be BoundingRectangle, BoundingCircle, or other ICollisionBounding implementations
        /// </summary>
        public ICollisionBounding CollisionType { get; set; }

        /// <summary>
        /// The actual width of a single sprite unit after scaling
        /// Calculated as UnitTextureWidth multiplied by SizeScale
        /// </summary>
        public float ScaledWidth => UnitTextureWidth * SizeScale;

        /// <summary>
        /// The actual height of a single sprite unit after scaling
        /// Calculated as UnitTextureHeight multiplied by SizeScale
        /// </summary>
        public float ScaledHeight => UnitTextureHeight * SizeScale;

        /// <summary>
        /// The physical weight of the entity
        /// Used for physics calculations and movement behaviors affecting momentum and inertia
        /// </summary>
        public float Weight { get; set; } = 1.0f;
    }
}