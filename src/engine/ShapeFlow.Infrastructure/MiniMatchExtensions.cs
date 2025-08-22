using Minimatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShapeFlow.Infrastructure
{
    /// <summary>
    /// Contains extension methods to apply glob expressions to paths.    /// 
    /// </summary>
    /// <see cref="https://en.wikipedia.org/wiki/Glob_(programming)"/>
    public static class GlobPatternsExtensions
    {
        private static readonly Regex GlobPatternRegex = new Regex(@"(\\).|([*?]|\[.*\]|\{.*\}|\(.*\|.*\)|^!)");
        private static readonly Options _options = new Options
        {
            AllowWindowsPaths = true,
            IgnoreCase = true
        };

        /// <summary>
        /// Checks if the given string contains a glob expression.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the given string contains a glob expression; false otherwise.</returns>
        public static bool IsGlobPattern(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            Match match;

            while ((match = GlobPatternRegex.Match(value)).Success)
            {
                if (match.Groups.Count >= 2)
                {
                    var matchedGroup = match.Groups[2];
                    if (!string.IsNullOrWhiteSpace(matchedGroup.Value))
                    {
                        return true;
                    }
                }

                value = value.Substring(match.Index + match.Groups[0].Length);
            }

            return false;
        }

        /// <summary>
        /// Checks if the string matches the given glob expression.
        /// </summary>
        /// <param name="what">The string to try to match.</param>
        /// <param name="expression">The glob expression.</param>
        /// <returns>True if the given string matches the glob expression; false otherwise.</returns>
        public static bool MatchesGlobExpression(this string what, string expression)
        {
            if(string.IsNullOrWhiteSpace(what))
            {
                throw new ArgumentNullException(nameof(what));
            }

            if(string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentNullException(nameof(expression));
            }

            return Minimatcher.Check(what, expression, _options);
        }
    }
}
