using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Superorganism.AI;
using Superorganism.Collisions;
using Superorganism.Entities;
using Superorganism.Enums;
using Superorganism.Particle;
using System.Collections.Generic;
using System.Linq;
using System;

public class EntityManager
{
    private Ant _ant;
    private AntEnemy _antEnemy;
    private Crop[] _crops;
    private Fly[] _flies;
    private ExplosionParticleSystem _explosions;
    private readonly Game _game;

    private const double BLINK_INTERVAL = 0.05;
    private const double FLY_INVINCIBLE_DURATION = 1.5;
    private const double ENEMY_INVINCIBLE_DURATION = 2.0;
    private const float INVINCIBLE_ALPHA = 0.4f;
    private const int ENEMY_DAMAGE = 20;
    private const int FLY_DAMAGE = 10;

    private double _invincibleTimer;
    private bool _blinkState;

    public Vector2 PlayerPosition => _ant.Position;
    public int PlayerHealth => _ant.HitPoints;
    public int PlayerMaxHealth => _ant.MaxHitPoint;
    public int CropsCount => _crops.Length;
    public bool IsPlayerInvincible { get; private set; }
    public Vector2 EnemyPosition => _antEnemy.Position;
    public Strategy EnemyStrategy => _antEnemy.Strategy;
    public List<(Strategy Strategy, double StartTime, double LastActionTime)> EnemyStrategyHistory
        => _antEnemy.StrategyHistory;

    public EntityManager(Game game, ContentManager content, GraphicsDevice graphicsDevice)
    {
        _game = game;
        InitializeEntities(graphicsDevice);
        LoadContent(content);
    }

    private void InitializeEntities(GraphicsDevice graphicsDevice)
    {
        _ant = new Ant { Position = new Vector2(200, 200), IsControlled = true };
        _antEnemy = new AntEnemy { Position = new Vector2(500, 200) };
        InitializeCropsAndFlies(graphicsDevice);
        _explosions = new ExplosionParticleSystem(_game, 20);
        DecisionMaker.Entities.Add(_ant);
        DecisionMaker.Entities.Add(_antEnemy);
    }

    private void InitializeCropsAndFlies(GraphicsDevice graphicsDevice)
    {
        Random rand = new();

        _crops = new Crop[12];
        for (int i = 0; i < _crops.Length; i++)
        {
            _crops[i] = new Crop(new Vector2(
                (float)rand.NextDouble() * graphicsDevice.Viewport.Width, 383));
            DecisionMaker.Entities.Add(_crops[i]);
        }

        int numberOfFlies = rand.Next(15, 21);
        _flies = new Fly[numberOfFlies];
        for (int i = 0; i < numberOfFlies; i++)
        {
            _flies[i] = new Fly
            {
                Position = new Vector2(rand.Next(0, 800), rand.Next(0, 600)),
                Direction = (Direction)rand.Next(0, 4)
            };
            DecisionMaker.Entities.Add(_flies[i]);
        }
    }

    private void LoadContent(ContentManager content)
    {
        _ant.LoadContent(content, "ant-side_Rev2", 3, 1, new BoundingRectangle(), 0.25f);
        _ant.LoadSound(content);
        _antEnemy.LoadContent(content, "antEnemy-side_Rev3", 3, 1, new BoundingRectangle(), 0.3f);

        foreach (Crop crop in _crops)
        {
            crop.LoadContent(content, "crops", 8, 1, new BoundingCircle(), 1.0f);
        }

        foreach (Fly fly in _flies)
        {
            fly.LoadContent(content, "flies", 4, 4, new BoundingCircle(), 1.0f);
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

    private void UpdateInvincibility(double deltaTime)
    {
        _invincibleTimer -= deltaTime;

        if (_invincibleTimer <= 0)
        {
            EndInvincibility();
            return;
        }

        _blinkState = ((int)(_invincibleTimer / BLINK_INTERVAL) % 2) == 0;
        _ant.Color = _blinkState ? Color.White * INVINCIBLE_ALPHA : Color.White;
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

        _ant.HitPoints -= ENEMY_DAMAGE;
        StartInvincibility(ENEMY_INVINCIBLE_DURATION);
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
                _ant.HitPoints = Math.Max(0, _ant.HitPoints - FLY_DAMAGE);
                StartInvincibility(FLY_INVINCIBLE_DURATION);
                return true;
            }
        }
        return false;
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        foreach (Crop crop in _crops) crop.Draw(gameTime, spriteBatch);
        foreach (Fly fly in _flies) fly.Draw(gameTime, spriteBatch);
        _ant.Draw(gameTime, spriteBatch);
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
}