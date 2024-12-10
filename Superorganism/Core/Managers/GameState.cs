using Superorganism.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Superorganism.Core.Managers
{
    public static class GameState
    {
        private static GameStateManager _instance;

        public static void Initialize(GameStateManager manager)
        {
            _instance = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public static Map CurrentMap
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("GameStateManager not initialized. Call Initialize() first.");
                return _instance.CurrentMap;
            }
        }

        public static Vector2 GetPlayerPosition()
        {
            if (_instance == null)
                throw new InvalidOperationException("GameStateManager not initialized. Call Initialize() first.");
            return _instance.GetPlayerPosition();
        }

        public static int GetPlayerHealth()
        {
            if (_instance == null)
                throw new InvalidOperationException("GameStateManager not initialized. Call Initialize() first.");
            return _instance.GetPlayerHealth();
        }
    }
}
