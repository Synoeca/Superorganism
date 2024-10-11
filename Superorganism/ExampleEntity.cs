using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism
{
	public class ExampleEntity : IEntity
	{
		public Texture2D Texture { get; set; }
		public Vector2 Position { get; set; }
		public Color Color { get; }

		public void LoadContent(ContentManager content)
		{
			throw new NotImplementedException();
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			throw new NotImplementedException();
		}
	}
}
