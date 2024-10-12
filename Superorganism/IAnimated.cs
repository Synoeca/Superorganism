using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism
{
	public interface IAnimated
	{
		bool IsSpriteAtlas { get; }
		bool HasDirection { get; }
		int NumOfSpriteCols { get; }
		int NumOfSpriteRows { get; }
		int DirectionIndex { get; }
		double AnimationTimer { get; }
		float AnimationInterval { get; }
		short AnimationFrame { get; }
		void UpdateAnimation(GameTime gameTime);
		void DrawAnimation(SpriteBatch spriteBatch);
	}
}
