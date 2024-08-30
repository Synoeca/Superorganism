using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpriteExample;
using System;

namespace Superorganism
{
	public class Superorganism : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		private CropSprite[] _crops;
		private int _cropsLeft;
		private BatSprite[] _bats;
		private AntSprite _ant;
		private AntEnemySprite _antEnemy;

		private Texture2D _groundTexture;
		private int _groundHeight = 100;
		private int _groundY = 400;

		public Superorganism()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			// Initialize crops
			Random rand = new();
			_crops =
			[
				new(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, 383)),
				new(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, 383)),
				new(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, 383)),
				new(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, 383)),
				new(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, 383)),
				new(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, 383)),
				new(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, 383)),
				new(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, 383)),
				new(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, 383)),
				new(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, 383)),
				new(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, 383)),
				new(new Vector2((float)rand.NextDouble() * GraphicsDevice.Viewport.Width, 383)),
			];
			_cropsLeft = _crops.Length;

			int numberOfBats = rand.Next(3, 21);
			_bats = new BatSprite[numberOfBats];

			for (int i = 0; i < numberOfBats; i++)
			{
				float xPos = rand.Next(0, 800);
				float yPos = rand.Next(0, 600);
				Direction randomDirection = (Direction)rand.Next(0, 4);

				_bats[i] = new BatSprite
				{
					Position = new Vector2(xPos, yPos),
					Direction = randomDirection
				};
			}

			_ant = new AntSprite();
			_antEnemy = new AntEnemySprite();

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			foreach (CropSprite crop in _crops)
			{
				crop.LoadContent(Content);
			}
			foreach (BatSprite bat in _bats)
			{
				bat.LoadContent(Content);
			}
			_ant.LoadContent(Content);
			_antEnemy.LoadContent(Content);
			_groundTexture = new Texture2D(GraphicsDevice, 1, 1);
			_groundTexture.SetData(new[] { new Color(139, 69, 19) });
			// TODO: use this.Content to load your game content here
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			_ant.Update(gameTime);
			_antEnemy.Update(gameTime, _ant.Position());

			_ant.Color = Color.White;
			foreach (CropSprite crop in _crops)
			{
				if (!crop.Collected && crop.Bounds.CollidesWith(_ant.Bounds))
				{
					_ant.Color = Color.Red;
					crop.Collected = true;
					_cropsLeft--;
				}
			}
			foreach (BatSprite bat in _bats)
			{
				bat.Update(gameTime);
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			
			_spriteBatch.Begin();
			foreach (CropSprite crop in _crops)
			{
				crop.Draw(gameTime, _spriteBatch);
			}
			foreach (BatSprite bat in _bats)
			{
				bat.Draw(gameTime, _spriteBatch);
			}
			// Draw the ground texture
			_spriteBatch.Draw(_groundTexture, new Rectangle(0, _groundY, _graphics.PreferredBackBufferWidth, _groundHeight), Color.White);
			_ant.Draw(gameTime, _spriteBatch);
			_antEnemy.Draw(gameTime, _spriteBatch);
			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
