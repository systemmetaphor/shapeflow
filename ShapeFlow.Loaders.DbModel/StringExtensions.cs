using System;
using System.Globalization;

namespace ShapeFlow.Loaders.DbModel
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            {
                return s;
            }

            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    break;
                }

                char c = char.ToLowerInvariant(chars[i]);
                chars[i] = c;
            }

            return new string(chars);
        }

        public static string ToPascalCase(this string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(text);
            }

            if (text.Length > 1)
            {
                return text.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture) + text.Substring(1, text.Length - 1);
            }
            else
            {
                return text.ToUpper(CultureInfo.InvariantCulture);
            }
        }
    }
}