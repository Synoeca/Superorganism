using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Superorganism.Collisions;
using System;

namespace Superorganism
{
	public class AntSprite : MovableEntity
	{
		public int HitPoint { get; set; } = 100;
		public int MaxHitPoint { get; private set; } = 100;
	}
}
