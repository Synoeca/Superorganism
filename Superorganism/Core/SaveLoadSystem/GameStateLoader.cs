using Superorganism.Core.Managers;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Superorganism.Entities;

namespace Superorganism.Core.SaveLoadSystem
{
    public static class GameStateLoader
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            Converters = { new Vector2Converter() }
        };

        public static GameStateInfo LoadGameState(string saveFile)
        {
            string savePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Superorganism", "Saves", saveFile);

            try
            {
                string jsonContent = File.ReadAllText(savePath);
                GameStateContent savedState = JsonSerializer.Deserialize<GameStateContent>(jsonContent, SerializerOptions);

                return RestoreGameState(savedState);
            }
            catch (Exception)
            {
                return CreateNewGameState();
            }
        }

        public static GameStateInfo RestoreGameState(GameStateContent savedState)
        {
            GameStateInfo gameState = new GameStateInfo
            {
                GameProgressTime = TimeSpan.FromSeconds(savedState.GameProgressTime.TotalSeconds),
                Entities = []
            };

            foreach (EntityData entityData in savedState.Entities)
            {
                Entity entity = CreateEntity(entityData);
                if (entity != null)
                {
                    gameState.Entities.Add(entity);
                }
            }

            return gameState;
        }

        private static Entity CreateEntity(EntityData data)
        {
            switch (data.Type)
            {
                case "Ant":
                    return new Ant
                    {
                        Position = data.Position,
                        HitPoints = data.Health
                    };

                case "AntEnemy":
                    AntEnemy enemy = new AntEnemy
                    {
                        Position = data.Position,
                        //Health = data.Health,
                        Strategy = data.CurrentStrategy,
                        StrategyHistory = data.StrategyHistory.Select(sh =>
                            (sh.Strategy, sh.StartTime, sh.LastActionTime)).ToList()
                    };
                    return enemy;

                case "Crop":
                    return new Crop
                    {
                        Position = data.Position,
                        //Health = data.Health
                    };

                case "Fly":
                    return new Fly
                    {
                        Position = data.Position,
                        //Health = data.Health
                    };

                default:
                    return null;
            }
        }

        private static GameStateInfo CreateNewGameState()
        {
            return new GameStateInfo
            {
                Entities = [new Ant { Position = new Vector2(100, 100), HitPoints = 100 }],
                GameProgressTime = TimeSpan.Zero
            };
        }
    }
}
