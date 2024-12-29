﻿using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Tiles.TilemapEngine
{
    /// <summary>
    /// A class representing a map created by the Tiled editor
    /// </summary>
    public class Map
    {
        /// <summary>
        /// The Map's Tilesets
        /// </summary>
        public SortedList<string, Tileset> Tilesets = new();

        /// <summary>
        /// The Map's Layers
        /// </summary>
        public SortedList<string, Layer> Layers = new();

        /// <summary>
        /// The Map's Object Groups
        /// </summary>
        public SortedList<string, ObjectGroup> ObjectGroups = new();

        /// <summary>
        /// The Map's properties
        /// </summary>
        public SortedList<string, string> Properties = new();

        /// <summary>
        /// The Map's width and height
        /// </summary>
        public int Width, Height;

        /// <summary>
        /// The Map's tile width and height
        /// </summary>
        public int TileWidth, TileHeight;

        /// <summary>
        /// Loads a TMX file into a Map object
        /// </summary>
        /// <param name="filename">The filename of the TMX file</param>
        /// <param name="content">The ContentManager to load textures with</param>
        /// <returns>The loaded map</returns>
        public static Map Load(string filename, ContentManager content)
        {
            Map result = new();
            XmlReaderSettings settings = new()
            {
                DtdProcessing = DtdProcessing.Parse
            };

            using (StreamReader stream = File.OpenText(filename))
            using (XmlReader reader = XmlReader.Create(stream, settings))
                while (reader.Read())
                {
                    string name = reader.Name;

                    switch (reader.NodeType)
                    {
                        case XmlNodeType.DocumentType:
                            if (name != "map")
                                throw new Exception("Invalid map format");
                            break;
                        case XmlNodeType.Element:
                            switch (name)
                            {
                                case "map":
                                    {
                                        result.Width = int.Parse(reader.GetAttribute("width") ?? throw new InvalidOperationException());
                                        result.Height = int.Parse(reader.GetAttribute("height") ?? throw new InvalidOperationException());
                                        result.TileWidth = int.Parse(reader.GetAttribute("tilewidth") ?? throw new InvalidOperationException());
                                        result.TileHeight = int.Parse(reader.GetAttribute("tileheight") ?? throw new InvalidOperationException());
                                    }
                                    break;
                                case "tileset":
                                    {
                                        using XmlReader st = reader.ReadSubtree();
                                        st.Read();
                                        Tileset tileset = Tileset.Load(st);
                                        result.Tilesets.Add(tileset.Name, tileset);
                                    }
                                    break;
                                case "layer":
                                    {
                                        using XmlReader st = reader.ReadSubtree();
                                        st.Read();
                                        Layer layer = Layer.Load(st);
                                        if (null != layer)
                                        {
                                            result.Layers.Add(layer.Name, layer);
                                        }
                                    }
                                    break;
                                case "objectgroup":
                                    {
                                        using XmlReader st = reader.ReadSubtree();
                                        st.Read();
                                        ObjectGroup objectgroup = ObjectGroup.Load(st);
                                        result.ObjectGroups.Add(objectgroup.Name, objectgroup);
                                    }
                                    break;
                                case "properties":
                                    {
                                        using XmlReader st = reader.ReadSubtree();
                                        while (!st.EOF)
                                        {
                                            switch (st.NodeType)
                                            {
                                                case XmlNodeType.Element:
                                                    if (st.Name == "property")
                                                    {
                                                        if (st.GetAttribute("name") != null)
                                                        {
                                                            result.Properties.Add(st.GetAttribute("name") ?? throw new InvalidOperationException(), st.GetAttribute("value"));
                                                        }
                                                    }

                                                    break;
                                                case XmlNodeType.EndElement:
                                                    break;
                                            }

                                            st.Read();
                                        }
                                    }
                                    break;
                            }
                            break;
                        case XmlNodeType.EndElement:
                            break;
                        case XmlNodeType.Whitespace:
                            break;
                    }
                }

            foreach (Tileset tileset in result.Tilesets.Values)
            {
                string relativePath = ContentPaths.GetMapPath(Path.GetFileNameWithoutExtension(tileset.Image));
                tileset.TileTexture = content.Load<Texture2D>(relativePath);
            }

            foreach (ObjectGroup objects in result.ObjectGroups.Values)
            {
                foreach (Object item in objects.Objects.Values)
                {
                    if (item.Image != null)
                    {
                        string relativePath = ContentPaths.GetMapPath(Path.GetFileNameWithoutExtension(item.Image));
                        item.TileTexture = content.Load<Texture2D>(relativePath);
                    }
                }
            }
            MapHelper.AnalyzeMapGround(result);
            return result;
        }

        /// <summary>
        /// Draws the Map
        /// </summary>
        /// <param name="batch">The SpriteBatch to draw with</param>
        /// <param name="viewport">The Viewport to draw within</param>
        /// <param name="cameraPosition">The position of the viewport within the map</param>
        public void Draw(SpriteBatch batch, Rectangle viewport, Vector2 cameraPosition)
        {
            // Calculate the visible area in world coordinates
            Rectangle visibleArea = new(
                (int)cameraPosition.X - viewport.Width / 2,  // Left edge
                (int)cameraPosition.Y - viewport.Height / 2, // Top edge
                viewport.Width,
                viewport.Height
            );

            // Add padding to ensure we draw tiles just outside the visible area
            visibleArea.Inflate(TileWidth * 2, TileHeight * 2);

            // Draw the layers
            foreach (Layer layer in Layers.Values)
            {
                layer.Draw(batch, Tilesets.Values, visibleArea, cameraPosition, TileWidth, TileHeight);
            }

            // Draw the objects
            foreach (ObjectGroup objectGroup in ObjectGroups.Values)
            {
                objectGroup.Draw(this, batch, visibleArea, cameraPosition);
            }
        }
    }
}