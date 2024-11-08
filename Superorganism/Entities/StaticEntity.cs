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
	public class StaticEntity(Vector2 position) : Entity
	{
		public override Texture2D Texture { get; set; }
		public override Vector2 Position { get; set; } = position;
		public override Color Color { get; set; } = Color.White;

		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Texture, Position, Color);
		}
	}
}
