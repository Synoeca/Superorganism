using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Superorganism
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
	}
}
