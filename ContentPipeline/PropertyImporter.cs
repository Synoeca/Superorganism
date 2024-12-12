using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Xml;

namespace ContentPipeline
{
    public static class PropertyImporter
    {
        public static SortedList<string, string> ImportProperties(XmlReader reader)
        {
            SortedList<string, string> properties = new SortedList<string, string>();

            using XmlReader? subtree = reader.ReadSubtree();
            while (subtree.Read())
            {
                if (subtree.NodeType == XmlNodeType.Element && subtree.Name == "property")
                {
                    string name = subtree.GetAttribute("name") ?? throw new ContentLoadException("Property missing name attribute");
                    string value = subtree.GetAttribute("value") ?? string.Empty;
                    properties[name] = value;
                }
            }

            return properties;
        }
    }
}