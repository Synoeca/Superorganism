using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ContentPipeline
{
    [ContentSerializerRuntimeType("Superorganism.Core.SaveLoadSystem.GameState, Superorganism")]
    public class GameStateContentObsolete
    {
        public bool IsGameOver { get; set; }
        public bool IsGameWon { get; set; }
        public int CropsLeft { get; set; }
        public double ElapsedTime { get; set; }
        

        public Vector2 PlayerPosition { get; set; }
        public int PlayerHealth { get; set; }
        public Vector2[] EnemyPositions { get; set; }
        public string[] EnemyStrategies { get; set; }

        public string SaveFilename { get; set; }
    }
}
