using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism
{
	public class AntEnemySprite
	{
		private Texture2D texture;
		private Vector2 position = new Vector2(400, 400);
		private Vector2 velocity = Vector2.Zero;

		private bool _flipped;
		private bool _isOnGround = true;

		private float _gravity = 0.5f;
		private float _groundLevel = 400;
		private float _movementSpeed = 2.5f;
		private float _friction = 0.8f;
		private float _chaseSpeed = 2f;
		private float _chaseThreshold = 150f;
		private float _acceleration = 0.05f;

		private bool _movingRight = true;

		public Color Color { get; set; } = Color.White;

		public void LoadContent(ContentManager content)
		{
			texture = content.Load<Texture2D>("antEnemy");
		}

		public void Update(GameTime gameTime, Vector2 playerPosition)
		{
			float distanceToPlayerX = Math.Abs(position.X - playerPosition.X);

			if (distanceToPlayerX < _chaseThreshold)
			{
				float targetVelocityX = playerPosition.X > position.X ? _movementSpeed : -_movementSpeed;

				velocity.X = MathHelper.Lerp(velocity.X, targetVelocityX, _acceleration);

				_flipped = velocity.X < 0;
			}
			else
			{
				float targetVelocityX = _movingRight ? _movementSpeed : -_movementSpeed;
				velocity.X = MathHelper.Lerp(velocity.X, targetVelocityX, _acceleration);

				if (position.X <= 100)
				{
					_movingRight = true;
				}
				else if (position.X >= 700)
				{
					_movingRight = false;
				}

				_flipped = velocity.X < 0;
			}


			velocity.Y += _gravity;

			position += velocity;

			// Simple ground collision detection
			if (position.Y >= _groundLevel)
			{
				position.Y = _groundLevel;
				velocity.Y = 0;
				_isOnGround = true;
			}

			// Apply friction to the horizontal movement if on the ground and not chasing the player
			if (_isOnGround && distanceToPlayerX >= _chaseThreshold)
			{
				velocity.X *= _friction;
				if (Math.Abs(velocity.X) < 0.1f)
				{
					velocity.X = 0;
				}
			}
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			SpriteEffects spriteEffects = (_flipped) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			spriteBatch.Draw(texture, position, null, Color, 0, new Vector2(120, 120), 0.25f, spriteEffects, 0);
		}
	}
}
