using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Interfaces
{
    public interface IEntity
    {
        /// <summary>
        /// 
        /// </summary>
        Texture2D Texture { get; }

        /// <summary>
        /// 
        /// </summary>
        Vector2 Position { get; }

        /// <summary>
        /// 
        /// </summary>
        Color Color { get; }
    }
}
