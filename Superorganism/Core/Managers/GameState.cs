using Superorganism.Tiles;
using System;
using Microsoft.Xna.Framework;

namespace Superorganism.Core.Managers
{
    public static class GameState
    {
        private static GameStateOrganizer _instance;

        public static string CurrentMapName { get; set; }

        public static void Initialize(GameStateOrganizer organizer)
        {
            _instance = organizer ?? throw new ArgumentNullException(nameof(organizer));
        }

        public static TiledMap CurrentMap
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("GameStateOrganizer not initialized. Call Initialize() first.");
                return _instance.CurrentMap;
            }
        }

        public static Vector2 GetPlayerPosition()
        {
            if (_instance == null)
                throw new InvalidOperationException("GameStateOrganizer not initialized. Call Initialize() first.");
            return _instance.GetPlayerPosition();
        }

        public static int GetPlayerHealth()
        {
            if (_instance == null)
                throw new InvalidOperationException("GameStateOrganizer not initialized. Call Initialize() first.");
            return _instance.GetPlayerHealth();
        }
    }
}
