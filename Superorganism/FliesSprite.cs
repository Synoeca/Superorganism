using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using Superorganism.Collisions;

namespace Superorganism
{
	public enum Direction
	{
		Down = 0,
		Right = 1,
		Up = 2,
		Left = 3
	}

	public class FliesSprite
	{
		private Texture2D texture;
		private double directionTimer;
		private double animationTimer;
		private short animationFrame = 1;
		private double directionChangeInterval;
		private Vector2 velocity;
		private static Random rand = new Random();

		private BoundingCircle bounds;

		public bool Destroyed { get; set; } = false;

		private Vector2 position;
		public Vector2 Position
		{
			get => position;
			set
			{
				position = value;
				bounds.Center = position + new Vector2(16, 16); // Update the bounds center when position changes
			}
		}

		public BoundingCircle Bounds => bounds;

		public Direction Direction { get; set; }

		public FliesSprite()
		{
			directionChangeInterval = rand.NextDouble() * 3.0 + 1.0;
			AssignRandomVelocity();
			position = Vector2.Zero; // Initialize position
			bounds = new BoundingCircle(position + new Vector2(16, 16), 16); // Adjust radius as needed
		}

		public void LoadContent(ContentManager content)
		{
			texture = content.Load<Texture2D>("flies");
		}

		private void AssignRandomVelocity()
		{
			double angle = rand.NextDouble() * Math.PI * 2;
			velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 100;
		}

		public void Update(GameTime gameTime, int screenWidth, int groundHeight)
		{
			if (Destroyed) return; // Skip update if destroyed

			directionTimer += gameTime.ElapsedGameTime.TotalSeconds;

			if (directionTimer > directionChangeInterval)
			{
				AssignRandomVelocity();
				directionTimer -= directionChangeInterval;
				directionChangeInterval = rand.NextDouble() * 3.0 + 1.0;
			}

			Position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

			if (Position.X < 0 || Position.X > screenWidth - 32)
			{
				velocity.X = -velocity.X;
				Position = new Vector2(Math.Clamp(Position.X, 0, screenWidth - 32), Position.Y);
			}

			if (Position.Y < 0 || Position.Y > groundHeight - 32)
			{
				velocity.Y = -velocity.Y;
				Position = new Vector2(Position.X, Math.Clamp(Position.Y, 0, groundHeight - 32));
			}

			Direction = Math.Abs(velocity.X) > Math.Abs(velocity.Y)
				? velocity.X > 0 ? Direction.Right : Direction.Left
				: velocity.Y > 0 ? Direction.Down : Direction.Up;
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			if (Destroyed) { return; }

			animationTimer += gameTime.ElapsedGameTime.TotalSeconds;

			if (animationTimer > 0.04)
			{
				animationFrame++;
				if (animationFrame > 3)
				{
					animationFrame = 1;
				}
				animationTimer -= 0.04;
			}

			int directionIndex = (int)Direction;
			if (Direction == Direction.Up)
			{
				directionIndex = (int)Direction.Down;
			}
			else if (Direction == Direction.Down)
			{
				directionIndex = (int)Direction.Up;
			}

			Rectangle source = new Rectangle(animationFrame * 32, directionIndex * 32, 32, 32);
			spriteBatch.Draw(texture, Position, source, Color.White);
		}

		public bool CollidesWith(BoundingCircle other)
		{
			return bounds.CollidesWith(other);
		}
	}
}
