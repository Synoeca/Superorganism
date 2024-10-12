using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct3D9;
using Superorganism.Collisions;
using Superorganism.Enums;

namespace Superorganism.Entities
{
    public class MoveableEntity : Entity, IAnimated
    {
	    protected DecisionMaker Decision = new DecisionMaker();

	    public MovementState Movement { get; set; }

		public bool Flipped { get; set; } = false;

		public bool IsSpriteAtlas { get; set; } = true;

		public bool HasDirection { get; set; }

		public Direction Direction { get; set; }

		public int DirectionIndex { get; set; }

		public int NumOfSpriteCols { get; set; }

		public int NumOfSpriteRows { get; set; }

		public double AnimationTimer { get; set; }

		public float AnimationInterval { get; set; }

		public short AnimationFrame { get; set; }

		public override Texture2D Texture { get; set; }

		public override Color Color { get; set; }

		private Vector2 _position;

		public override Vector2 Position
		{
			get => _position;
			set
			{
				_position = value;
				Movement.Position = _position;
			}
		}

		public override void LoadContent(ContentManager content, string assetName)
		{
			Texture = content.Load<Texture2D>(assetName);
		}

		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			Vector2 origin = new((Texture.Width/ NumOfSpriteCols) / 2f, (Texture.Height / NumOfSpriteRows) / 2f);
			SpriteEffects spriteEffects = Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			spriteBatch.Draw(Texture, Position, null, Color, 0, origin, 1f, spriteEffects, 0);
		}

		public virtual void Update(GameTime gameTime, List<Entity> positions)
		{
			this.Movement.Velocity = Decision.Action();
		}

		public virtual void UpdateAnimation(GameTime gameTime)
		{
			AnimationTimer += gameTime.ElapsedGameTime.TotalSeconds;

			if (AnimationTimer > AnimationInterval)
			{
				AnimationFrame++;
				if (AnimationFrame > 3)
				{
					AnimationFrame = 1;
				}
				AnimationTimer -= 0.04;
			}

			if (!HasDirection) return;

			if (Direction == Direction.Up)
			{
				DirectionIndex = (int)Direction.Down;
			}

			else if (Direction == Direction.Down)
			{
				DirectionIndex = (int)Direction.Up;
			}
		}

		public virtual void DrawAnimation(SpriteBatch spriteBatch)
		{
			Rectangle source = new Rectangle(AnimationFrame * (Texture.Width / NumOfSpriteCols), DirectionIndex * (Texture.Height / NumOfSpriteRows), (Texture.Width / NumOfSpriteCols), (Texture.Height / NumOfSpriteRows));
			spriteBatch.Draw(Texture, Position, source, Color.White);
		}
    }
}
