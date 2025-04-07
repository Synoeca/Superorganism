using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Superorganism.AI;
using Superorganism.Collisions;
using Superorganism.Core.Camera;
using Superorganism.Entities;
using Superorganism.ScreenManagement;
using Superorganism.Tiles;

namespace Superorganism.Core.Managers
{
    public class GameStateManager
    {
        private readonly EntitySpawner _entitySpawner;
        private readonly GameAudioManager _audioManager;
        private readonly InputAction _pauseAction;
        private readonly Camera2D _camera;
        private readonly TiledMap _map;
        private readonly ContentManager _content;

        public TiledMap CurrentMap => _map;

        public bool IsGameOver { get; set; }
        public bool IsGameWon { get; set; }
        public int CropsLeft { get; set; }
        public double ElapsedTime { get; set; }

        public GameTime GameTime { get; set; }

        private double _enemyCollisionTimer;
        private const double EnemyCollisionInterval = 0.2;

        public GameStateManager(Game game, ContentManager content, GraphicsDevice graphicsDevice, 
            Camera2D camera, GameAudioManager audio, TiledMap map, GameStateInfo gameStateInfo)
        {
            DecisionMaker.Entities.Clear();
            _audioManager = audio;
            _camera = camera;
            _map = map;
            _content = content;

            _entitySpawner = new EntitySpawner(game, content, graphicsDevice, map, gameStateInfo);

            _pauseAction = new InputAction(
                [Buttons.Start, Buttons.Back],
                [Keys.Back, Keys.Escape],
                true);

            InitializeGameState();
        }

        // Existing methods remain unchanged
        public void InitializeAudio(float soundEffectVolume, float musicVolume)
        {
            _audioManager.Initialize(soundEffectVolume, musicVolume);
        }

        private void InitializeGameState()
        {
            IsGameOver = false;
            IsGameWon = false;
            ElapsedTime = 0;
            _enemyCollisionTimer = 0;
            CropsLeft = _entitySpawner.CropsCount;
        }

        // Updated methods to handle multiple enemies
        public Vector2[] GetEnemyPositions() => _entitySpawner.GetEnemyPositions();

        public Strategy[] GetEnemyStrategies() => _entitySpawner.GetEnemyStrategies();

        public ICollisionBounding[] GetEnemyBoundings() => _entitySpawner.GetEnemyCollisionBoundings();

        public float GetEntityDistance(Entity entity1, Entity entity2) => Vector2.Distance(
            entity1.Position,
            entity2.Position
        );

        public float GetDistanceToPlayer(Entity entity) => Vector2.Distance(
            _entitySpawner.PlayerPosition,
            entity.Position
        );

        // Updated to return closest enemy distance
        public float GetEnemyDistanceToPlayer()
        {
            Vector2[] enemyPositions = GetEnemyPositions();
            float closestDistance = float.MaxValue;

            foreach (Vector2 enemyPos in enemyPositions)
            {
                float distance = Vector2.Distance(_entitySpawner.PlayerPosition, enemyPos);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                }
            }

            return closestDistance;
        }

        // Updated to return array of strategy histories
        public List<(Strategy Strategy, double StartTime, double LastActionTime)>[] GetEnemyStrategyHistories()
            => _entitySpawner.GetEnemyStrategyHistories();

        public void Update(GameTime gameTime)
        {
            if (IsGameOver || IsGameWon) return;
            GameTime = gameTime;
            UpdateTimers(gameTime);
            _entitySpawner.Update(gameTime);
            CheckCollisions();
            CheckWinLoseConditions();
        }

        private void UpdateTimers(GameTime gameTime)
        {
            ElapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

            if (_enemyCollisionTimer > 0)
            {
                _enemyCollisionTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        private void CheckCollisions()
        {
            // Handle crop collisions
            if (_entitySpawner.CheckCropCollisions())
            {
                CropsLeft--;
                _audioManager.PlayCropPickup();
            }

            // Handle enemy collisions with timer
            if (_entitySpawner.IsCollidingWithEnemy())
            {
                if (_enemyCollisionTimer <= 0)
                {
                    if (!_entitySpawner.IsPlayerInvincible)
                    {
                        _entitySpawner.ApplyEnemyDamage();
                        _audioManager.PlayFliesDestroy();
                        _enemyCollisionTimer = EnemyCollisionInterval;
                        _camera.StartShake(0.5f);
                    }
                }
                else if (!_entitySpawner.IsPlayerInvincible)
                {
                    _entitySpawner.ResetEntityColors();
                }
            }

            // Handle fly collisions
            if (_entitySpawner.CheckFlyCollisions())
            {
                _audioManager.PlayFliesDestroy();
                _camera.StartShake(0.2f);
            }
        }

        private void CheckWinLoseConditions()
        {
            if (CropsLeft <= 0)
                IsGameWon = true;

            if (_entitySpawner.PlayerHealth <= 0)
                IsGameOver = true;
        }

        public void DisplayWinOrLoseMessage()
        {
        }

        // Updated save/load state methods
        public void SetPlayerPosition(Vector2 statePlayerPosition)
        {
            _entitySpawner.PlayerPosition = statePlayerPosition;
        }

        public void SetPlayerHealth(int statePlayerHealth)
        {
            _entitySpawner.PlayerHealth = statePlayerHealth;
        }

        public void SetEnemyPosition(int index, Vector2 position)
        {
            _entitySpawner.SetEnemyPosition(index, position);
        }

        public void SetEnemyStrategy(int index, string stateCurrentEnemyStrategy)
        {
            Strategy newStrategy = Strategy.Idle;
            switch (stateCurrentEnemyStrategy)
            {
                case nameof(Strategy.Random360FlyingMovement):
                    newStrategy = Strategy.Random360FlyingMovement;
                    break;
                case nameof(Strategy.Patrol):
                    newStrategy = Strategy.Patrol;
                    break;
                case nameof(Strategy.ChaseEnemy):
                    newStrategy = Strategy.ChaseEnemy;
                    break;
                case nameof(Strategy.Transition):
                    newStrategy = Strategy.Transition;
                    break;
            }
            _entitySpawner.SetEnemyStrategy(index, newStrategy);
        }

        // Convenience method to set all enemies to the same strategy
        public void SetAllEnemyStrategies(string stateCurrentEnemyStrategy)
        {
            Strategy newStrategy = Strategy.Idle;
            switch (stateCurrentEnemyStrategy)
            {
                case nameof(Strategy.Random360FlyingMovement):
                    newStrategy = Strategy.Random360FlyingMovement;
                    break;
                case nameof(Strategy.Patrol):
                    newStrategy = Strategy.Patrol;
                    break;
                case nameof(Strategy.ChaseEnemy):
                    newStrategy = Strategy.ChaseEnemy;
                    break;
                case nameof(Strategy.Transition):
                    newStrategy = Strategy.Transition;
                    break;
            }
            _entitySpawner.SetAllEnemyStrategies(newStrategy);
        }

        // Helper method to get number of enemies
        public int GetEnemyCount() => _entitySpawner.EnemyCount;

        // Existing methods remain unchanged
        public bool HandlePauseInput(InputState input, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            return _pauseAction.Occurred(input, controllingPlayer, out playerIndex);
        }

        public void Reset()
        {
            _entitySpawner.Reset();
            InitializeGameState();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _entitySpawner.Draw(gameTime, spriteBatch);
        }

        public Vector2 GetPlayerPosition() => _entitySpawner.PlayerPosition;
        public int GetPlayerHealth() => _entitySpawner.PlayerHealth;
        public int GetPlayerMaxHealth() => _entitySpawner.PlayerMaxHealth;
        public void ResumeMusic() => _audioManager.ResumeMusic();

        public void Unload()
        {
            _entitySpawner.Unload();
        }
    }
}
