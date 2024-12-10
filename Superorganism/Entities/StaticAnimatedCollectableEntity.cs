using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Collisions;
using Superorganism.Interfaces;

namespace Superorganism.Entities
{
    public class StaticAnimatedCollectableEntity : StaticAnimatedEntity, ICollectable, ICollidable
	{
		public bool Collected { get; set; }

		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			if (Collected) return;
			UpdateAnimation(gameTime);
			DrawAnimation(spriteBatch);
		}
	}
}
