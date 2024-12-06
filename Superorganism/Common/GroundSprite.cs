using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Common
{
	public class GroundSprite(GraphicsDevice graphics, int groundY, int groundHeight)
	{
		private Texture2D _texture;
		private Vector2 _position;
		private int _groundY = groundY;
		private int _groundHeight = groundHeight;
		private GraphicsDevice _graphics = graphics;

		/// <summary>
		/// Loads the sprite texture using the provided ContentManager
		/// </summary>
		/// <param name="content">The ContentManager to load with</param>
		public void LoadContent(ContentManager content)
		{
			_texture = content.Load<Texture2D>("tiles2");
		}

		/// <summary>
		/// Draws the ground sprite by tiling the texture across the screen
		/// </summary>
		/// <param name="spriteBatch">The spritebatch to render with</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			int screenWidth = _graphics.Viewport.Width;  // Use Viewport to get the screen width

			int textureWidth = _texture.Width; 
			int textureHeight = _texture.Height;

			// Loop through and draw the tiles to fill the ground
			for (int x = 0; x < screenWidth; x += textureWidth)
			{
				for (int y = _groundY; y < _groundY + _groundHeight; y += textureHeight)
				{
					spriteBatch.Draw(_texture, new Vector2(x, y), Color.White);
				}
			}
		}
	}
}
