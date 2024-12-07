using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Tiles
{
    public class Tilemap(string filename)
    {
        /// <summary>
        /// The dimensions of tiles and the map
        /// </summary>
        private int _tileWidth;

        private int _tileHeight;
        private int _mapWidth;
        private int _mapHeight;

        /// <summary>
        /// The tileset texture
        /// </summary>
        private Texture2D _tilesetTexture;

        /// <summary>
        /// The tile info in the tileset
        /// </summary>
        private Rectangle[] _tiles;

        /// <summary>
        /// The tile map data
        /// </summary>
        private int[] _map;

        /// <summary>
        /// The filename of the map file
        /// </summary>
        private readonly string _filename = filename;

        public void LoadContent(ContentManager content)
        {
            string data = File.ReadAllText(Path.Join(content.RootDirectory, _filename));
            string[] lines = data.Split('\n');

            // First line is the tileset filename
            string tilesetFilename = lines[0].Trim();
            _tilesetTexture = content.Load<Texture2D>(tilesetFilename);

            // Second line is the tile size
            string[] secondLine = lines[1].Split(',');
            _tileWidth = int.Parse(secondLine[0]);
            _tileHeight = int.Parse(secondLine[1]);

            // Now we can determine our tile bounds
            int tilesetColumns = _tilesetTexture.Width / _tileWidth;
            int tilesetRows = _tilesetTexture.Height / _tileHeight;
            _tiles = new Rectangle[tilesetColumns * tilesetRows];

            for (int y = 0; y < tilesetColumns; y++)
            {
                for (int x = 0; x < tilesetRows; x++)
                {
                    int index = y * tilesetColumns + x;
                    _tiles[index] = new Rectangle(
                        x * _tileWidth,
                        y * _tileHeight,
                        _tileWidth,
                        _tileHeight);
                }
            }

            // Third line is the map size
            string[] thirdLine = lines[2].Split(',');
            _mapWidth = int.Parse(thirdLine[0]);
            _mapHeight = int.Parse(thirdLine[1]);

            // Now we can create our map
            string[] fourthLine = lines[3].Split(',');
            _map = new int[_mapWidth * _mapHeight];
            for (int i = 0; i < _mapWidth * _mapHeight; i++)
            {
                _map[i] = int.Parse(fourthLine[i]);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    int index = _map[y * _mapWidth + x] - 1;
                    if (index == -1)
                    {
                        continue;
                    }

                    spriteBatch.Draw(
                        _tilesetTexture,
                        new Vector2(
                            x * _tileWidth,
                            y * _tileHeight
                        ),
                        _tiles[index],
                        Color.White
                    );
                }
            }
        }
    }
}
