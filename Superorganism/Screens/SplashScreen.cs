using GameArchitectureExample.StateManagement;
using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GameArchitectureExample.Screens
{
	public class SplashScreen : GameScreen
	{
		private ContentManager _content;
		private Texture2D _background;
		private TimeSpan _displayTime;

		public override void Activate()
		{
			base.Activate();

			if (_content == null)
			{
				_content = new ContentManager(ScreenManager.Game.Services, "Content");
			}
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
			ScreenManager.SpriteBatch.Draw(_background, Vector2.Zero, Color.White);
			ScreenManager.SpriteBatch.End();
		}
	}
}
