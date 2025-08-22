using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ShapeFlow.ModelToCode
{
    public static class SafeName
    {
        private static readonly Regex nonDigitOrLetterReplacer = new Regex("[^a-zA-Z0-9]");
        
        public static string ToSafeName(this string value, string invalidReplacement = "_")
        {
            var validText = nonDigitOrLetterReplacer.Replace(value.Trim(), (m) => ReplaceInvalidChar(m.Value, invalidReplacement));
            return validText;
        }

        private static string ReplaceInvalidChar(string value, string replacer)
        {
            switch (value)
            {
                case "ã": return "a";
                case "é": return "e";
                case "ê": return "e";
                case "á": return "a";
                case "à": return "a";
                case "ç": return "c";
                case "ó": return "o";

                case "Ã": return "A";
                case "É": return "E";
                case "Ê": return "E";
                case "Á": return "A";
                case "À": return "A";
                case "Ç": return "C";
                case "Ó": return "O";

                case "%": return "PERCENTAGE";
            }

            return replacer;
        }
    }
}
