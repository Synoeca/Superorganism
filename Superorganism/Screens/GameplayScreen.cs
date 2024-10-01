using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using GameArchitectureExample.StateManagement;
using Superorganism;

namespace GameArchitectureExample.Screens
{
	public class GameplayScreen : GameScreen
	{
		private ContentManager _content;
		private SpriteFont _gameFont;

		private CropSprite[] _crops;
		private int _cropsLeft;
		private FliesSprite[] _flies;
		private AntSprite _ant;
		private AntEnemySprite _antEnemy;
		private GroundSprite _groundTexture;

		private SoundEffect _cropPickup;
		private SoundEffect _fliesDestroy;
		private Song _backgroundMusic;

		private double _damageTimer;
		private double _elapsedTime;

		private bool _isGameOver = false;
		private bool _isGameWon = false;

		private float _pauseAlpha;
		private readonly InputAction _pauseAction;

		private int _groundY = 400;
		private int _cropY = 383;

		public GameplayScreen()
		{
			TransitionOnTime = TimeSpan.FromSeconds(1.5);
			TransitionOffTime = TimeSpan.FromSeconds(0.5);

			_pauseAction = new InputAction(
				[Buttons.Start, Buttons.Back],
				[Keys.Back, Keys.Escape], true);
		}

		public override void Activate()
		{
			if (_content == null)
				_content = new ContentManager(ScreenManager.Game.Services, "Content");

			_gameFont = _content.Load<SpriteFont>("gamefont");

			_groundTexture = new GroundSprite(ScreenManager.GraphicsDevice, _groundY, 100);
			_ant = new AntSprite();
			_antEnemy = new AntEnemySprite();

			_groundTexture.LoadContent(_content);
			_ant.LoadContent(_content);
			_antEnemy.LoadContent(_content);

			ResetGame();

			foreach (CropSprite crop in _crops)
			{
				crop.LoadContent(_content);
			}

			foreach (FliesSprite fly in _flies)
			{
				fly.LoadContent(_content);
			}

			_cropPickup = _content.Load<SoundEffect>("Pickup_Coin4");
			_fliesDestroy = _content.Load<SoundEffect>("damaged");

			_backgroundMusic = _content.Load<Song>("MaxBrhon_Cyberpunk");
			MediaPlayer.IsRepeating = true;
			MediaPlayer.Volume = 0.2f;
			MediaPlayer.Play(_backgroundMusic);
		}

		private void ResetGame()
		{
			Random rand = new();
			_crops = new CropSprite[12];

			for (int i = 0; i < _crops.Length; i++)
			{
				_crops[i] = new CropSprite(new Vector2(
					(float)rand.NextDouble() * ScreenManager.GraphicsDevice.Viewport.Width, _cropY));
			}

			_cropsLeft = _crops.Length;

			int numberOfFlies = rand.Next(15, 21);
			_flies = new FliesSprite[numberOfFlies];

			for (int i = 0; i < numberOfFlies; i++)
			{
				float xPos = rand.Next(0, 800);
				float yPos = rand.Next(0, 600);
				Direction randomDirection = (Direction)rand.Next(0, 4);

				_flies[i] = new FliesSprite();
				_flies[i].Position = new Vector2(xPos, yPos);
				_flies[i].Direction = randomDirection;
			}

			_isGameOver = false;
			_isGameWon = false;
			_elapsedTime = 0;
			_damageTimer = 0;
		}

		public override void Deactivate()
		{
			base.Deactivate();
		}

		public override void Unload()
		{
			_content.Unload();
		}

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, false);

			if (IsActive && !_isGameOver && !_isGameWon)
			{
				_elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

				_ant.Update(gameTime);
				_antEnemy.Update(gameTime, _ant.Position);
				_ant.Color = Color.White;
				_antEnemy.Color = Color.White;

				foreach (CropSprite crop in _crops)
				{
					if (!crop.Collected && crop.Bounds.CollidesWith(_ant.Bounds))
					{
						_ant.Color = Color.Gold;
						crop.Collected = true;
						_cropsLeft--;
						_cropPickup.Play();
					}
				}

				if (_cropsLeft <= 0)
				{
					_isGameWon = true;
				}

				foreach (FliesSprite fly in _flies)
				{
					if (!fly.Destroyed)
					{
						fly.Update(gameTime, ScreenManager.GraphicsDevice.Viewport.Width, _cropY + 26);
						if (fly.Bounds.CollidesWith(_ant.Bounds))
						{
							if (_elapsedTime > 1.5)
							{
								_ant.Color = Color.Gray;
								fly.Destroyed = true;
								_fliesDestroy.Play();
								_ant.HitPoint = Math.Max(0, _ant.HitPoint - 10);
							}
						}
					}
				}

				if (_antEnemy.Bounds.CollidesWith(_ant.Bounds))
				{
					_ant.Color = Color.Gray;
					_antEnemy.Color = Color.Gray;

					_damageTimer += gameTime.ElapsedGameTime.TotalSeconds;
					if (_damageTimer >= 0.1)
					{
						_ant.HitPoint = Math.Max(0, _ant.HitPoint - 15);
						_damageTimer = 0;
						_fliesDestroy.Play();
					}
				}

				if (_ant.HitPoint <= 0)
				{
					_isGameOver = true;
				}
			}
		}

		public override void HandleInput(GameTime gameTime, InputState input)
		{
			if (_pauseAction.Occurred(input, ControllingPlayer, out _))
			{
				ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
				return;
			}

			if (_isGameOver || _isGameWon)
			{
				if (input.IsNewKeyPress(Keys.R, ControllingPlayer, out _))
				{
					ScreenManager.AddScreen(new GameplayScreen(), ControllingPlayer);
				}
			}
		}

		private Texture2D CreateTexture(GraphicsDevice graphicsDevice, Color color)
		{
			Texture2D texture = new Texture2D(graphicsDevice, 1, 1);
			texture.SetData(new[] { color });
			return texture;
		}

		private void DrawHealthBar(SpriteBatch spriteBatch)
		{
			int barWidth = 200;
			int barHeight = 20;
			int barX = 20;
			int barY = 20; 

			Texture2D grayTexture = CreateTexture(spriteBatch.GraphicsDevice, Color.Gray);
			Texture2D redTexture = CreateTexture(spriteBatch.GraphicsDevice, Color.Red);

			spriteBatch.Draw(grayTexture, new Rectangle(barX, barY, barWidth, barHeight), Color.White);

			float healthPercentage = (float)_ant.HitPoint / _ant.MaxHitPoint;
			spriteBatch.Draw(redTexture, new Rectangle(barX, barY, (int)(barWidth * healthPercentage), barHeight), Color.White);
		}

		private void DrawCropsLeft(SpriteBatch spriteBatch)
		{
			string cropsLeftText = $"Crops Left: {_cropsLeft}";
			Vector2 textSize = _gameFont.MeasureString(cropsLeftText);
			Vector2 textPosition = new Vector2(
				ScreenManager.GraphicsDevice.Viewport.Width - textSize.X - 20,
				20
			);

			Vector2 shadowOffset = new Vector2(2, 2); // Adjust shadow offset as needed
			spriteBatch.DrawString(_gameFont, cropsLeftText, textPosition + shadowOffset, Color.Black * 0.5f); // Draw shadow with some transparency

			spriteBatch.DrawString(_gameFont, cropsLeftText, textPosition, Color.White);
		}



		public override void Draw(GameTime gameTime)
		{
			ScreenManager.GraphicsDevice.Clear(Color.CornflowerBlue);

			var spriteBatch = ScreenManager.SpriteBatch;
			spriteBatch.Begin();

			_groundTexture.Draw(spriteBatch);
			foreach (CropSprite crop in _crops)
				crop.Draw(gameTime, spriteBatch);
			foreach (FliesSprite fly in _flies)
				fly.Draw(gameTime, spriteBatch);

			_ant.Draw(gameTime, spriteBatch);
			_antEnemy.Draw(gameTime, spriteBatch);

			DrawHealthBar(spriteBatch);
			DrawCropsLeft(spriteBatch);

			Vector2 shadowOffset = new Vector2(2, 2);

			if (_isGameOver)
			{
				var message = "You Lose";
				var textSize = _gameFont.MeasureString(message);
				var textPosition = new Vector2(
					(ScreenManager.GraphicsDevice.Viewport.Width - textSize.X) / 2,
					(ScreenManager.GraphicsDevice.Viewport.Height - textSize.Y) / 2
				);


				spriteBatch.DrawString(_gameFont, message, textPosition + shadowOffset, Color.Black * 0.5f);


				spriteBatch.DrawString(_gameFont, message, textPosition, Color.Red);


				var restartMessage = "Press R to Restart";
				var restartTextSize = _gameFont.MeasureString(restartMessage);
				var restartTextPosition = new Vector2(
					(ScreenManager.GraphicsDevice.Viewport.Width - restartTextSize.X) / 2,
					textPosition.Y + textSize.Y + 20
				);


				spriteBatch.DrawString(_gameFont, restartMessage, restartTextPosition + shadowOffset, Color.Black * 0.5f);


				spriteBatch.DrawString(_gameFont, restartMessage, restartTextPosition, Color.White);
			}

			if (_isGameWon)
			{
				var message = "You Win"; 
				var textSize = _gameFont.MeasureString(message);
				var textPosition = new Vector2(
					(ScreenManager.GraphicsDevice.Viewport.Width - textSize.X) / 2,
					(ScreenManager.GraphicsDevice.Viewport.Height - textSize.Y) / 2
				);

				spriteBatch.DrawString(_gameFont, message, textPosition + shadowOffset, Color.Black * 0.5f); // Shadow


				spriteBatch.DrawString(_gameFont, message, textPosition, Color.Green);

				var restartMessage = "Press R to Restart"; // Space added here
				var restartTextSize = _gameFont.MeasureString(restartMessage);
				var restartTextPosition = new Vector2(
					(ScreenManager.GraphicsDevice.Viewport.Width - restartTextSize.X) / 2,
					textPosition.Y + textSize.Y + 20 // Space below the win message
				);


				spriteBatch.DrawString(_gameFont, restartMessage, restartTextPosition + shadowOffset, Color.Black * 0.5f);


				spriteBatch.DrawString(_gameFont, restartMessage, restartTextPosition, Color.White);
			}

			spriteBatch.End();

			if (TransitionPosition > 0 || _pauseAlpha > 0)
			{
				var alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, _pauseAlpha / 2);
				ScreenManager.FadeBackBufferToBlack(alpha);
			}
		}
	}
}
