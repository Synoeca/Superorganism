using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Collisions;
using Superorganism.Common;
using Superorganism.Core.Inventory;
using Superorganism.Interfaces;
namespace Superorganism.Entities
{
    /// <summary>
    /// Base abstract class for all game entities that provides common functionality and properties
    /// </summary>
    public abstract class Entity : IEntity
    {
        /// <summary>
        /// The texture used to render this entity
        /// </summary>
        public abstract Texture2D Texture { get; set; }

        /// <summary>
        /// Contains information about the entity's texture such as dimensions, sprite details, and scaling
        /// </summary>
        public TextureInfo TextureInfo { get; set; }

        /// <summary>
        /// Represents the current status of the entity (active, inactive, etc.)
        /// </summary>
        public abstract EntityStatus EntityStatus { get; set; }

        /// <summary>
        /// The entity's inventory containing items
        /// </summary>
        public Inventory Inventory { get; set; } = [];

        /// <summary>
        /// The position of the entity in the game world
        /// </summary>
        public abstract Vector2 Position { get; set; }

        /// <summary>
        /// The color tint applied when rendering the entity
        /// </summary>
        public abstract Color Color { get; set; }

        /// <summary>
        /// Loads the entity's texture content and initializes its texture information
        /// </summary>
        /// <param name="content">Content organizer for loading assets</param>
        /// <param name="assetName">Name of the texture asset to load</param>
        /// <param name="numOfSpriteCols">Number of sprite columns in the texture</param>
        /// <param name="numOfSpriteRows">Number of sprite rows in the texture</param>
        /// <param name="collisionType">Type of collision bounding to use for this entity</param>
        /// <param name="sizeScale">Scale factor to apply to the entity size</param>
		public virtual void LoadContent(ContentManager content, string assetName, int numOfSpriteCols, int numOfSpriteRows,
            ICollisionBounding collisionType, float sizeScale)
        {
            Texture = content.Load<Texture2D>(assetName);
            TextureInfo = new TextureInfo()
            {
                TextureWidth = Texture.Width,
                TextureHeight = Texture.Height,
                NumOfSpriteCols = numOfSpriteCols,
                NumOfSpriteRows = numOfSpriteRows,
                Center = new Vector2(Texture.Width / (float)numOfSpriteCols / 2.0f, Texture.Height / (float)numOfSpriteRows / 2.0f),
                SizeScale = sizeScale
            };
            TextureInfo.CollisionType = collisionType switch
            {
                BoundingCircle => new BoundingCircle(TextureInfo.Center * sizeScale,
                    TextureInfo.UnitTextureWidth / 2.0f * sizeScale),
                BoundingRectangle => new BoundingRectangle(TextureInfo.Center * sizeScale,
                    TextureInfo.UnitTextureWidth * sizeScale,
                    TextureInfo.UnitTextureHeight * sizeScale),
                _ => TextureInfo.CollisionType
            };
        }
        /// <summary>
        /// The collision boundary used for detecting interactions with other entities
        /// </summary>
        public abstract ICollisionBounding CollisionBounding { get; set; }

        /// <summary>
        /// Renders the entity to the screen
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        /// <param name="spriteBatch">The sprite batch used for drawing textures</param>
		public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

        /// <summary>
        /// Updates the entity's state based on game time
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public virtual void Update(GameTime gameTime) { }
    }
}