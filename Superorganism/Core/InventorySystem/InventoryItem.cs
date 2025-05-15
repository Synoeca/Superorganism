#nullable enable
using System.Collections.Generic;
using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Core.InventorySystem
{
    /// <summary>
    /// Represents an item in the inventory with change notifications.
    /// </summary>
    public class InventoryItem : INotifyPropertyChanged
    {
        /// <summary>
        /// Event triggered when a property changes
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _name;
        /// <summary>
        /// Name of the inventory item
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        private int _quantity;
        /// <summary>
        /// Quantity of the item in inventory
        /// </summary>
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantity)));
            }
        }

        private string _description;
        /// <summary>
        /// Description of the inventory item
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
            }
        }

        private Texture2D? _texture;
        /// <summary>
        /// The texture of the item (full texture)
        /// </summary>
        public Texture2D? Texture
        {
            get => _texture;
            set
            {
                _texture = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Texture)));
            }
        }

        private Rectangle _sourceRectangle;
        /// <summary>
        /// The source rectangle within the texture (for sprite atlases)
        /// </summary>
        public Rectangle SourceRectangle
        {
            get => _sourceRectangle;
            set
            {
                _sourceRectangle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceRectangle)));
            }
        }

        private bool _isSpriteAtlas;
        /// <summary>
        /// Whether the texture is a sprite atlas
        /// </summary>
        public bool IsSpriteAtlas
        {
            get => _isSpriteAtlas;
            set
            {
                _isSpriteAtlas = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSpriteAtlas)));
            }
        }

        private float _scale = 1.0f;
        /// <summary>
        /// Scale factor for the item's texture
        /// </summary>
        public float Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Scale)));
            }
        }

        private bool _isFromTileset;
        /// <summary>
        /// Whether this item was created from a tileset
        /// </summary>
        public bool IsFromTileset
        {
            get => _isFromTileset;
            set
            {
                _isFromTileset = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFromTileset)));
            }
        }

        private int _tilesetIndex = -1;
        /// <summary>
        /// The index of the tileset this item was created from (-1 if not from a tileset)
        /// </summary>
        public int TilesetIndex
        {
            get => _tilesetIndex;
            set
            {
                _tilesetIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TilesetIndex)));
            }
        }

        private int _tileIndex = -1;
        /// <summary>
        /// The index of the tile in the tileset this item was created from (-1 if not from a tileset)
        /// </summary>
        public int TileIndex
        {
            get => _tileIndex;
            set
            {
                _tileIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TileIndex)));
            }
        }

        /// <summary>
        /// Initializes a new instance of the InventoryItem class.
        /// </summary>
        /// <param name="name">The name of the item</param>
        /// <param name="quantity">The quantity of the item</param>
        /// <param name="description">The description of the item</param>
        public InventoryItem(string name, int quantity, string description)
        {
            _name = name;
            _quantity = quantity;
            _description = description;
            _sourceRectangle = Rectangle.Empty;
            _isFromTileset = false;
            _tilesetIndex = -1;
            _tileIndex = -1;
        }

        /// <summary>
        /// Initializes a new instance of the InventoryItem class with texture info.
        /// </summary>
        /// <param name="name">The name of the item</param>
        /// <param name="quantity">The quantity of the item</param>
        /// <param name="description">The description of the item</param>
        /// <param name="texture">The texture of the item</param>
        /// <param name="sourceRectangle">The source rectangle within the texture (for sprite atlases)</param>
        /// <param name="isSpriteAtlas">Whether the texture is a sprite atlas</param>
        /// <param name="scale">Scale factor for rendering the texture</param>
        public InventoryItem(string name, int quantity, string description, Texture2D texture,
            Rectangle sourceRectangle, bool isSpriteAtlas = false, float scale = 1.0f)
            : this(name, quantity, description)
        {
            _texture = texture;
            _sourceRectangle = sourceRectangle;
            _isSpriteAtlas = isSpriteAtlas;
            _scale = scale;
        }

        /// <summary>
        /// Initializes a new instance of the InventoryItem class with tileset info.
        /// </summary>
        /// <param name="name">The name of the item</param>
        /// <param name="quantity">The quantity of the item</param>
        /// <param name="description">The description of the item</param>
        /// <param name="texture">The texture of the item</param>
        /// <param name="sourceRectangle">The source rectangle within the texture</param>
        /// <param name="tilesetIndex">The index of the tileset</param>
        /// <param name="tileIndex">The index of the tile in the tileset</param>
        /// <param name="scale">Scale factor for rendering the texture</param>
        public InventoryItem(string name, int quantity, string description, Texture2D texture,
            Rectangle sourceRectangle, int tilesetIndex, int tileIndex, float scale = 1.0f)
            : this(name, quantity, description, texture, sourceRectangle, true, scale)
        {
            _isFromTileset = true;
            _tilesetIndex = tilesetIndex;
            _tileIndex = tileIndex;
        }

        /// <summary>
        /// Creates an item from a tileset tile using the tileset index
        /// </summary>
        /// <param name="name">The name of the item</param>
        /// <param name="quantity">The quantity of the item</param>
        /// <param name="description">The description of the item</param>
        /// <param name="tilesets">The collection of tilesets</param>
        /// <param name="tilesetIndex">The index of the tileset in the collection</param>
        /// <param name="tileIndex">The index of the tile in the tileset</param>
        /// <returns>A new InventoryItem with the tile's texture information</returns>
        public static InventoryItem CreateFromTileset(string name, int quantity, string description,
            SortedList<string, Tiles.Tileset> tilesets, int tilesetIndex, int tileIndex)
        {
            // Get the tileset from the collection using the index
            if (tilesetIndex < 0 || tilesetIndex >= tilesets.Count)
            {
                throw new ArgumentException($"Tileset index {tilesetIndex} is out of range");
            }

            // Get the tileset at the specified index
            Tiles.Tileset tileset = tilesets.Values[tilesetIndex];

            if (tileset == null)
            {
                throw new ArgumentException($"No tileset found at index {tilesetIndex}");
            }

            Rectangle sourceRect = Rectangle.Empty;
            tileset.MapTileToRect(tileIndex, ref sourceRect);

            // Create the item with the tileset and tile indices
            InventoryItem item = new(
                name,
                quantity,
                description,
                tileset.TileTexture,
                sourceRect,
                tilesetIndex,
                tileIndex
            );

            return item;
        }

        /// <summary>
        /// Creates an item from an animated entity's sprite frame
        /// </summary>
        /// <param name="name">The name of the item</param>
        /// <param name="quantity">The quantity of the item</param>
        /// <param name="description">The description of the item</param>
        /// <param name="entity">The entity to use as a source</param>
        /// <param name="frameIndex">Which animation frame to use (defaults to 0)</param>
        /// <param name="directionIndex">Which direction to use (defaults to 0)</param>
        /// <returns>A new InventoryItem with the entity's sprite information</returns>
        public static InventoryItem CreateFromEntity(string name, int quantity, string description,
            Entities.MovableAnimatedEntity entity, int frameIndex = 0, int directionIndex = 0)
        {
            if (entity?.Texture == null || entity.TextureInfo == null)
                return new InventoryItem(name, quantity, description);

            Rectangle sourceRect;

            if (entity.HasDirection)
            {
                sourceRect = new Rectangle(
                    (int)(frameIndex * (entity.TextureInfo.TextureWidth / entity.TextureInfo.NumOfSpriteCols)),
                    (int)(directionIndex * (entity.TextureInfo.TextureHeight / entity.TextureInfo.NumOfSpriteRows)),
                    (int)(entity.TextureInfo.TextureWidth / entity.TextureInfo.NumOfSpriteCols),
                    (int)(entity.TextureInfo.TextureHeight / entity.TextureInfo.NumOfSpriteRows)
                );
            }
            else
            {
                sourceRect = new Rectangle(
                    (int)(frameIndex * (entity.TextureInfo.TextureWidth / entity.TextureInfo.NumOfSpriteCols)),
                    0,
                    (int)(entity.TextureInfo.TextureWidth / entity.TextureInfo.NumOfSpriteCols),
                    (int)entity.TextureInfo.TextureHeight
                );
            }

            return new InventoryItem(name, quantity, description, entity.Texture, sourceRect, true, entity.TextureInfo.SizeScale);
        }
    }
}