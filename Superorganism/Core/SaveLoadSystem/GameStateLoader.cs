using ContentPipeline;
using Superorganism.Core.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism.Core.SaveLoadSystem
{
    public static class GameStateLoader
    {
        public static void RestoreGameState(GameStateManager manager, GameStateContent state)
        {
            manager.IsGameOver = state.IsGameOver;
            manager.IsGameWon = state.IsGameWon;
            manager.CropsLeft = state.CropsLeft;
            manager.ElapsedTime = state.ElapsedTime;

            // Restore entity states
            manager.SetPlayerPosition(state.PlayerPosition);
            manager.SetPlayerHealth(state.PlayerHealth);
            manager.SetEnemyPosition(state.EnemyPosition);
            manager.SetEnemyStrategy(state.CurrentEnemyStrategy);
        }
    }
}
