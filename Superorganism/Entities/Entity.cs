using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Collisions;
using Superorganism.Interfaces;
// ReSharper disable All

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
				Center = new Vector2((float)(Texture.Width / (float)numOfSpriteCols) / 2.0f, (float)(Texture.Height / (float)numOfSpriteRows) / 2.0f),
				SizeScale = sizeScale
			};
			if (collisionType.GetType() == typeof(BoundingCircle))
			{
				TextureInfo.CollisionType = new BoundingCircle(TextureInfo.Center,
					(float)(TextureInfo.UnitTextureWidth / 2.0f) * sizeScale * 0.8f);
			}
			else if (collisionType.GetType() == typeof(BoundingRectangle))
			{
				TextureInfo.CollisionType = new BoundingRectangle(TextureInfo.Center,
					TextureInfo.UnitTextureWidth * sizeScale, TextureInfo.UnitTextureHeight * sizeScale * 0.8f);
			}
		}

		public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
        public virtual void Update(GameTime gameTime) { }
    }
}
