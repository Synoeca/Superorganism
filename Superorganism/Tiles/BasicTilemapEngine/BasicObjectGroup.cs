using Superorganism.Tiles.TilemapEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Tiles.BasicTilemapEngine
{
    public class BasicObjectGroup
    {
        public SortedList<string, BasicObject> Objects = new();
        public SortedList<string, string> Properties = new();

        public string Name;
        public int Width, Height, X, Y;
        private float _opacity = 1;

        /// <summary>
        /// Loads the object group from a TMX file
        /// </summary>
        /// <param name="reader">A reader to the TMX file being processed</param>
        /// <returns>An initialized ObjectGroup</returns>
        internal static BasicObjectGroup Load(XmlReader reader)
        {
            BasicObjectGroup result = new();
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            if (reader.GetAttribute("name") != null)
                result.Name = reader.GetAttribute("name");
            if (reader.GetAttribute("width") != null)
                result.Width = int.Parse(reader.GetAttribute("width") ?? throw new InvalidOperationException());
            if (reader.GetAttribute("height") != null)
                result.Height = int.Parse(reader.GetAttribute("height") ?? throw new InvalidOperationException());
            if (reader.GetAttribute("x") != null)
                result.X = int.Parse(reader.GetAttribute("x") ?? throw new InvalidOperationException());
            if (reader.GetAttribute("y") != null)
                result.Y = int.Parse(reader.GetAttribute("y") ?? throw new InvalidOperationException());
            if (reader.GetAttribute("opacity") != null)
                result._opacity = float.Parse(reader.GetAttribute("opacity") ?? throw new InvalidOperationException(), NumberStyles.Any, ci);

            while (!reader.EOF)
            {
                string name = reader.Name;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (name)
                        {
                            case "object":
                                {
                                    using XmlReader st = reader.ReadSubtree();
                                    st.Read();
                                    BasicObject objects = BasicObject.Load(st);
                                    if (!result.Objects.TryAdd(objects.Name, objects))
                                    {
                                        int count = result.Objects.Keys.Count((item) => item.Equals(objects.Name));
                                        result.Objects.Add($"{objects.Name}{count}", objects);
                                    }
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
                }

                reader.Read();
            }

            return result;
        }

        public void Draw(BasicMap result, SpriteBatch batch, Rectangle rectangle, Vector2 viewportPosition)
        {
            foreach (BasicObject objects in Objects.Values)
            {
                if (objects.TileTexture != null)
                {
                    objects.Draw(batch, rectangle, new Vector2(X * result.TileWidth, Y * result.TileHeight), viewportPosition, _opacity);
                }
            }
        }
    }
}
