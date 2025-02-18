using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Common;

namespace Superorganism.Interfaces
{
    public interface IAnimated
    {
        bool IsSpriteAtlas { get; }
        bool HasDirection { get; }
        TextureInfo TextureInfo { get; }
        double DirectionTimer { get; }
        double DirectionInterval { get; }
        double AnimationTimer { get; }
        float AnimationSpeed { get; }
        short AnimationFrame { get; }
        void UpdateAnimation(GameTime gameTime);
        void DrawAnimation(SpriteBatch spriteBatch);
    }
}
