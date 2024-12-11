using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.AI;
using Superorganism.Collisions;
using Superorganism.Core.SaveLoadSystem;
using Superorganism.Entities;
using Superorganism.Enums;
using Superorganism.Particle;
using Superorganism.Tiles;

namespace Superorganism.Core.Managers;

public class EntityManager
{
    private Ant _ant;
    private AntEnemy _antEnemy;
    private Crop[] _crops;
    private Fly[] _flies;
    private ExplosionParticleSystem _explosions;
    private readonly Game _game;
    private readonly Map _map;

    private const float InvincibleAlpha = 0.4f;
    private const float EnemyAlpha = 0.8f;
    private const float FlyAlpha = 0.9f;
    private const double BlinkInterval = 0.05;
    private const double FlyInvincibleDuration = 1.5;
    private const double EnemyInvincibleDuration = 2.0;
    private const int EnemyDamage = 20;
    private const int FlyDamage = 10;

    private double _invincibleTimer;
    private bool _blinkState;

    public Vector2 PlayerPosition => _ant.Position;
    public int PlayerHealth => _ant.HitPoints;
    public int PlayerMaxHealth => _ant.MaxHitPoint;
    public int CropsCount => _crops.Length;
    public bool IsPlayerInvincible { get; private set; }
    public Vector2 EnemyPosition => _antEnemy.Position;
    public Strategy EnemyStrategy => _antEnemy.Strategy;
    public ICollisionBounding EnemyCollisionBounding => _antEnemy.CollisionBounding;
    public List<(Strategy Strategy, double StartTime, double LastActionTime)> EnemyStrategyHistory
        => _antEnemy.StrategyHistory;
    public Map GetCurrentMap() => _map;

    public EntityManager(Game game, ContentManager content, 
        GraphicsDevice graphicsDevice, Map map)
    {
        _game = game;
        _map = map;
        InitializeEntities(graphicsDevice);
        LoadContent(content);
        //_ant.CollisionBounding = (BoundingRectangle)_ant.TextureInfo.CollisionType;
    }

    private void InitializeEntities(GraphicsDevice graphicsDevice)
    {
        _ant = new Ant();
        _ant.InitializeAtTile(72, 19);
        //_ant.CollisionBounding.Center = _ant.Position;
        _ant.IsControlled = true;
        

        _antEnemy = new AntEnemy();
        _antEnemy.InitializeAtTile(81, 18);

        InitializeCropsAndFlies(graphicsDevice);
        _explosions = new ExplosionParticleSystem(_game, 20);
        DecisionMaker.Entities.Add(_ant);
        DecisionMaker.Entities.Add(_antEnemy);
    }


    private void InitializeCropsAndFlies(GraphicsDevice graphicsDevice)
    {
        _crops = new Crop[30];
        for (int i = 0; i < _crops.Length; i++)
        {
            _crops[i] = new Crop();
            Vector2 position = MapHelper.TileToWorld(10 + 8*i, 19);
            _crops[i].Position = position;  // Set position after creation
            DecisionMaker.Entities.Add(_crops[i]);
        }

        _flies = new Fly[100];
        Random rand = new();
        for (int i = 0; i < _flies.Length; i++)
        {
            _flies[i] = new Fly();

            // Spread flies across a wider X range (50 to 100 tiles)
            int spreadX = 50 + rand.Next(50);
            // Vary Y position between tiles 10 and 20
            int spreadY = 5 + rand.Next(9);

            Vector2 position = MapHelper.TileToWorld(spreadX, spreadY);

            // Add small random offsets within the tile for more natural positioning
            position.X += rand.Next(-32, 32); // Half tile random offset
            position.Y += rand.Next(-32, 32);

            _flies[i].Position = position;
            _flies[i].Direction = (Direction)(rand.Next(4)); // Random initial direction
            DecisionMaker.Entities.Add(_flies[i]);
        }
    }

    private void LoadContent(ContentManager content)
    {
        _ant.LoadContent(content, "ant-side_Rev2", 3, 1, 
            new BoundingRectangle(), 0.25f);
        _ant.LoadSound(content);

        _antEnemy.LoadContent(content, "antEnemy-side_Rev3", 3, 1, 
            new BoundingRectangle(), 0.3f);

        foreach (Crop crop in _crops)
        {
            crop.LoadContent(content, "crops", 8, 1, 
                new BoundingCircle(), 1.0f);
        }

        foreach (Fly fly in _flies)
        {
            fly.LoadContent(content, "flies", 4, 4, 
                new BoundingCircle(), 1.0f);
        }
    }

    public void Update(GameTime gameTime)
    {
        if (IsPlayerInvincible)
        {
            UpdateInvincibility(gameTime.ElapsedGameTime.TotalSeconds);
        }

        UpdateEntities(gameTime);
    }

    // Inside UpdateInvincibility method:
    private void UpdateInvincibility(double deltaTime)
    {
        _invincibleTimer -= deltaTime;

        if (_invincibleTimer <= 0)
        {
            EndInvincibility();
            return;
        }

        // Smoother alpha transition during blinking
        float blinkProgress = (float)(_invincibleTimer % BlinkInterval / BlinkInterval);
        _blinkState = ((int)(_invincibleTimer / BlinkInterval) % 2) == 0;
        float alpha = _blinkState ?
            MathHelper.Lerp(InvincibleAlpha, 1f, blinkProgress) :
            MathHelper.Lerp(1f, InvincibleAlpha, blinkProgress);

        _ant.Color = Color.White * alpha;
    }

    private void UpdateEntities(GameTime gameTime)
    {
        _ant.Update(gameTime);
        _antEnemy.Update(gameTime);

        foreach (Crop crop in _crops.Where(c => !c.Collected))
        {
            crop.Update(gameTime);
        }

        foreach (Fly fly in _flies.Where(f => !f.Destroyed))
        {
            fly.Update(gameTime);
        }
    }

    private void EndInvincibility()
    {
        IsPlayerInvincible = false;
        _ant.Color = Color.White;
    }

    private void StartInvincibility(double duration)
    {
        IsPlayerInvincible = true;
        _invincibleTimer = duration;
        _blinkState = true;
    }

    public bool CheckCropCollisions()
    {
        foreach (Crop crop in _crops.Where(c => !c.Collected))
        {
            if (crop.CollisionBounding.CollidesWith(_ant.CollisionBounding))
            {
                crop.Collected = true;
                return true;
            }
        }
        return false;
    }

    public bool IsCollidingWithEnemy()
        => !IsPlayerInvincible && _antEnemy.CollisionBounding.CollidesWith(_ant.CollisionBounding);

    public void ApplyEnemyDamage()
    {
        if (IsPlayerInvincible) return;

        _ant.HitPoints -= EnemyDamage;

        // Add flash effect
        _ant.Color = Color.Red;  // Will be modified by invincibility immediately after
        StartInvincibility(EnemyInvincibleDuration);
    }

    public bool CheckFlyCollisions()
    {
        if (IsPlayerInvincible) return false;

        foreach (Fly fly in _flies.Where(f => !f.Destroyed))
        {
            if (fly.CollisionBounding.CollidesWith(_ant.CollisionBounding))
            {
                fly.Destroyed = true;
                _explosions.PlaceExplosion(fly.Position);
                _ant.HitPoints = Math.Max(0, _ant.HitPoints - FlyDamage);

                // Add flash effect
                _ant.Color = Color.Red * 0.8f;  // Will be modified by invincibility immediately after
                StartInvincibility(FlyInvincibleDuration);
                return true;
            }
        }
        return false;
    }

    // Modify Draw method to handle layering
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Draw crops first (they're on the ground)
        foreach (Crop crop in _crops)
        {
            crop.Draw(gameTime, spriteBatch);
        }

        // Draw flies with slight transparency
        foreach (Fly fly in _flies.Where(f => !f.Destroyed))
        {
            fly.Color = Color.White * FlyAlpha;
            fly.Draw(gameTime, spriteBatch);
        }

        // Draw the player ant
        _ant.Draw(gameTime, spriteBatch);

        // Draw enemy ant
        _antEnemy.Draw(gameTime, spriteBatch);
    }

    public void ResetEntityColors()
    {
        _ant.Color = Color.White;
        _antEnemy.Color = Color.White;
    }

    public void Reset()
    {
        _ant.Position = new Vector2(200, 200);
        _antEnemy.Position = new Vector2(500, 200);
        DecisionMaker.Entities.Clear();
    }

    public void Unload()
    {
        _game.Components.Remove(_explosions);
    }

    public void SaveGameState(string saveName)
    {
        GameSaveData saveData = new()
        {
            PlayerPosition = _ant.Position,
            PlayerHealth = _ant.HitPoints,
            EnemyPosition = _antEnemy.Position,
            CurrentStrategy = _antEnemy.Strategy.ToString(),
            CurrentMapName = _map.Properties.GetValueOrDefault("name", "unknown"),

            // Save all crops
            Crops = _crops.Select(c => new CropData
            {
                Position = c.Position,
                Collected = c.Collected
            }).ToList(),

            // Save all flies
            Flies = _flies.Select(f => new FlyData
            {
                Position = f.Position,
                Destroyed = f.Destroyed,
                Direction = f.Direction
            }).ToList()
        };

        SaveSystem.SaveGame(saveData, saveName);
    }

    public void LoadGameState(string saveName)
    {
        GameSaveData saveData = SaveSystem.LoadGame(saveName);
        if (saveData == null) return;

        // Restore player state
        _ant.Position = saveData.PlayerPosition;
        _ant.HitPoints = saveData.PlayerHealth;

        // Restore enemy state
        _antEnemy.Position = saveData.EnemyPosition;
        if (Enum.TryParse<Strategy>(saveData.CurrentStrategy, out Strategy strategy))
        {
            _antEnemy.Strategy = strategy;
        }

        // Restore crops
        for (int i = 0; i < Math.Min(_crops.Length, saveData.Crops.Count); i++)
        {
            _crops[i].Position = saveData.Crops[i].Position;
            _crops[i].Collected = saveData.Crops[i].Collected;
        }

        // Restore flies
        for (int i = 0; i < Math.Min(_flies.Length, saveData.Flies.Count); i++)
        {
            _flies[i].Position = saveData.Flies[i].Position;
            _flies[i].Destroyed = saveData.Flies[i].Destroyed;
            _flies[i].Direction = saveData.Flies[i].Direction;
        }

        // Reset entity states
        ResetEntityColors();
        IsPlayerInvincible = false;
    }
}