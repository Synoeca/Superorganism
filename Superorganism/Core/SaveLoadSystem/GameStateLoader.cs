using ContentPipeline;
using Superorganism.Core.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism.Core.SaveLoadSystem
{
    public static class GameStateLoaderObsolete
    {
        //public static void RestoreGameState(GameStateManager manager, GameStateContent state)
        //{
        //    manager.IsGameOver = state.IsGameOver;
        //    manager.IsGameWon = state.IsGameWon;
        //    manager.CropsLeft = state.CropsLeft;
        //    manager.ElapsedTime = state.ElapsedTime;

        //    // Restore entity states
        //    manager.SetPlayerPosition(state.PlayerPosition);
        //    manager.SetPlayerHealth(state.PlayerHealth);
        //    int enemyCount = Math.Min(state.EnemyPositions.Length, manager.GetEnemyCount());
        //    for (int i = 0; i < enemyCount; i++)
        //    {
        //        manager.SetEnemyPosition(i, state.EnemyPositions[i]);
        //        manager.SetEnemyStrategy(i, state.EnemyStrategies[i]);
        //    }
        //}
    }
}
