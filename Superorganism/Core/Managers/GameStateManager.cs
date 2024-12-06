﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Superorganism.AI;
using Superorganism.ScreenManagement;

namespace Superorganism.Core.Managers
{
    public class GameStateManager
    {
        private readonly EntityManager _entityManager;
        private readonly GameAudioManager _audioManager;
        private readonly InputAction _pauseAction;

        public bool IsGameOver { get; private set; }
        public bool IsGameWon { get; private set; }
        public int CropsLeft { get; private set; }
        public double ElapsedTime { get; private set; }

        private double _enemyCollisionTimer;
        private const double EnemyCollisionInterval = 0.2;

        public GameStateManager(Game game, ContentManager content, GraphicsDevice graphicsDevice)
        {
            DecisionMaker.Entities.Clear();
            _entityManager = new EntityManager(game, content, graphicsDevice);
            _audioManager = new GameAudioManager(content);

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
            CropsLeft = _entityManager.CropsCount;
        }

        public Vector2 GetEnemyPosition() => _entityManager.EnemyPosition;

        public Strategy GetEnemyStrategy() => _entityManager.EnemyStrategy;

        public float GetDistanceToPlayer() => Vector2.Distance(
            _entityManager.PlayerPosition,
            _entityManager.EnemyPosition
        );

        public List<(Strategy Strategy, double StartTime, double LastActionTime)> GetEnemyStrategyHistory()
            => _entityManager.EnemyStrategyHistory;

        public void Update(GameTime gameTime)
        {
            if (IsGameOver || IsGameWon) return;

            UpdateTimers(gameTime);
            _entityManager.Update(gameTime);
            CheckCollisions();
            CheckWinLoseConditions();
        }

        private void UpdateTimers(GameTime gameTime)
        {
            ElapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

            if (_enemyCollisionTimer > 0)
            {
                _enemyCollisionTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                System.Diagnostics.Debug.WriteLine($"Enemy collision timer updated: {_enemyCollisionTimer}");
            }
        }

        private void CheckCollisions()
        {
            // Handle crop collisions
            if (_entityManager.CheckCropCollisions())
            {
                CropsLeft--;
                _audioManager.PlayCropPickup();
            }

            // Handle enemy collisions with timer
            if (_entityManager.IsCollidingWithEnemy())
            {
                if (_enemyCollisionTimer <= 0)
                {
                    _entityManager.ApplyEnemyDamage();
                    _audioManager.PlayFliesDestroy();
                    _enemyCollisionTimer = EnemyCollisionInterval;
                    System.Diagnostics.Debug.WriteLine($"Enemy collision: Applied damage, Timer reset to {EnemyCollisionInterval}");
                }
                else
                {
                    // Still colliding but timer active, ensure visual feedback
                    _entityManager.ResetEntityColors();
                    System.Diagnostics.Debug.WriteLine($"Enemy collision: Timer active {_enemyCollisionTimer}, no damage");
                }
            }

            // Handle fly collisions
            if (_entityManager.CheckFlyCollisions())
            {
                _audioManager.PlayFliesDestroy();
            }
        }

        private void CheckWinLoseConditions()
        {
            if (CropsLeft <= 0)
                IsGameWon = true;

            if (_entityManager.PlayerHealth <= 0)
                IsGameOver = true;
        }

        public bool HandlePauseInput(InputState input, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            return _pauseAction.Occurred(input, controllingPlayer, out playerIndex);
        }

        public void Reset()
        {
            _entityManager.Reset();
            InitializeGameState();
        }

        public void PauseAudio() => _audioManager.PauseMusic();
        public void ResumeAudio() => _audioManager.ResumeMusic();
        public void StopAudio() => _audioManager.StopMusic();

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _entityManager.Draw(gameTime, spriteBatch);
        }

        public Vector2 GetPlayerPosition() => _entityManager.PlayerPosition;
        public int GetPlayerHealth() => _entityManager.PlayerHealth;
        public int GetPlayerMaxHealth() => _entityManager.PlayerMaxHealth;

        public void Unload()
        {
            _entityManager.Unload();
            StopAudio();
        }
    }
}