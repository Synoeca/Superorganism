using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Superorganism.Collisions;
using Superorganism.Entities;
using Superorganism.Tiles;

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
