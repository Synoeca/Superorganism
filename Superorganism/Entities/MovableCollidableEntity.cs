using Microsoft.Xna.Framework;
using Superorganism.Collisions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Entities
{
	public class MovableCollidableEntity : MoveableEntity, ICollidable
	{
		public virtual ICollisionBounding CollisionBounding { get; set; }

		public bool Destroyed { get; set; } = false;

		private Vector2 _position;
		public override Vector2 Position
		{
			get => _position;
			set
			{
				_position = value;
				if (Texture != null)
				{
					CollisionBounding.Center = _position + new Vector2(((Texture.Height / NumOfSpriteCols) / 2), ((Texture.Width / NumOfSpriteRows) / 2)); // Update the bounds center when position changes
				}
			}
		}

		public override void Update(GameTime gameTime, List<Entity> entities)
		{
			if (Destroyed) { return; }

			Decision.GameTime = gameTime;
			Decision.Entities = entities;
			this.Movement.Velocity = Decision.Action();
		}
	}
}
