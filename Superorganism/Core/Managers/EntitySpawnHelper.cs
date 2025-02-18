using Superorganism.Entities;
using Superorganism.Tiles;
using Microsoft.Xna.Framework;

namespace Superorganism.Core.Managers
{
    public static class EntitySpawnHelper
    {
        public static void InitializeAtTile(this Entity entity, int tileX, int tileY)
        {
            Vector2 worldPos = MapHelper.TileToWorld(tileX, tileY);
            entity.Position = worldPos;
        }
    }
}
