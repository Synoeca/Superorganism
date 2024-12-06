using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Interfaces;

namespace Superorganism.Entities
{
    public class StaticAnimatedEntity(Vector2 position) : StaticEntity(position), IAnimated
	{
		public virtual bool IsSpriteAtlas { get; set; }
		public virtual bool HasDirection { get; set; }
		public virtual int DirectionIndex { get; set; }
		public virtual double DirectionTimer { get; set; }
		public virtual double DirectionInterval { get; set; }
		public virtual double AnimationTimer { get; set;}
		public virtual float AnimationSpeed { get; set; }
		public virtual short AnimationFrame { get; set; }

		public virtual void UpdateAnimation(GameTime gameTime)
		{
			AnimationTimer += gameTime.ElapsedGameTime.TotalSeconds;

			if (!(AnimationTimer > AnimationSpeed)) return;
			AnimationFrame++;
			if (AnimationFrame >= TextureInfo.NumOfSpriteCols) { AnimationFrame = 0; }
			AnimationTimer -= AnimationSpeed;
		}

		public void DrawAnimation(SpriteBatch spriteBatch)
        {
            switch (IsSpriteAtlas)
            {
                case true:
                {
                    Rectangle source = new((AnimationFrame * TextureInfo.TextureWidth) / TextureInfo.NumOfSpriteCols, DirectionIndex, TextureInfo.UnitTextureWidth, TextureInfo.UnitTextureHeight);
                    spriteBatch.Draw(Texture, Position, source, Color.White);
                    break;
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
