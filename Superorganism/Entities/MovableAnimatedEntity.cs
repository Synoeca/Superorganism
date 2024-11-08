﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Enums;

namespace Superorganism.Entities
{
	public class MovableAnimatedEntity(Vector2 position) : MoveableEntity(position), IAnimated
	{
		public virtual bool IsSpriteAtlas { get; set; }
		public virtual bool HasDirection { get; set; } = true;
		//public virtual int NumOfSpriteCols { get; set; }
		//public virtual int NumOfSpriteRows { get; set; }
		public virtual int DirectionIndex { get; set; }
		//public virtual double DirectionTimer { get; set; }
		//public virtual double DirectionInterval { get; set; }
		public virtual double AnimationTimer { get; set; } = 0;
		public virtual float AnimationSpeed { get; set; }
		public virtual short AnimationFrame { get; set; } = 0;

		public void UpdateAnimation(GameTime gameTime)
		{
			AnimationTimer += gameTime.ElapsedGameTime.TotalSeconds;

			if (!(AnimationTimer > AnimationSpeed)) return;
			AnimationFrame++;
			if (AnimationFrame >= TextureInfo.NumOfSpriteCols) { AnimationFrame = 0; }
			AnimationTimer -= AnimationSpeed;
		}

		public void DrawAnimation(SpriteBatch spriteBatch)
		{
			if (IsSpriteAtlas)
			{
				if (HasDirection)
				{
					Rectangle source = new(AnimationFrame * (TextureInfo.TextureWidth / TextureInfo.NumOfSpriteCols), DirectionIndex * (TextureInfo.TextureWidth / TextureInfo.NumOfSpriteCols), (TextureInfo.TextureWidth / TextureInfo.NumOfSpriteCols), (TextureInfo.TextureHeight / TextureInfo.NumOfSpriteRows));
					spriteBatch.Draw(Texture, Position, source, Color.White);
				}
			}
		}

		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			UpdateAnimation(gameTime);
			DrawAnimation(spriteBatch);
		}
	}
}
