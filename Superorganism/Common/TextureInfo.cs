using Microsoft.Xna.Framework;
using Superorganism.Collisions;

namespace Superorganism.Common
{
	public class TextureInfo
	{
		public float TextureWidth {get; set; }

		public float TextureHeight { get; set; }

		public int NumOfSpriteCols { get; set; }

		public int NumOfSpriteRows { get; set; }

		public float UnitTextureWidth => TextureWidth / NumOfSpriteCols;

		public float UnitTextureHeight => TextureHeight / NumOfSpriteRows;

		public Vector2 Center { get; set; }

		public float SizeScale { get; set; } = 1;

		public ICollisionBounding CollisionType { get; set; }
	}
}
