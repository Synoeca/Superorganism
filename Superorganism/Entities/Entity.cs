using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Entities
{
    public abstract class Entity : IEntity
    {
        public abstract Texture2D Texture { get; set; }
        public abstract Vector2 Position { get; set; }
        public abstract Color Color { get; set; }
        public abstract void LoadContent(ContentManager content, string assetName);
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
        public virtual void Update(GameTime gameTime) { }
    }
}
