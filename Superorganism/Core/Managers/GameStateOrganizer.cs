using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Superorganism.AI;
using Superorganism.Collisions;
using Superorganism.Common;
using Superorganism.Core.Camera;
using Superorganism.Core.InventorySystem;
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

        public Vector2 GetPlayerPosition() => _entityOraganizer.PlayerPosition;
        public float GetPlayerHealth() => _entityOraganizer.PlayerHealth;
        public float GetPlayerMaxHealth() => _entityOraganizer.PlayerMaxHealth;
        public float GetPlayerStamina() => _entityOraganizer.PlayerStamina;
        public float GetPlayerMaxStamina() => _entityOraganizer.PlayerMaxStamina;
        public float GetPlayerHunger() => _entityOraganizer.PlayerHunger;
        public float GetPlayerMaxHunger() => _entityOraganizer.PlayerMaxHunger;
        public EntityStatus GetPlayerEntityStatus => _entityOraganizer.PlayerEntityStatus;
        public Ant GetPlayerAnt() => _entityOraganizer.PlayerAnt;
        public void ResumeMusic() => _audioManager.ResumeMusic();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="content"></param>
        /// <param name="graphicsDevice"></param>
        /// <param name="camera"></param>
        /// <param name="audio"></param>
        /// <param name="map"></param>
        /// <param name="gameStateInfo"></param>
        public GameStateOrganizer(Game game, ContentManager content, GraphicsDevice graphicsDevice,
            Camera2D camera, GameAudioManager audio, TiledMap map, GameStateInfo gameStateInfo)
        {
            //DecisionMaker.Entities.Clear();
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

        private void InitializeGameState()
        {
            IsGameOver = false;
            IsGameWon = false;
            ElapsedTime = 0;
            _enemyCollisionTimer = 0;
            CropsLeft = _entityOraganizer.CropsCount;
        }

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

        public bool HandlePauseInput(InputState input, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            return _pauseAction.Occurred(input, controllingPlayer, out playerIndex);
        }

        public void Reset()
        {
            _entityOraganizer.Reset();
            InitializeGameState();
        }

        /// <summary>
        /// Creates a dropped item from an inventory item at the specified position
        /// </summary>
        /// <param name="itemName">Name of the item</param>
        /// <param name="itemDescription">Description of the item</param>
        /// <param name="position">Position to create the item at</param>
        /// <param name="texture">Texture of the item</param>
        /// <param name="sourceRect">Source rectangle for sprite atlas</param>
        /// <param name="isSpriteAtlas">Whether the texture is a sprite atlas</param>
        /// <param name="isFromTileset">Whether the item is from a tileset</param>
        /// <param name="tilesetIndex">Index of the tileset</param>
        /// <param name="tileIndex">Index of the tile in the tileset</param>
        /// <param name="scale">Scale factor for the item</param>
        /// <summary>
        /// Creates a dropped item from an inventory item at the specified position
        /// </summary>
        public void CreateItemDropFromInventory(
            string itemName,
            string itemDescription,
            Vector2 position,
            Texture2D texture,
            Rectangle sourceRect,
            bool isSpriteAtlas = false,
            bool isFromTileset = false,
            int tilesetIndex = -1,
            int tileIndex = -1,
            float scale = 1.0f)
        {
            if (texture == null)
                return;

            // Calculate dimensions
            int width = sourceRect != Rectangle.Empty ? sourceRect.Width : texture.Width;
            int height = sourceRect != Rectangle.Empty ? sourceRect.Height : texture.Height;

            // Create a new dropped item
            DroppedItem droppedItem = new()
            {
                ItemName = itemName,
                ItemDescription = itemDescription,
                Position = position, // Use exact player position
                Texture = texture,
                SourceRectangle = sourceRect,
                IsSpriteAtlas = isSpriteAtlas,
                IsFromTileset = isFromTileset,
                TilesetIndex = tilesetIndex,
                TileIndex = tileIndex,
                Color = Color.White,
                Collected = false,
                // Create proper TextureInfo
                TextureInfo = new TextureInfo
                {
                    TextureWidth = texture.Width,
                    TextureHeight = texture.Height,
                    NumOfSpriteCols = 1,
                    NumOfSpriteRows = 1,
                    SizeScale = scale,
                    Center = new Vector2(width / 2.0f, height / 2.0f),
                }
            };

            // Create collision bounds slightly smaller than the item for better collision
            float boundingWidth = width * scale * 0.8f;
            float boundingHeight = height * scale * 0.8f;

            BoundingRectangle boundingRect = new(
                position.X - (boundingWidth / 2),
                position.Y - (boundingHeight / 2),
                boundingWidth,
                boundingHeight
            );
            droppedItem.CollisionBounding = boundingRect;

            // Initialize with slight downward velocity
            droppedItem.InitializePhysics(new Vector2(0, 1.0f));

            // Add to entities
            DecisionMaker.Entities.Add(droppedItem);
        }

        /// <summary>
        /// Finds the nearest droppable item that can be collected near the player.
        /// </summary>
        /// <returns>The nearest collectable item, or null if none found</returns>
        public DroppedItem FindNearestCollectibleItem()
        {
            Ant playerAnt = _entityOraganizer.PlayerAnt;
            if (playerAnt == null || playerAnt.CollisionBounding == null)
                return null;

            // Find all DroppedItems that are not collected yet and have landed on the ground
            List<DroppedItem> droppedItems = DecisionMaker.Entities
                .OfType<DroppedItem>()
                .Where(i => !i.Collected && i.CanBeCollected)
                .ToList();

            if (droppedItems.Count == 0)
                return null;

            // Find the nearest item to the player
            DroppedItem nearestItem = null;
            float nearestDistance = float.MaxValue;

            foreach (DroppedItem item in droppedItems)
            {
                // Check if the item is within pickup range (collision + small radius)
                if (item.CollisionBounding != null &&
                    (item.CollisionBounding.CollidesWith(playerAnt.CollisionBounding) ||
                     Vector2.Distance(item.Position, playerAnt.Position) < 100)) // 100px pickup radius
                {
                    float distance = Vector2.Distance(item.Position, playerAnt.Position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestItem = item;
                    }
                }
            }

            return nearestItem;
        }

        /// <summary>
        /// Collects a specific dropped item and adds it to the player's inventory.
        /// </summary>
        /// <param name="item">The item to collect</param>
        /// <returns>True if the item was collected, false otherwise</returns>
        public bool CollectDroppedItem(DroppedItem item)
        {
            if (item == null || item.Collected || !item.CanBeCollected)
                return false;

            Ant playerAnt = _entityOraganizer.PlayerAnt;
            if (playerAnt == null)
                return false;

            // Create inventory item from the dropped item
            InventoryItem inventoryItem = new(
                item.ItemName,
                1,  // Just pick up one item
                item.ItemDescription);

            // Copy all visual properties
            if (item.Texture != null)
            {
                inventoryItem.Texture = item.Texture;
                inventoryItem.SourceRectangle = item.SourceRectangle;
                inventoryItem.IsSpriteAtlas = item.IsSpriteAtlas;
                inventoryItem.IsFromTileset = item.IsFromTileset;
                inventoryItem.TilesetIndex = item.TilesetIndex;
                inventoryItem.TileIndex = item.TileIndex;
                inventoryItem.Scale = item.TextureInfo.SizeScale * 2.0f; // Double scale since we halved it when dropping
            }

            // Add to player inventory
            playerAnt.Inventory.Add(inventoryItem);

            // Mark as collected
            item.Collected = true;
            item.CanBeCollected = false;

            // Play pickup sound
            _audioManager?.PlayCropPickup();

            return true;
        }

        /// <summary>
        /// Checks for collisions between the player and any uncollected dropped items.
        /// </summary>
        /// <returns>True if any collision occurred, otherwise false.</returns>
        public bool CheckDroppedItemCollisions()
        {
            bool collisionOccurred = false;
            Ant playerAnt = _entityOraganizer.PlayerAnt;

            if (playerAnt == null || playerAnt.CollisionBounding == null)
                return false;

            // Find all DroppedItems that are not collected yet
            List<DroppedItem> droppedItems = DecisionMaker.Entities.OfType<DroppedItem>().Where(i => !i.Collected).ToList();

            foreach (DroppedItem item in droppedItems)
            {
                if (item.CollisionBounding != null &&
                    item.CollisionBounding.CollidesWith(playerAnt.CollisionBounding))
                {
                    // Create inventory item from the dropped item
                    InventoryItem inventoryItem = new(
                        item.ItemName,
                        1,  // Add just one item
                        item.ItemDescription);

                    // Copy all visual properties
                    if (item.Texture != null)
                    {
                        inventoryItem.Texture = item.Texture;
                        inventoryItem.SourceRectangle = item.SourceRectangle;
                        inventoryItem.IsSpriteAtlas = item.IsSpriteAtlas;
                        inventoryItem.IsFromTileset = item.IsFromTileset;
                        inventoryItem.TilesetIndex = item.TilesetIndex;
                        inventoryItem.TileIndex = item.TileIndex;
                        inventoryItem.Scale = item.TextureInfo.SizeScale * 2.0f; // Double scale since we halved it when dropping
                    }

                    // Add to player inventory
                    playerAnt.Inventory.Add(inventoryItem);

                    // Mark as collected
                    item.Collected = true;
                    collisionOccurred = true;

                    // Play pickup sound
                    _audioManager?.PlayCropPickup();
                }
            }

            return collisionOccurred;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _entityOraganizer.Draw(gameTime, spriteBatch);
        }


        public void Unload()
        {
            _entityOraganizer.Unload();
        }

        public ContentManager GetContentManager()
        {
            return _content;
        }
    }
}