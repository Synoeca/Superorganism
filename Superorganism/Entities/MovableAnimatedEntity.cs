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
		protected bool Flipped;
		public virtual bool IsSpriteAtlas { get; set; }
		public virtual bool HasDirection { get; set; } = true;
		public virtual double AnimationTimer { get; set; }
		public virtual float AnimationSpeed { get; set; }
		public virtual short AnimationFrame { get; set; }

        protected bool UseRotation { get; set; }
        protected float Rotation { get; set; }
        protected const float RotationSmoothing = 0.05f;

        protected virtual void UpdateRotation()
        {
            switch (UseRotation)
            {
                case true when _velocity != Vector2.Zero:
                {
                    if (Math.Abs(_velocity.X) > Math.Abs(_velocity.Y))
                    {
                        float targetRotation = (float)Math.Atan2(_velocity.Y, _velocity.X);

                        // Adjust rotation based on sprite's base direction
                        if (Direction == Direction.Left)
                        {
                            targetRotation += MathHelper.Pi; // Add 180 degrees
                        }
                        // Direction.Right uses the base rotation

                        // Normalize rotation to keep it between -Pi and Pi
                        while (targetRotation > MathHelper.Pi)
                            targetRotation -= MathHelper.TwoPi;
                        while (targetRotation < -MathHelper.Pi)
                            targetRotation += MathHelper.TwoPi;

                        // Calculate the shortest rotation path
                        float diff = targetRotation - Rotation;
                        switch (diff)
                        {
                            case > MathHelper.Pi:
                                targetRotation -= MathHelper.TwoPi;
                                break;
                            case < -MathHelper.Pi:
                                targetRotation += MathHelper.TwoPi;
                                break;
                        }

                        // Smooth rotation transition
                        Rotation = MathHelper.Lerp(Rotation, targetRotation, RotationSmoothing);
                    }
                    else
                    {
                        // Reset rotation when moving vertically
                        Rotation = 0f;
                    }

                    break;
                }
            }
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
            if (HasDirection)
            {
                int directionIndex = (int)Direction;
                Rectangle source = new(
                    (int)(AnimationFrame * (TextureInfo.TextureWidth / TextureInfo.NumOfSpriteCols)),
                    (int)(directionIndex * (TextureInfo.TextureWidth / TextureInfo.NumOfSpriteCols)),
                    (int)(TextureInfo.TextureWidth / TextureInfo.NumOfSpriteCols),
                    (int)(TextureInfo.TextureHeight / TextureInfo.NumOfSpriteRows)
                );

                // Calculate origin for rotation
                Vector2 origin = UseRotation ?
                    new Vector2(source.Width / 2f, source.Height / 2f) :
                    Vector2.Zero;

                // Adjust position if using origin
                Vector2 drawPosition = UseRotation ?
                    Position + (origin * TextureInfo.SizeScale) :
                    Position;

                // Only apply horizontal flipping for sprites with less than 2 rows
                SpriteEffects effect = TextureInfo.NumOfSpriteRows < 2 && _velocity.X < 0
                    ? SpriteEffects.FlipHorizontally
                    : SpriteEffects.None;

                spriteBatch.Draw(
                    Texture,
                    drawPosition,
                    source,
                    Color,
                    UseRotation ? Rotation : 0f,
                    origin,
                    TextureInfo.SizeScale,
                    effect,
                    0f
                );
            }
            else
			{
				// Single row sprite with three frames (idle, walk1, walk2)
				Rectangle source = new((int)(AnimationFrame * (TextureInfo.TextureWidth / TextureInfo.NumOfSpriteCols)),
					0,  // y is always 0 for single row
					(int)(TextureInfo.TextureWidth / TextureInfo.NumOfSpriteCols),
					(int)TextureInfo.TextureHeight);

				// Use last movement direction for flipping when stopped
				if (Math.Abs(_velocity.X) > 0.1f)
				{
					Flipped = _velocity.X < 0;
				}
				// else Flipped keeps its last value

				SpriteEffects effect = Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

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
            if (UseRotation) { UpdateRotation(); }

            DecisionMaker.Action(ref _strategy, ref _strategyHistory, gameTime, ref _direction, ref _position, ref _directionTimer, ref _directionInterval, ref _collisionBounding,
				ref _velocity, 800, 420, TextureInfo, EntityStatus);
		}
	}
}
