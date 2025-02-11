using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.Xna.Framework;

namespace Superorganism.Tiles
{
    /// <summary>
    /// Utility class containing common XML parsing methods used across the tilemap engine
    /// </summary>
    public static class XmlParsingUtilities
    {
        /// <summary>
        /// Parses an integer attribute from an XML element
        /// </summary>
        /// <param name="reader">The XML reader</param>
        /// <param name="attributeName">Name of the attribute to parse</param>
        /// <param name="defaultValue">Default value if attribute is missing or invalid</param>
        /// <returns>The parsed integer value</returns>
        public static int ParseIntAttribute(XmlReader reader, string attributeName, int defaultValue = 0)
        {
            string value = reader.GetAttribute(attributeName);
            return string.IsNullOrEmpty(value) ? defaultValue :
                   int.TryParse(value, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// Parses a float attribute from an XML element
        /// </summary>
        /// <param name="reader">The XML reader</param>
        /// <param name="attributeName">Name of the attribute to parse</param>
        /// <param name="defaultValue">Default value if attribute is missing or invalid</param>
        /// <returns>The parsed float value</returns>
        public static float ParseFloatAttribute(XmlReader reader, string attributeName, float defaultValue = 0.0f)
        {
            string value = reader.GetAttribute(attributeName);
            if (string.IsNullOrEmpty(value)) return defaultValue;

            return float.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float result
            ) ? result : defaultValue;
        }

        /// <summary>
        /// Parses a boolean attribute from an XML element
        /// </summary>
        /// <param name="reader">The XML reader</param>
        /// <param name="attributeName">Name of the attribute to parse</param>
        /// <param name="defaultValue">Default value if attribute is missing or invalid</param>
        /// <returns>The parsed boolean value</returns>
        public static bool ParseBoolAttribute(XmlReader reader, string attributeName, bool defaultValue = false)
        {
            string value = reader.GetAttribute(attributeName);
            if (string.IsNullOrEmpty(value)) return defaultValue;

            // Handle numeric boolean values
            if (value == "1") return true;
            if (value == "0") return false;

            return bool.TryParse(value, out bool result) ? result : defaultValue;
        }

        /// <summary>
        /// Parses a color attribute from an XML element
        /// </summary>
        /// <param name="colorStr">The color string to parse</param>
        /// <returns>The parsed Color value, or null if invalid</returns>
        public static Color? ParseColor(string colorStr)
        {
            if (string.IsNullOrEmpty(colorStr)) return null;

            colorStr = colorStr.TrimStart('#');

            try
            {
                return colorStr.Length switch
                {
                    6 => ParseRGBColor(colorStr),    // RGB format
                    8 => ParseARGBColor(colorStr),   // ARGB format
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }

        private static Color ParseRGBColor(string colorStr)
        {
            int r = Convert.ToInt32(colorStr[..2], 16);
            int g = Convert.ToInt32(colorStr[2..4], 16);
            int b = Convert.ToInt32(colorStr[4..6], 16);
            return new Color(r, g, b);
        }

        private static Color ParseARGBColor(string colorStr)
        {
            int a = Convert.ToInt32(colorStr[..2], 16);
            int r = Convert.ToInt32(colorStr[2..4], 16);
            int g = Convert.ToInt32(colorStr[4..6], 16);
            int b = Convert.ToInt32(colorStr[6..8], 16);
            return new Color(r, g, b, a);
        }

        /// <summary>
        /// Loads properties from an XML element into a dictionary
        /// </summary>
        /// <param name="reader">The XML reader</param>
        /// <param name="properties">The dictionary to store properties in</param>
        public static void LoadProperties(XmlReader reader, IDictionary<string, string> properties)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "property")
                {
                    string name = reader.GetAttribute("name");
                    string value = reader.GetAttribute("value") ?? string.Empty;

                    if (!string.IsNullOrEmpty(name))
                    {
                        properties[name] = value;
                    }
                }
            }
        }
    }
}