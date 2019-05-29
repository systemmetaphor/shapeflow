using System;
using System.Collections.Generic;
using System.Linq;

namespace ShapeFlow.ModelDriven
{
    internal class TagsParser
    {
        internal static IEnumerable<string> Parse(string tagsText)
        {
            var delimiters = new[] { ' ', ',', ';' };
            return tagsText.Split(delimiters)
                .Select(t => t?.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToArray();
        }
    }
}