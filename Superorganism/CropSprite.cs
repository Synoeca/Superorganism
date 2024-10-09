using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Collisions;

namespace Superorganism
{
	public class CropSprite : AnimatedEntity
	{
		public CropSprite(Vector2 position) : base(position)
		{
			this.Position = position;
		}
	}
}
