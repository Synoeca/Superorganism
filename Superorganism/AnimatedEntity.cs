using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Collisions;

namespace Superorganism
{
	public abstract class AnimatedEntity : IEntity, IAnimated
	{
		public Texture2D Texture { get; set; }

		private const float ANIMATION_SPEED = 0.1f;

		private double animationTimer;

		private int animationFrame;

		public bool Collected { get; set; } = false;

		protected Vector2 _position;

		public Vector2 Position
		{
			get => _position;
			set => _position = value;
		}

		private BoundingCircle _bounds;

		public BoundingCircle Bounds => _bounds;

		// Animation variables
		protected float _animationTimer = 0f;
		protected float _animationInterval = 0.15f;

		public AnimatedEntity(Vector2 position)
		{
			this._position = position;
			this._bounds = new BoundingCircle(position + new Vector2(8, 8), 8);
		}

		public virtual void UpdateAnimation(GameTime gameTime)
		{
			animationTimer += gameTime.ElapsedGameTime.TotalSeconds;

			if (animationTimer > ANIMATION_SPEED)
			{
				animationFrame++;
				if (animationFrame > 7) animationFrame = 0;
				animationTimer -= ANIMATION_SPEED;
			}
		}
		public virtual void DrawAnimation(SpriteBatch spriteBatch)
		{
			var source = new Rectangle(animationFrame * 16, 0, 16, 16);
			spriteBatch.Draw(Texture, Position, source, Color.White);
		}

		public void Update(bool collected)
		{
			this.Collected = collected;
		}

		public virtual void LoadContent(ContentManager content)
		{
			Texture = content.Load<Texture2D>("crops");
		}

		public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			if (Collected)
			{
				return;
			}
			UpdateAnimation(gameTime);
			DrawAnimation(spriteBatch);
		}
	}
}
