using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Collisions;
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
				Center = new Vector2((Texture.Width / numOfSpriteCols) / 2, (Texture.Height / numOfSpriteRows) / 2),
				SizeScale = sizeScale
			};
			if (collisionType.GetType() == typeof(BoundingCircle))
			{
				TextureInfo.CollisionType = new BoundingCircle(TextureInfo.Center * sizeScale,
					(TextureInfo.UnitTextureWidth / 2.0f) * sizeScale);
			}
			else if (collisionType.GetType() == typeof(BoundingRectangle))
			{
				TextureInfo.CollisionType = new BoundingRectangle(TextureInfo.Center, Texture.Width, Texture.Height);
			}
		}
		public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
        public virtual void Update(GameTime gameTime) { }
    }
}
