using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.ScreenManagement;

namespace Superorganism.Screens
{
	public class SplashScreen : GameScreen
	{
		private ContentManager _content;
		private Texture2D _background;
		private TimeSpan _displayTime;

		public override void Activate()
		{
			base.Activate();

			_content ??= new ContentManager(ScreenManager.Game.Services, "Content");
			_background = _content.Load<Texture2D>("splashRev1");
			_displayTime = TimeSpan.FromSeconds(2);
		}

		public override void HandleInput(GameTime gameTime, InputState input)
		{
			base.HandleInput(gameTime, input);

			_displayTime -= gameTime.ElapsedGameTime;
			if (_displayTime <= TimeSpan.Zero)
			{
				ExitScreen();
			}
		}

		public override void Draw(GameTime gameTime)
		{
			ScreenManager.SpriteBatch.Begin();
			//ScreenManager.SpriteBatch.Draw(_background, Vector2.Zero, Color.White);
			ScreenManager.SpriteBatch.Draw(_background,
				new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height),
				Color.White);
			ScreenManager.SpriteBatch.End();
		}
	}
}
