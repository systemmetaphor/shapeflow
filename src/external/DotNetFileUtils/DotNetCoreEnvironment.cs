using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DotNetFileUtils
{
    public class DotNetCoreEnvironment : IEnvironment
    {
        private static readonly Regex _regex = new Regex("%(.*?)%");

        public DirectoryPath WorkingDirectory => System.IO.Directory.GetCurrentDirectory();

        public string ExpandEnvironmentVariables(string text)
        {
            var variables = System.Environment.GetEnvironmentVariables();

            var dictionary = new Dictionary<string, string>();

            foreach (DictionaryEntry entry in variables)
            {
                dictionary.Add((string)entry.Key, entry.Value as string);
            }

            var matches = _regex.Matches(text);
            foreach (Match match in matches)
            {
                string value = match.Groups[1].Value;
                if (dictionary.ContainsKey(value))
                {
                    text = text.Replace(match.Value, dictionary[value]);
                }
            }

            return text;
        }
    }
}