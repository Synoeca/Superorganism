using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism
{
	public class AntSprite
	{
		private GamePadState gamePadState;

		private KeyboardState keyboardState;

		private Texture2D texture;

		private Vector2 position = new(200, 200);

		private bool flipped;

		public Color Color { get; set; } = Color.White;

		public void LoadContent(ContentManager content)
		{
			texture = content.Load<Texture2D>("ant");
		}

		public void Update(GameTime gameTime)
		{
			gamePadState = GamePad.GetState(0);
			keyboardState = Keyboard.GetState();

			// Apply the gamepad movement with inverted Y axis
			position += gamePadState.ThumbSticks.Left * new Vector2(1, -1);
			if (gamePadState.ThumbSticks.Left.X < 0) flipped = true;
			if (gamePadState.ThumbSticks.Left.X > 0) flipped = false;

			// Apply keyboard movement
			if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W)) position += new Vector2(0, -1);
			if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S)) position += new Vector2(0, 1);
			if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
			{
				position += new Vector2(-1, 0);
				flipped = true;
			}
			if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
			{
				position += new Vector2(1, 0);
				flipped = false;
			}

			// Update the bounds
			//bounds.X = position.X - 16;
			//bounds.Y = position.Y - 16;
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			SpriteEffects spriteEffects = (flipped) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			spriteBatch.Draw(texture, position, null, Color, 0, new Vector2(120, 120), 0.25f, spriteEffects, 0);
		}
	}
}
