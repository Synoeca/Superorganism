﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Collisions;
using Superorganism.Enums;

namespace Superorganism.Entities
{
	public class MovableAnimatedDestroyableEntity(Vector2 position) : MovableAnimatedEntity(position), ICollidable
	{
		public bool Destroyed { get; set; } = false;

		//public override void Update(GameTime gameTime)
		//{
			
		//}

		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			if (Destroyed)
			{
				return;
			}
			UpdateAnimation(gameTime);
			DrawAnimation(spriteBatch);
		}
	}
}