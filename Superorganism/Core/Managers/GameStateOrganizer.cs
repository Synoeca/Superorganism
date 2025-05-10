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
    /// <summary>
    /// Organize the main gameplay systems by coordinating entity behaviors, 
    /// handling collisions, tracking win/lose conditions, and synchronizing 
    /// game subsystems for a cohesive gameplay experience.
    /// </summary>
    public class GameStateOrganizer
    {
        private readonly EntityOraganizer _entityOraganizer;
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

        public GameStateOrganizer(Game game, ContentManager content, GraphicsDevice graphicsDevice,
            Camera2D camera, GameAudioManager audio, TiledMap map, GameStateInfo gameStateInfo)
        {
            DecisionMaker.Entities.Clear();
            _audioManager = audio;
            _camera = camera;
            _map = map;
            _content = content;

            _entityOraganizer = new EntityOraganizer(game, content, graphicsDevice, map, gameStateInfo);

            _pauseAction = new InputAction(
                [Buttons.Start, Buttons.Back],
                [Keys.Back, Keys.Escape],
                true);

            InitializeGameState();
        }

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
            CropsLeft = _entityOraganizer.CropsCount;
        }

        public Vector2[] GetEnemyPositions() => _entityOraganizer.GetEnemyPositions();

        public Strategy[] GetEnemyStrategies() => _entityOraganizer.GetEnemyStrategies();

        public ICollisionBounding[] GetEnemyBoundings() => _entityOraganizer.GetEnemyCollisionBoundings();

        public float GetEntityDistance(Entity entity1, Entity entity2) => Vector2.Distance(
            entity1.Position,
            entity2.Position
        );

        public float GetDistanceToPlayer(Entity entity) => Vector2.Distance(
            _entityOraganizer.PlayerPosition,
            entity.Position
        );

        public float GetEnemyDistanceToPlayer()
        {
            Vector2[] enemyPositions = GetEnemyPositions();
            float closestDistance = float.MaxValue;

            foreach (Vector2 enemyPos in enemyPositions)
            {
                float distance = Vector2.Distance(_entityOraganizer.PlayerPosition, enemyPos);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                }
            }

            return closestDistance;
        }

        public List<(Strategy Strategy, double StartTime, double LastActionTime)>[] GetEnemyStrategyHistories()
            => _entityOraganizer.GetEnemyStrategyHistories();

        public void Update(GameTime gameTime)
        {
            if (IsGameOver || IsGameWon) return;
            GameTime = gameTime;
            UpdateTimers(gameTime);
            _entityOraganizer.Update(gameTime);
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
            if (_entityOraganizer.CheckCropCollisions())
            {
                CropsLeft--;
                _audioManager.PlayCropPickup();
            }

            // Handle enemy collisions with timer
            if (_entityOraganizer.IsCollidingWithEnemy())
            {
                if (_enemyCollisionTimer <= 0)
                {
                    if (!_entityOraganizer.IsPlayerInvincible)
                    {
                        _entityOraganizer.ApplyEnemyDamage();
                        _audioManager.PlayFliesDestroy();
                        _enemyCollisionTimer = EnemyCollisionInterval;
                        _camera.StartShake(0.5f);
                    }
                }
                else if (!_entityOraganizer.IsPlayerInvincible)
                {
                    _entityOraganizer.ResetEntityColors();
                }
            }

            // Handle fly collisions
            if (_entityOraganizer.CheckFlyCollisions())
            {
                _audioManager.PlayFliesDestroy();
                _camera.StartShake(0.2f);
            }
        }

        private void CheckWinLoseConditions()
        {
            if (CropsLeft <= 0)
                IsGameWon = true;

            if (_entityOraganizer.PlayerHealth <= 0)
                IsGameOver = true;
        }

        public void DisplayWinOrLoseMessage()
        {
            // Implementation would go here
        }

        public void SetPlayerPosition(Vector2 statePlayerPosition)
        {
            _entityOraganizer.PlayerPosition = statePlayerPosition;
        }

        public void SetPlayerHealth(int statePlayerHealth)
        {
            _entityOraganizer.PlayerHealth = statePlayerHealth;
        }

        public void SetEnemyPosition(int index, Vector2 position)
        {
            _entityOraganizer.SetEnemyPosition(index, position);
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
            _entityOraganizer.SetEnemyStrategy(index, newStrategy);
        }

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
            _entityOraganizer.SetAllEnemyStrategies(newStrategy);
        }

        public int GetEnemyCount() => _entityOraganizer.EnemyCount;

        public bool HandlePauseInput(InputState input, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            return _pauseAction.Occurred(input, controllingPlayer, out playerIndex);
        }

        public void Reset()
        {
            _entityOraganizer.Reset();
            InitializeGameState();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _entityOraganizer.Draw(gameTime, spriteBatch);
        }

        public Vector2 GetPlayerPosition() => _entityOraganizer.PlayerPosition;
        public int GetPlayerHealth() => _entityOraganizer.PlayerHealth;
        public int GetPlayerMaxHealth() => _entityOraganizer.PlayerMaxHealth;
        public int GetPlayerStamina() => _entityOraganizer.PlayerStamina;
        public int GetPlayerMaxStamina() => _entityOraganizer.PlayerMaxStamina;
        public int GetPlayerHunger() => _entityOraganizer.PlayerHunger;
        public int GetPlayerMaxHunger() => _entityOraganizer.PlayerMaxHunger;
        public void ResumeMusic() => _audioManager.ResumeMusic();

        public void Unload()
        {
            _entityOraganizer.Unload();
        }
    }
}