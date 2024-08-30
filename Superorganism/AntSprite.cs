using CollisionExample.Collisions;
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
		public Vector2 Position()
		{
			return position;
		}

		private Vector2 velocity = Vector2.Zero;

		private bool _flipped;

		private BoundingRectangle bounds = new BoundingRectangle(new Vector2(200 - 16, 200 - 16), 32, 32);

		/// <summary>
		/// The bounding volume of the sprite
		/// </summary>
		public BoundingRectangle Bounds => bounds;

		private bool _isOnGround = true;
		public bool IsOnGround()
		{
			return _isOnGround;
		}

		private float _gravity = 0.5f;
		private float _groundLevel = 400;
		private float _jumpStrength = -14f;
		private float _movementSpeed = 3f;
		private float _friction = 0.8f;


		public Color Color { get; set; } = Color.White;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		public void LoadContent(ContentManager content)
		{
			texture = content.Load<Texture2D>("ant");
		}

		public void Update(GameTime gameTime)
		{
			gamePadState = GamePad.GetState(0);
			keyboardState = Keyboard.GetState();

			_movementSpeed = (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift)) ? 2.5f : 1f;

			// Apply the gamepad movement with inverted Y axis
			position += gamePadState.ThumbSticks.Left * new Vector2(1, -1);
			if (gamePadState.ThumbSticks.Left.X < 0) _flipped = true;
			if (gamePadState.ThumbSticks.Left.X > 0) _flipped = false;

			// Handle horizontal movement input
			if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
			{
				velocity.X = -_movementSpeed;
				_flipped = true;
			}
			else if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
			{
				velocity.X = _movementSpeed;
				_flipped = false;
			}
			else if (_isOnGround)
			{
				// Apply friction to gradually stop the sprite when on the ground
				velocity.X *= _friction;
				if (Math.Abs(velocity.X) < 0.1f)
				{
					velocity.X = 0;
				}
			}

			// Handle vertical movement input (jump)
			if (_isOnGround && keyboardState.IsKeyDown(Keys.Space))
			{
				velocity.Y = _jumpStrength;
				_isOnGround = false; // The sprite is now in the air
			}

			// Apply gravity to the velocity
			velocity.Y += _gravity;

			// Update position based on velocity
			position += velocity;

			// Simple ground collision detection
			if (position.Y >= _groundLevel)
			{
				position.Y = _groundLevel; // Keep the sprite on the ground
				velocity.Y = 0; // Stop the downward velocity when on the ground
				_isOnGround = true;
			}

			// Update the bounds
			bounds.X = position.X - 16;
			bounds.Y = position.Y - 16;

			velocity.X = MathHelper.Clamp(velocity.X, -_movementSpeed * 2, _movementSpeed * 2);
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			SpriteEffects spriteEffects = (_flipped) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			spriteBatch.Draw(texture, position, null, Color, 0, new Vector2(120, 120), 0.25f, spriteEffects, 0);
		}

		
	}
}
