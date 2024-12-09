using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Collisions;
using Superorganism.Common;

namespace Superorganism.Entities
{
	public class StaticEntity : Entity
	{
		public override Texture2D Texture { get; set; }
		public override EntityStatus EntityStatus { get; set; }
        public override Vector2 Position { get; set; }
		public override Color Color { get; set; } = Color.White;

        protected ICollisionBounding _collisionBounding;
        public override ICollisionBounding CollisionBounding
        {
            get => _collisionBounding;
            set => _collisionBounding = value;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Texture, Position, Color);
		}

        public override void Update(GameTime gameTime)
        {
            CollisionBounding ??= TextureInfo.CollisionType;
            switch (CollisionBounding)
            {
                case BoundingCircle br:
                    br.Center = new Vector2((Position.X + (br.Radius / 2)), Position.Y + (br.Radius / 2));
                    CollisionBounding = br;
                    break;

                case BoundingRectangle br:
                    br.X = Position.X - ((TextureInfo.UnitTextureWidth) * TextureInfo.SizeScale / 2.0f);
                    br.Y = Position.Y - ((TextureInfo.UnitTextureHeight) * TextureInfo.SizeScale / 2.0f);
                    CollisionBounding = br;
                    break;
            }
        }
    }
}
