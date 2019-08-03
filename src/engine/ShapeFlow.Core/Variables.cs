using System;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ShapeFlow
{
    public class Variables
    {
        public static readonly string ExpressionPrefix = "$(";
        public static readonly string ExpressionSuffix = ")";

        public Variables(JObject root)
        {
            Root = root;
        }

        protected JObject Root { get; }

        /// <summary>
        /// Parse the input value resolving the macro expressions.
        /// </summary>
        /// <param name="value">The input value.</param>
        /// <returns>The result of processing the macro expressions.</returns>
        public string Resolve(string targetValue)
        {
            int startIndex = 0;
            int prefixIndex;
            int suffixIndex;

            while (startIndex < targetValue.Length &&
                (prefixIndex = targetValue.IndexOf(ExpressionPrefix, startIndex, StringComparison.Ordinal)) >= 0 &&
                (suffixIndex = targetValue.IndexOf(ExpressionSuffix, prefixIndex + ExpressionPrefix.Length, StringComparison.Ordinal)) >= 0)
            {
                string variableKey = targetValue.Substring(prefixIndex + ExpressionPrefix.Length, suffixIndex - prefixIndex - ExpressionPrefix.Length);

                if (!string.IsNullOrEmpty(variableKey) && TryGetValue(variableKey, out string variableValue))
                {
                    // TODO: If a variable is returned as null it is a sign that something is wrong and I
                    // should give users feedback on this
                    targetValue = string.Concat(
                        targetValue.Substring(0, prefixIndex),
                        variableValue ?? string.Empty,
                        targetValue.Substring(suffixIndex + ExpressionSuffix.Length));

                    startIndex = prefixIndex + (variableValue ?? string.Empty).Length;
                }
                else
                {
                    startIndex = prefixIndex + 1;
                }
            }

            return targetValue;
        }

        bool TryGetValue(string name, out string val)
        {
            if (Root.TryGetValue(name, out JToken variable))
            {
                val = variable?.Value<string>() ?? string.Empty;                
                return true;
            }

            val = null;            
            return false;
        }
    }
}
