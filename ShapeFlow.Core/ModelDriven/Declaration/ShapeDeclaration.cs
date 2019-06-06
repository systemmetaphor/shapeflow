using System.Collections.Generic;
using ShapeFlow.Infrastructure;
using Newtonsoft.Json.Linq;

namespace ShapeFlow
{
    public class ShapeDeclaration
    {
        private readonly IDictionary<string, string> _parameters;

        public ShapeDeclaration(Solution solution, string modelName, string loaderName, IEnumerable<string> tags, Dictionary<string, string> parameters)
        {
            Solution = solution;
            ModelName = modelName;
            LoaderName = loaderName;
            Tags = tags;
            _parameters = parameters;
        }

        public Solution Solution { get; }

        public string ModelName { get; }

        public string LoaderName { get; }

        public IEnumerable<string> Tags { get; }

        public IEnumerable<KeyValuePair<string, string>> Parameters => _parameters;

        public static ShapeDeclaration Parse(Solution solution, JObject modelObject)
        {
            var modelName = modelObject.GetStringPropertyValue("name");
            var loaderName = modelObject.GetStringPropertyValue("loaderName");
            var tagsText = modelObject.GetStringPropertyValue("tags");
            var tags = TagsParser.Parse(tagsText);
            var parametersObject = modelObject.Property("parameters")?.Value as JObject;
            var parameters = parametersObject.ToParametersDictionary();
            var modelDeclaration = new ShapeDeclaration(solution, modelName, loaderName, tags, parameters);
            return modelDeclaration;
        }

        public string GetParameter(string name)
        {
            if (_parameters?.ContainsKey(name) ?? false)
            {
                return _parameters[name];
            }

            return null;
        }

        public void SetParameter(string name, string value)
        {
            if (_parameters?.ContainsKey(name) ?? false)
            {
                _parameters[name] = value;
            }
            else
            {
                _parameters.Add(name, value);
            }
        }
    }
}