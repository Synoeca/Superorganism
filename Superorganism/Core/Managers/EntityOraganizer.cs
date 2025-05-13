using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.AI;
using Superorganism.Collisions;
using Superorganism.Common;
using Superorganism.Core.InventorySystem;
using Superorganism.Entities;
using Superorganism.Enums;
using Superorganism.Particle;
using Superorganism.Tiles;

namespace Superorganism.Core.Managers;

/// <summary>
/// Manages the initialization, updating, and interactions of all entities in the game world,
/// including the player (ant), enemies, crops, and flies.
/// Handles player invincibility, collisions, and entity states.
/// </summary>
public class EntityOraganizer
{
    private Ant _ant;
    private readonly List<AntEnemy> _antEnemies = [];
    private readonly List<Crop> _crops = [];
    private readonly List<Fly> _flies = [];
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

    /// <summary>
    /// Gets or sets the position of the player ant.
    /// </summary>
    public Vector2 PlayerPosition
    {
        get => _ant.Position;
        set => _ant.Position = value;
    }

    /// <summary>
    /// Gets or sets the current health of the player ant.
    /// </summary>
    public float PlayerHealth
    {
        get => _ant.EntityStatus.HitPoints;
        set => _ant.EntityStatus.HitPoints = value;
    }

    /// <summary>
    /// Gets or sets the current stamina of the player ant.
    /// </summary>
    public float PlayerStamina
    {
        get => _ant.EntityStatus.Stamina;
        set => _ant.EntityStatus.Stamina = value;
    }

    /// <summary>
    /// Gets or sets the current hunger level of the player ant.
    /// </summary>
    public float PlayerHunger
    {
        get => _ant.EntityStatus.Hunger;
        set => _ant.EntityStatus.Hunger = value;
    }

    /// <summary>
    /// Gets or sets the status object representing player's health, stamina, and hunger.
    /// </summary>
    public EntityStatus PlayerEntityStatus
    {
        get => _ant.EntityStatus;
        set => _ant.EntityStatus = value;
    }

    /// <summary>
    /// Gets the maximum health of the player.
    /// </summary>
    public float PlayerMaxHealth => _ant.EntityStatus.MaxHitPoints;

    /// <summary>
    /// Gets the maximum stamina of the player.
    /// </summary>
    public float PlayerMaxStamina => _ant.EntityStatus.MaxStamina;

    /// <summary>
    /// Gets the maximum hunger of the player.
    /// </summary>
    public float PlayerMaxHunger => _ant.EntityStatus.MaxHunger;

    /// <summary>
    /// Gets the number of crops currently in the game world.
    /// </summary>
    public int CropsCount => _crops.Count;

    /// <summary>
    /// Indicates whether the player is currently invincible (immune to damage).
    /// </summary>
    public bool IsPlayerInvincible { get; private set; }

    /// <summary>
    /// Retrieves the positions of all enemy ants in the game world.
    /// </summary>
    /// <returns>An array of <see cref="Vector2"/> representing the position of each enemy ant.</returns>
    public Vector2[] GetEnemyPositions()
    {
        return _antEnemies.Select(enemy => enemy.Position).ToArray();
    }

    /// <summary>
    /// Sets the position of a specific enemy ant by index.
    /// </summary>
    /// <param name="index">Index of the enemy in the list.</param>
    /// <param name="position">New position to assign.</param>
    public void SetEnemyPosition(int index, Vector2 position)
    {
        if (index >= 0 && index < _antEnemies.Count)
        {
            _antEnemies[index].Position = position;
        }
    }

    /// <summary>
    /// Retrieves the current strategy assigned to each enemy ant.
    /// </summary>
    /// <returns>An array of <see cref="Strategy"/> objects representing each enemy's behavior strategy.</returns>
    public Strategy[] GetEnemyStrategies()
    {
        return _antEnemies.Select(enemy => enemy.Strategy).ToArray();
    }

    /// <summary>
    /// Sets the strategy of a specific enemy ant.
    /// </summary>
    /// <param name="index">Index of the enemy in the list.</param>
    /// <param name="strategy">New strategy to assign.</param>
    public void SetEnemyStrategy(int index, Strategy strategy)
    {
        if (index >= 0 && index < _antEnemies.Count)
        {
            _antEnemies[index].Strategy = strategy;
        }
    }

    /// <summary>
    /// Assigns the same strategy to all enemy ants.
    /// </summary>
    /// <param name="strategy">Strategy to apply to all enemies.</param>
    public void SetAllEnemyStrategies(Strategy strategy)
    {
        foreach (AntEnemy enemy in _antEnemies)
        {
            enemy.Strategy = strategy;
        }
    }

    /// <summary>
    /// Retrieves the collision boundaries for all enemy ants.
    /// </summary>
    /// <returns>An array of <see cref="ICollisionBounding"/> representing each enemy's collision bounds.</returns>
    public ICollisionBounding[] GetEnemyCollisionBoundings()
    {
        return _antEnemies.Select(enemy => enemy.CollisionBounding).ToArray();
    }

    /// <summary>
    /// Gets the strategic history for each enemy ant.
    /// </summary>
    /// <returns>A list of strategy history entries for each enemy.</returns>
    public List<(Strategy Strategy, double StartTime, double LastActionTime)>[] GetEnemyStrategyHistories()
    {
        return _antEnemies.Select(enemy => enemy.StrategyHistory).ToArray();
    }

    /// <summary>
    /// Gets the number of enemy ants currently in the game world.
    /// </summary>
    public int EnemyCount => _antEnemies.Count;

    /// <summary>
    /// Gets or sets the player-controlled ant.
    /// </summary>
    public Ant PlayerAnt
    {
        get => _ant;
        set => _ant = value;

    }

    /// <summary>
    /// Retrieves a specific enemy ant by index.
    /// </summary>
    /// <param name="index">Index of the enemy.</param>
    /// <returns>AntEnemy instance if found, otherwise null.</returns>
    public AntEnemy GetEnemy(int index)
    {
        if (index >= 0 && index < _antEnemies.Count)
        {
            return _antEnemies[index];
        }
        return null;
    }

    /// <summary>
    /// Constructs the entity organizer and sets up the game world entities,
    /// loading from a saved state if available or initializing default entities.
    /// </summary>
    /// <param name="game">Reference to the game.</param>
    /// <param name="content">Content manager for loading assets.</param>
    /// <param name="graphicsDevice">Graphics device for rendering purposes.</param>
    /// <param name="map">Tiled map used for tile-based positioning.</param>
    /// <param name="gameStateInfo">State info containing previous entity states.</param>
    public EntityOraganizer(Game game, ContentManager content,
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
    }

    /// <summary>
    /// Initializes all default entities including player ant, enemies, crops, and flies.
    /// </summary>
    /// <param name="graphicsDevice">Graphics device used by the particle system.</param>
    private void InitializeEntities(GraphicsDevice graphicsDevice)
    {
        _ant = new Ant();
        _ant.InitializeAtTile(114, 10);
        _ant.IsControlled = true;
        _ant.Inventory =
        [
            InventoryItem.CreateFromTileset("T1", 3, "Test 1", _map.Tilesets, 1, 49),
            InventoryItem.CreateFromTileset("T2", 3, "Test 2", _map.Tilesets, 1, 50),
            InventoryItem.CreateFromTileset("T3", 3, "Test 3", _map.Tilesets, 1, 51),
            InventoryItem.CreateFromTileset("T4", 3, "Test 4", _map.Tilesets, 1, 52)
        ];
        _ant.Inventory.Add(InventoryItem.CreateFromTileset("T4", 3, "Test 4", _map.Tilesets, 1, 52));

        // Initialize multiple ant enemies
        const int count = 1;
        Random rand = new();

        for (int i = 0; i < count; i++)
        {
            int enemyX = 10 + rand.Next(150); // Spread between tile 60-100
            int enemyY = 10 + rand.Next(8);   // Spread between tile 5-12
            AntEnemy antEnemy = new();
            //antEnemy.InitializeAtTile(enemyX, enemyY);
            antEnemy.InitializeAtTile(115, 14);
            _antEnemies.Add(antEnemy);
        }

        InitializeCrops();
        InitializeFlies(graphicsDevice);
        _explosions = new ExplosionParticleSystem(_game, 20);

        // Add entities to DecisionMaker
        DecisionMaker.Entities.Add(_ant);
        foreach (AntEnemy enemy in _antEnemies)
        {
            DecisionMaker.Entities.Add(enemy);
        }
    }

    /// <summary>
    /// Initializes crops in the game world at random tile locations.
    /// </summary>
    private void InitializeCrops()
    {
        Random rand = new();
        int count = 1;
        for (int i = 0; i < count; i++)
        {
            // Spread crops across different heights
            Crop crop = new();
            int cropX = 10 + (2 * i); // Spread them out horizontally
            int cropY = 5 + rand.Next(10); // Random height between tile 5-14
            Vector2 position = TilePhysicsInspector.TileToWorld(cropX, cropY);

            // Add small random offset within tile
            position.X += rand.Next(-16, 16);
            position.Y += rand.Next(-16, 16);
            crop.Position = position;
            _crops.Add(crop);
            DecisionMaker.Entities.Add(crop);
        }
    }

    /// <summary>
    /// Initializes fly entities at randomized locations in the game world.
    /// </summary>
    /// <param name="graphicsDevice">Graphics device (currently unused in this method).</param>
    private void InitializeFlies(GraphicsDevice graphicsDevice)
    {
        Random rand = new();
        int count = 1;
        for (int i = 0; i < count; i++)
        {
            Fly fly = new();
            // Spread flies across a wider range and higher up
            int spreadX = 40 + rand.Next(80);  // Spread between tile 40-120
            int spreadY = 2 + rand.Next(10);   // Higher up between tile 2-11
            Vector2 position = TilePhysicsInspector.TileToWorld(spreadX, spreadY);

            // Add random offsets within tile for more natural distribution
            position.X += rand.Next(-32, 32);
            position.Y += rand.Next(-32, 32);
            fly.Position = position;
            fly.Direction = (Direction)(rand.Next(4));
            _flies.Add(fly);
            DecisionMaker.Entities.Add(fly);
        }
    }

    /// <summary>
    /// Loads visual and audio assets for all entities.
    /// </summary>
    /// <param name="content">Content manager for asset loading.</param>
    private void LoadContent(ContentManager content)
    {
        _ant.LoadContent(content, "ant-side_Rev2", 3, 1,
            new BoundingRectangle(), 0.23f);
        _ant.LoadSound(content);

        // Load content for all ant enemies
        foreach (AntEnemy enemy in _antEnemies)
        {
            enemy.LoadContent(content, "antEnemy-side_Rev3", 3, 1,
                new BoundingRectangle(), 0.23f);
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

    /// <summary>
    /// Updates all entities and handles the player's invincibility timer.
    /// </summary>
    /// <param name="gameTime">Game timing snapshot.</param>
    public void Update(GameTime gameTime)
    {
        if (IsPlayerInvincible)
        {
            UpdateInvincibility(gameTime.ElapsedGameTime.TotalSeconds);
        }

        UpdateEntities(gameTime);
    }

    /// <summary>
    /// Updates the player's invincibility duration and visual blinking effect.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update.</param>
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

    /// <summary>
    /// Updates each entity's behavior and animation.
    /// </summary>
    /// <param name="gameTime">Game timing snapshot.</param>
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

        foreach (Entity entity in DecisionMaker.Entities)
        {
            if (entity is DroppedItem di)
            {
                di.Update(gameTime);
            }
        }
    }

    /// <summary>
    /// Ends the invincibility state for the player.
    /// </summary>
    private void EndInvincibility()
    {
        IsPlayerInvincible = false;
        _ant.Color = Color.White;
    }

    /// <summary>
    /// Begins a temporary invincibility period for the player.
    /// </summary>
    /// <param name="duration">Duration of invincibility in seconds.</param>
    private void StartInvincibility(double duration)
    {
        IsPlayerInvincible = true;
        _invincibleTimer = duration;
        _blinkState = true;
    }

    /// <summary>
    /// Checks for collisions between the player and any uncollected crops.
    /// Marks the crop as collected on collision.
    /// </summary>
    /// <returns>True if a collision occurred, otherwise false.</returns>
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

    /// <summary>
    /// Checks if the player is currently colliding with any enemy ant.
    /// </summary>
    /// <returns>True if a collision occurred and the player is not invincible.</returns>
    public bool IsCollidingWithEnemy()
    {
        if (IsPlayerInvincible) return false;

        return _antEnemies.Any(enemy => enemy.CollisionBounding.CollidesWith(_ant.CollisionBounding));
    }

    /// <summary>
    /// Applies damage to the player due to enemy contact and triggers invincibility.
    /// </summary>
    public void ApplyEnemyDamage()
    {
        if (IsPlayerInvincible) return;

        _ant.EntityStatus.HitPoints -= EnemyDamage;

        // Add flash effect
        _ant.Color = Color.Red;  // Will be modified by invincibility immediately after
        StartInvincibility(EnemyInvincibleDuration);
    }

    /// <summary>
    /// Checks for collisions between the player and any non-destroyed flies.
    /// Destroys the fly and applies damage on collision.
    /// </summary>
    /// <returns>True if a collision occurred, otherwise false.</returns>
    public bool CheckFlyCollisions()
    {
        if (IsPlayerInvincible) return false;

        foreach (Fly fly in _flies.Where(f => !f.Destroyed))
        {
            if (fly.CollisionBounding.CollidesWith(_ant.CollisionBounding))
            {
                fly.Destroyed = true;
                //_explosions.PlaceExplosion(fly.Position);
                _ant.EntityStatus.HitPoints = Math.Max(0, _ant.EntityStatus.HitPoints - FlyDamage);

                // Add flash effect
                _ant.Color = Color.Red * 0.8f;  // Will be modified by invincibility immediately after
                StartInvincibility(FlyInvincibleDuration);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Draws all entities in the proper rendering order, with visual effects for invincibility.
    /// </summary>
    /// <param name="gameTime">Game timing snapshot.</param>
    /// <param name="spriteBatch">Sprite batch used for drawing.</param>
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

        foreach (Entity entity in DecisionMaker.Entities)
        {
            if (entity is DroppedItem di)
            {
                di.Color = Color.White;
                di.Draw(gameTime, spriteBatch);
            }
        }
    }

    /// <summary>
    /// Resets visual colors of all entities to white (default).
    /// Useful after flashing effects or damage states.
    /// </summary>
    public void ResetEntityColors()
    {
        _ant.Color = Color.White;
        foreach (AntEnemy enemy in _antEnemies)
        {
            enemy.Color = Color.White;
        }
    }

    /// <summary>
    /// Resets the player and enemies to new positions.
    /// Clears decision-making entity list for fresh setup.
    /// </summary>
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

    /// <summary>
    /// Unloads systems such as the explosion particle system from the game components.
    /// </summary>
    public void Unload()
    {
        _game.Components.Remove(_explosions);
    }
}