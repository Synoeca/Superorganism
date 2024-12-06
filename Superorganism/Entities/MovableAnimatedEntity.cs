using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.AI;
using Superorganism.Collisions;
using Superorganism.Enums;
using Superorganism.Interfaces;

namespace Superorganism.Entities
{
    public class MovableAnimatedEntity : MovableEntity, IAnimated
	{
		protected bool _flipped;
		public virtual bool IsSpriteAtlas { get; set; }
		public virtual bool HasDirection { get; set; } = true;
		public virtual double AnimationTimer { get; set; } = 0;
		public virtual float AnimationSpeed { get; set; }
		public virtual short AnimationFrame { get; set; } = 0;

		protected ICollisionBounding _collisionBounding;
		public ICollisionBounding CollisionBounding
		{
			get => _collisionBounding;
			set => _collisionBounding = value;
		}

		public virtual void UpdateAnimation(GameTime gameTime)
		{
			if (!IsSpriteAtlas) return;

			AnimationTimer += gameTime.ElapsedGameTime.TotalSeconds;

			if (AnimationTimer > AnimationSpeed)
			{
				if (HasDirection)
				{
					AnimationFrame++;
					if (AnimationFrame >= TextureInfo.NumOfSpriteCols)
					{
						AnimationFrame = 0;
					}
				}
				else
				{
					// For walking animation: use frames 1 and 2 when movings
					if (Math.Abs(_velocity.X) > 0.1f)
					{
						AnimationFrame++;
						if (AnimationFrame < 1 || AnimationFrame > 2)  // Ensure we only use frames 1 and 2
						{
							AnimationFrame = 1;
						}
					}
					else
					{
						AnimationFrame = 0;  // Idle frame
					}
				}
				AnimationTimer -= AnimationSpeed;
			}
		}

		public void DrawAnimation(SpriteBatch spriteBatch)
		{
            //if (!IsSpriteAtlas) return;

            if (HasDirection)
            {
                int directionIndex = (int)Direction;
                Rectangle source = new(AnimationFrame * (TextureInfo.TextureWidth / TextureInfo.NumOfSpriteCols),
                    directionIndex * (TextureInfo.TextureWidth / TextureInfo.NumOfSpriteCols),
                    TextureInfo.TextureWidth / TextureInfo.NumOfSpriteCols,
                    TextureInfo.TextureHeight / TextureInfo.NumOfSpriteRows);

                // Only apply horizontal flipping for sprites with less than 2 rows
                SpriteEffects effect = TextureInfo.NumOfSpriteRows < 2 && _velocity.X < 0
                    ? SpriteEffects.FlipHorizontally
                    : SpriteEffects.None;

                spriteBatch.Draw(Texture, Position, source, Color, 0f, Vector2.Zero,
                    TextureInfo.SizeScale, effect, 0f);
            }
            else
			{
				// Single row sprite with three frames (idle, walk1, walk2)
				Rectangle source = new(AnimationFrame * (TextureInfo.TextureWidth / TextureInfo.NumOfSpriteCols),
					0,  // y is always 0 for single row
					TextureInfo.TextureWidth / TextureInfo.NumOfSpriteCols,
					TextureInfo.TextureHeight);

				// Use last movement direction for flipping when stopped
				if (Math.Abs(_velocity.X) > 0.1f)
				{
					_flipped = _velocity.X < 0;
				}
				// else _flipped keeps its last value

				SpriteEffects effect = _flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

				spriteBatch.Draw(Texture, Position, source, Color, 0f, Vector2.Zero,
					TextureInfo.SizeScale, effect, 0f);
			}
		}

		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			UpdateAnimation(gameTime);
			DrawAnimation(spriteBatch);
		}

		public override void Update(GameTime gameTime)
		{
			CollisionBounding ??= TextureInfo.CollisionType;
			DecisionMaker.Action(ref _strategy, ref _strategyHistory, gameTime, ref _direction, ref _position, ref _directionTimer, ref _directionInterval, ref _collisionBounding,
				ref _velocity, 800, 420, TextureInfo, EntityStatus);
			if (CollisionBounding is BoundingRectangle br)
			{
				br.X = Position.X - ((TextureInfo.UnitTextureWidth) * TextureInfo.SizeScale / 2.0f);
				br.Y = Position.Y - ((TextureInfo.UnitTextureHeight) * TextureInfo.SizeScale / 2.0f);
				CollisionBounding = br;
			}
		}
	}
}
