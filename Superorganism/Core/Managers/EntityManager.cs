using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.AI;
using Superorganism.Collisions;
using Superorganism.Entities;
using Superorganism.Enums;
using Superorganism.Particle;
using Superorganism.Tiles;

namespace Superorganism.Core.Managers;

public class EntityManager
{
    private Ant _ant;
    private List<AntEnemy> _antEnemies = [];
    private List<Crop> _crops = [];
    private List<Fly> _flies = [];
    private ExplosionParticleSystem _explosions;
    private readonly Game _game;
    private readonly TiledMap _map;

    private const float InvincibleAlpha = 0.4f;
    private const float EnemyAlpha = 0.8f;
    private const float FlyAlpha = 0.9f;
    private const double BlinkInterval = 0.05;
    private const double FlyInvincibleDuration = 1.5;
    private const double EnemyInvincibleDuration = 2.0;
    private const int EnemyDamage = 10;
    private const int FlyDamage = 3;

    private double _invincibleTimer;
    private bool _blinkState;

    public Vector2 PlayerPosition
    {
        get => _ant.Position;
        set => _ant.Position = value;
    }

    public int PlayerHealth
    {
        get => _ant.HitPoints;
        set => _ant.HitPoints = value;
    }

    public int PlayerMaxHealth => _ant.MaxHitPoint;
    public int CropsCount => _crops.Count;
    public bool IsPlayerInvincible { get; private set; }
    public Vector2[] GetEnemyPositions()
    {
        return _antEnemies.Select(enemy => enemy.Position).ToArray();
    }

    public void SetEnemyPosition(int index, Vector2 position)
    {
        if (index >= 0 && index < _antEnemies.Count)
        {
            _antEnemies[index].Position = position;
        }
    }

    public Strategy[] GetEnemyStrategies()
    {
        return _antEnemies.Select(enemy => enemy.Strategy).ToArray();
    }

    public void SetEnemyStrategy(int index, Strategy strategy)
    {
        if (index >= 0 && index < _antEnemies.Count)
        {
            _antEnemies[index].Strategy = strategy;
        }
    }

    public void SetAllEnemyStrategies(Strategy strategy)
    {
        foreach (AntEnemy enemy in _antEnemies)
        {
            enemy.Strategy = strategy;
        }
    }

    public ICollisionBounding[] GetEnemyCollisionBoundings()
    {
        return _antEnemies.Select(enemy => enemy.CollisionBounding).ToArray();
    }

    public List<(Strategy Strategy, double StartTime, double LastActionTime)>[] GetEnemyStrategyHistories()
    {
        return _antEnemies.Select(enemy => enemy.StrategyHistory).ToArray();
    }

    // Add helper method to get number of enemies
    public int EnemyCount => _antEnemies.Count;

    // Add method to get specific enemy
    public AntEnemy GetEnemy(int index)
    {
        if (index >= 0 && index < _antEnemies.Count)
        {
            return _antEnemies[index];
        }
        return null;
    }
    public TiledMap GetCurrentMap() => _map;

    public EntityManager(Game game, ContentManager content,
        GraphicsDevice graphicsDevice, TiledMap map, GameStateInfo gameStateInfo)
    {
        _game = game;
        _map = map;
        if (gameStateInfo.Entities == null)
        {
            InitializeEntities(graphicsDevice);
        }
        else
        {
            DecisionMaker.Entities = gameStateInfo.Entities;
            foreach (Entity entity in gameStateInfo.Entities)
            {
                switch (entity)
                {
                    case Ant ant:
                        _ant = ant;
                        break;
                    case AntEnemy antEnemy:
                        _antEnemies!.Add(antEnemy);
                        break;
                    case Fly fly:
                        _flies!.Add(fly);
                        break;
                    case Crop crop:
                        _crops!.Add(crop);
                        break;
                }
            }
        }


        LoadContent(content);
        //_ant.CollisionBounding = (BoundingRectangle)_ant.TextureInfo.CollisionType;
    }

    private void InitializeEntities(GraphicsDevice graphicsDevice)
    {
        _ant = new Ant();
        _ant.InitializeAtTile(72, 10);
        _ant.IsControlled = true;

        // Initialize multiple ant enemies
        int count = 15;
        Random rand = new();
        //for (int i = 0; i < count; i++)
        //{
        //    _antEnemies[i] = new AntEnemy();
        //    // Spread enemies across different X positions and higher Y positions
        //    int enemyX = 2 + rand.Next(50); // Spread between tile 60-100
        //    int enemyY = 5 + rand.Next(8);   // Spread between tile 5-12
        //    _antEnemies[i].InitializeAtTile(enemyX, enemyY);
        //}

        for (int i = 0; i < count; i++)
        {
            int enemyX = 2 + rand.Next(50); // Spread between tile 60-100
            int enemyY = 5 + rand.Next(8);   // Spread between tile 5-12
            AntEnemy antEnemy = new();
            antEnemy.InitializeAtTile(enemyX, enemyY);
            _antEnemies.Add(antEnemy);
            //_antEnemies[i].InitializeAtTile(enemyX, enemyY);
        }

        InitializeCrops(graphicsDevice);
        InitializeFlies(graphicsDevice);
        _explosions = new ExplosionParticleSystem(_game, 20);

        // Add entities to DecisionMaker
        DecisionMaker.Entities.Add(_ant);
        foreach (AntEnemy enemy in _antEnemies)
        {
            DecisionMaker.Entities.Add(enemy);
        }
    }


    private void InitializeCrops(GraphicsDevice graphicsDevice)
    {
        Random rand = new();
        int count = 2;
        for (int i = 0; i < count; i++)
        {
            //_crops[i] = new Crop();
            // Spread crops across different heights
            Crop crop = new();
            int cropX = 10 + (6 * i); // Spread them out horizontally
            int cropY = 5 + rand.Next(10); // Random height between tile 5-14
            Vector2 position = MapHelper.TileToWorld(cropX, cropY);
            // Add small random offset within tile
            position.X += rand.Next(-16, 16);
            position.Y += rand.Next(-16, 16);
            crop.Position = position;
            _crops.Add(crop);
            //_crops[i].Position = position;
            DecisionMaker.Entities.Add(crop);
        }
    }

    private void InitializeFlies(GraphicsDevice graphicsDevice)
    {
        Random rand = new();
        int count = 2;
        for (int i = 0; i < count; i++)
        {
            //_flies[i] = new Fly();
            Fly fly = new();
            // Spread flies across a wider range and higher up
            int spreadX = 40 + rand.Next(80);  // Spread between tile 40-120
            int spreadY = 2 + rand.Next(10);   // Higher up between tile 2-11
            Vector2 position = MapHelper.TileToWorld(spreadX, spreadY);
            // Add random offsets within tile for more natural distribution
            position.X += rand.Next(-32, 32);
            position.Y += rand.Next(-32, 32);
            fly.Position = position;
            fly.Direction = (Direction)(rand.Next(4));
            //_flies[i].Position = position;
            //_flies[i].Direction = (Direction)(rand.Next(4));
            _flies.Add(fly);
            DecisionMaker.Entities.Add(fly);
        }
    }

    private void LoadContent(ContentManager content)
    {
        _ant.LoadContent(content, "ant-side_Rev2", 3, 1,
            new BoundingRectangle(), 0.23f);
        _ant.LoadSound(content);

        // Load content for all ant enemies
        foreach (AntEnemy enemy in _antEnemies)
        {
            enemy.LoadContent(content, "antEnemy-side_Rev3", 3, 1,
                new BoundingRectangle(), 0.3f);
        }

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

        foreach (AntEnemy antEnemy in _antEnemies)
        {
            antEnemy.Update(gameTime);
        }

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
    {
        if (IsPlayerInvincible) return false;

        return _antEnemies.Any(enemy => enemy.CollisionBounding.CollidesWith(_ant.CollisionBounding));
    }

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
                //_explosions.PlaceExplosion(fly.Position);
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

        // Draw all enemy ants
        foreach (AntEnemy enemy in _antEnemies)
        {
            enemy.Color = Color.White;
            enemy.Draw(gameTime, spriteBatch);
        }
    }

    public void ResetEntityColors()
    {
        _ant.Color = Color.White;
        foreach (AntEnemy enemy in _antEnemies)
        {
            enemy.Color = Color.White;
        }
    }

    public void Reset()
    {
        _ant.Position = new Vector2(200, 200);

        // Reset all enemies to new positions
        Random rand = new();
        foreach (AntEnemy enemy in _antEnemies)
        {
            enemy.Position = new Vector2(
                300 + rand.Next(400),
                100 + rand.Next(100)
            );
        }

        DecisionMaker.Entities.Clear();
    }

    public void Unload()
    {
        _game.Components.Remove(_explosions);
    }
}