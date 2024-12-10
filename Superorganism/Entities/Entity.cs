using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Collisions;
using Superorganism.Common;
using Superorganism.Interfaces;

namespace Superorganism.Entities
{
    public abstract class Entity : IEntity
    {
        public abstract Texture2D Texture { get; set; }
        public TextureInfo TextureInfo { get; set; }
        public abstract EntityStatus EntityStatus { get; set; }
        public abstract Vector2 Position { get; set; }
        public abstract Color Color { get; set; }

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

        public abstract ICollisionBounding CollisionBounding { get; set; }
		public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
        public virtual void Update(GameTime gameTime) { }
    }
}
