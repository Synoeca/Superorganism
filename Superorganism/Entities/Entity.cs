using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
// ReSharper disable All

namespace Superorganism.Entities
{
    public abstract class Entity : IEntity
    {
        public abstract Texture2D Texture { get; set; }
        public TextureInfo TextureInfo { get; set; }
        public abstract Vector2 Position { get; set; }
        public abstract Color Color { get; set; }
		public virtual void LoadContent(ContentManager content, string assetName, int numOfSpriteCols, int numOfSpriteRows)
		{
			Texture = content.Load<Texture2D>(assetName);
			TextureInfo = new TextureInfo()
			{
				TextureWidth = Texture.Width,
				TextureHeight = Texture.Height,
				NumOfSpriteCols = numOfSpriteCols,
				NumOfSpriteRows = numOfSpriteRows,
				Center = new Vector2(Texture.Width / 2, Texture.Height / 2)
			};
		}
		public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
        public virtual void Update(GameTime gameTime) { }
    }
}
