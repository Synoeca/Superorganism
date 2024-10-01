using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism
{
	public interface IEntity
	{
		/// <summary>
		/// 
		/// </summary>
		Vector2 Position { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		void Update(GameTime gameTime);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		void LoadContent(ContentManager content);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="spriteBatch"></param>
		void Draw(GameTime gameTime, SpriteBatch spriteBatch);
	}
}
