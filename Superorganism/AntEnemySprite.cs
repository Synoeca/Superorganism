using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using Superorganism.Collisions;

namespace Superorganism
{
	public class AntEnemySprite : MovableEntity
	{
		public AntEnemySprite(Vector2 position) : base(position)
		{
			((AnimatedEntity)this).Position = position;
		}
	}
}
