﻿using Microsoft.Xna.Framework;
using Superorganism.Collisions;

namespace Superorganism.Common
{
	public class TextureInfo
	{
		public int TextureWidth {get; set; }

		public int TextureHeight { get; set; }

		public int NumOfSpriteCols { get; set; }

		public int NumOfSpriteRows { get; set; }

		public int UnitTextureWidth => TextureWidth / NumOfSpriteCols;

		public int UnitTextureHeight => TextureHeight / NumOfSpriteRows;

		public Vector2 Center { get; set; }

		public float SizeScale { get; set; } = 1;

		public ICollisionBounding CollisionType { get; set; }
	}
}
