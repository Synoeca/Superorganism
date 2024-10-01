using GameArchitectureExample.Screens;
using GameArchitectureExample.StateManagement;
using GameArchitectureExample;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Linq;

namespace Superorganism
{
	public class Superorganism : Game
	{
		private GraphicsDeviceManager _graphics;
		private readonly ScreenManager _screenManager;

		public Superorganism()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			var screenFactory = new ScreenFactory();
			Services.AddService(typeof(IScreenFactory), screenFactory);

			_screenManager = new ScreenManager(this);
			Components.Add(_screenManager);

			AddInitialScreens();
		}

		private void AddInitialScreens()
		{
			_screenManager.AddScreen(new BackgroundScreen(), null);
			_screenManager.AddScreen(new MainMenuScreen(), null);
			_screenManager.AddScreen(new SplashScreen(), null);
		}

		protected override void Initialize()
		{
			base.Initialize();
		}

		protected override void LoadContent() { }

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			base.Draw(gameTime);    // The real drawing happens inside the ScreenManager component
		}

		//private GraphicsDeviceManager _graphics;
		//private SpriteBatch _spriteBatch;
		//private SpriteFont _spriteFont;

		//private CropSprite[] _crops;
		//private int _cropsLeft;
		//private FliesSprite[] _flies;
		//private AntSprite _ant;
		//private AntEnemySprite _antEnemy;

		//private GroundSprite _groundTexture;
		//private int _groundHeight = 100;
		//private int _groundY = 400;
		//private int _cropY = 383;

		//private SoundEffect _cropPickup;
		//private SoundEffect _fliesDestroy;
		//private Song _backgroundMusic;

		//private double _damageTimer;
		//private double _elapsedTime;

		//private bool _isGameOver = false;
		//private bool _isGameWon = false;

		//public Superorganism()
		//{
		//	_graphics = new GraphicsDeviceManager(this);
		//	Content.RootDirectory = "Content";
		//	IsMouseVisible = true;
		//}

		//protected override void Initialize()
		//{
		//	ResetGame();
		//	base.Initialize();
		//}

		//private void ResetGame()
		//{
		//	Random rand = new();
		//	_crops =
		//	[
		//		new CropSprite(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, _cropY)),
		//		new CropSprite(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, _cropY)),
		//		new CropSprite(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, _cropY)),
		//		new CropSprite(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, _cropY)),
		//		new CropSprite(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, _cropY)),
		//		new CropSprite(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, _cropY)),
		//		new CropSprite(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, _cropY)),
		//		new CropSprite(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, _cropY)),
		//		new CropSprite(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, _cropY)),
		//		new CropSprite(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, _cropY)),
		//		new CropSprite(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, _cropY)),
		//		new CropSprite(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, _cropY)),
		//	];
		//	_cropsLeft = _crops.Length;

		//	int numberOfFlies = rand.Next(15, 21);
		//	_flies = new FliesSprite[numberOfFlies];

		//	for (int i = 0; i < numberOfFlies; i++)
		//	{
		//		float xPos = rand.Next(0, 800);
		//		float yPos = rand.Next(0, 600);
		//		Direction randomDirection = (Direction)rand.Next(0, 4);

		//		_flies[i] = new FliesSprite();
		//		_flies[i].Position = new Vector2(xPos, yPos);
		//		_flies[i].Direction = randomDirection;
		//	}

		//	_ant = new AntSprite();
		//	_antEnemy = new AntEnemySprite();
		//	_groundTexture = new GroundSprite(_graphics, _groundY, _groundHeight);

		//	_isGameOver = false;
		//	_isGameWon = false;
		//	_elapsedTime = 0;
		//	_damageTimer = 0;
		//}

		//protected override void LoadContent()
		//{
		//	_spriteBatch = new SpriteBatch(GraphicsDevice);
		//	foreach (CropSprite crop in _crops)
		//	{
		//		crop.LoadContent(Content);
		//	}
		//	foreach (FliesSprite fly in _flies)
		//	{
		//		fly.LoadContent(Content);
		//	}
		//	_ant.LoadContent(Content);
		//	_antEnemy.LoadContent(Content);
		//	_spriteFont = Content.Load<SpriteFont>("arial");

		//	_groundTexture.LoadContent(Content);
		//	_backgroundMusic = Content.Load<Song>("MaxBrhon_Cyberpunk");
		//	MediaPlayer.IsRepeating = true;
		//	MediaPlayer.Volume = 0.2f;
		//	MediaPlayer.Play(_backgroundMusic);
		//	_cropPickup = Content.Load<SoundEffect>("Pickup_Coin4");
		//	_fliesDestroy = Content.Load<SoundEffect>("damaged");
		//}

		//protected override void Update(GameTime gameTime)
		//{
		//	if (Keyboard.GetState().IsKeyDown(Keys.R))
		//	{
		//		ResetGame();
		//		LoadContent();
		//		return;
		//	}
		//	if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
		//		Exit();

		//	if (Keyboard.GetState().IsKeyDown(Keys.R))
		//	{
		//		ResetGame();
		//		return;
		//	}

		//	if (!_isGameOver && !_isGameWon)
		//	{
		//		_elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
		//		_ant.Update(gameTime);
		//		_antEnemy.Update(gameTime, _ant.Position());

		//		_ant.Color = Color.White;
		//		_antEnemy.Color = Color.White;

		//		foreach (CropSprite crop in _crops)
		//		{
		//			if (!crop.Collected && crop.Bounds.CollidesWith(_ant.Bounds))
		//			{
		//				_ant.Color = Color.Gold;
		//				crop.Collected = true;
		//				_cropsLeft--;
		//				_cropPickup.Play();
		//			}
		//		}

		//		if (_cropsLeft <= 0)
		//		{
		//			_isGameWon = true;
		//		}

		//		foreach (FliesSprite fly in _flies)
		//		{
		//			if (!fly.Destroyed)
		//			{
		//				fly.Update(gameTime, GraphicsDevice.Viewport.Width, _cropY + 26);
		//				if (fly.Bounds.CollidesWith(_ant.Bounds))
		//				{
		//					if (_elapsedTime > 1.5)
		//					{
		//						_ant.Color = Color.Gray;
		//						fly.Destroyed = true;
		//						_fliesDestroy.Play();
		//						_ant.HitPoint = Math.Max(0, _ant.HitPoint - 10);
		//					}
		//				}
		//			}
		//		}

		//		if (_antEnemy.Bounds.CollidesWith(_ant.Bounds))
		//		{
		//			_ant.Color = Color.Gray;
		//			_antEnemy.Color = Color.Gray;

		//			_damageTimer += gameTime.ElapsedGameTime.TotalSeconds;
		//			if (_damageTimer >= 0.1)
		//			{
		//				_ant.HitPoint = Math.Max(0, _ant.HitPoint - 15);
		//				_damageTimer = 0;
		//				_fliesDestroy.Play();
		//			}
		//		}

		//		if (_ant.HitPoint <= 0)
		//		{
		//			_isGameOver = true;
		//		}
		//	}

		//	base.Update(gameTime);
		//}

		//protected override void Draw(GameTime gameTime)
		//{
		//	GraphicsDevice.Clear(Color.CornflowerBlue);

		//	_spriteBatch.Begin();
		//	foreach (CropSprite crop in _crops)
		//	{
		//		crop.Draw(gameTime, _spriteBatch);
		//	}
		//	foreach (FliesSprite fly in _flies)
		//	{
		//		fly.Draw(gameTime, _spriteBatch);
		//	}

		//	_groundTexture.Draw(_spriteBatch);

		//	_spriteBatch.DrawString(_spriteFont, $"Ant HP: {_ant.HitPoint}", new Vector2(4, 2), Color.Gold);
		//	_spriteBatch.DrawString(_spriteFont, $"Crops left: {_cropsLeft}", new Vector2(4, 35), Color.Gold);
		//	_ant.Draw(gameTime, _spriteBatch);
		//	_antEnemy.Draw(gameTime, _spriteBatch);

		//	if (_isGameOver)
		//	{
		//		string message = "You Lose";
		//		Vector2 textSize = _spriteFont.MeasureString(message);
		//		Vector2 textPosition = new Vector2(
		//			(GraphicsDevice.Viewport.Width - textSize.X) / 2,
		//			(GraphicsDevice.Viewport.Height - textSize.Y) / 2);

		//		_spriteBatch.DrawString(_spriteFont, message, textPosition, Color.Red);

		//		string restartMessage = "Press R to Restart";
		//		Vector2 restartTextSize = _spriteFont.MeasureString(restartMessage);
		//		Vector2 restartTextPosition = new Vector2(
		//			(GraphicsDevice.Viewport.Width - restartTextSize.X) / 2,
		//			(GraphicsDevice.Viewport.Height - restartTextSize.Y) / 2 + 40);

		//		_spriteBatch.DrawString(_spriteFont, restartMessage, restartTextPosition, Color.White);
		//	}

		//	if (_isGameWon)
		//	{
		//		string message = "You Win";
		//		Vector2 textSize = _spriteFont.MeasureString(message);
		//		Vector2 textPosition = new Vector2(
		//			(GraphicsDevice.Viewport.Width - textSize.X) / 2,
		//			(GraphicsDevice.Viewport.Height - textSize.Y) / 2);

		//		_spriteBatch.DrawString(_spriteFont, message, textPosition, Color.Green);

		//		string restartMessage = "Press R to Restart";
		//		Vector2 restartTextSize = _spriteFont.MeasureString(restartMessage);
		//		Vector2 restartTextPosition = new Vector2(
		//			(GraphicsDevice.Viewport.Width - restartTextSize.X) / 2,
		//			(GraphicsDevice.Viewport.Height - restartTextSize.Y) / 2 + 40);

		//		_spriteBatch.DrawString(_spriteFont, restartMessage, restartTextPosition, Color.White);
		//	}

		//	_spriteBatch.End();

		//	base.Draw(gameTime);
	}
}
