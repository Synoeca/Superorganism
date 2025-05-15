using Superorganism.Tiles;
using System;
using Microsoft.Xna.Framework;
using Superorganism.Common;

namespace Superorganism.Core.Managers
{
    /// <summary>
    /// 
    /// </summary>
    public static class GameState
    {
        /// <summary>
        /// 
        /// </summary>
        private static GameStateOrganizer _instance;

        /// <summary>
        /// 
        /// </summary>
        public static string CurrentMapName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="organizer"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Initialize(GameStateOrganizer organizer)
        {
            _instance = organizer ?? throw new ArgumentNullException(nameof(organizer));
        }

        /// <summary>
        /// 
        /// </summary>
        public static TiledMap CurrentMap
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("GameStateOrganizer not initialized. Call Initialize() first.");
                return _instance.CurrentMap;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static EntityStatus GetPlayerEntityStatus()
        {
            if (_instance == null)
                throw new InvalidOperationException("GameStateOrganizer not initialized. Call Initialize() first.");
            return _instance.GetPlayerEntityStatus;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static Vector2 GetPlayerPosition()
        {
            if (_instance == null)
                throw new InvalidOperationException("GameStateOrganizer not initialized. Call Initialize() first.");
            return _instance.GetPlayerPosition();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static float GetPlayerHealth()
        {
            if (_instance == null)
                throw new InvalidOperationException("GameStateOrganizer not initialized. Call Initialize() first.");
            return _instance.GetPlayerHealth();
        }
    }
}
