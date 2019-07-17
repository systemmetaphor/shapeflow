using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
{
    public class ShapeDeclaration
    {
        private readonly IDictionary<string, string> _parameters;

        public ShapeDeclaration(string modelName, string loaderName)
            : this(modelName, loaderName, Enumerable.Empty<string>())
        {
        }

        public ShapeDeclaration(string modelName, string loaderName, IEnumerable<string> tags)
            : this(modelName, loaderName, tags, new Dictionary<string, string>())
        {
        }


        public ShapeDeclaration(string modelName, string loaderName, IEnumerable<string> tags, Dictionary<string, string> parameters)
        {
            ModelName = modelName;
            LoaderName = loaderName;
            Tags = tags;
            _parameters = parameters;
        }

        public string ModelName { get; }

        public string LoaderName { get; }

        public IEnumerable<string> Tags { get; }

        public IEnumerable<KeyValuePair<string, string>> Parameters => _parameters;

        public static ShapeDeclaration Parse(JObject modelObject)
        {
            var modelName = modelObject.GetStringPropertyValue("name");
            var loaderName = modelObject.GetStringPropertyValue("loaderName");
            var tagsText = modelObject.GetStringPropertyValue("tags");
            var tags = TagsParser.Parse(tagsText);
            var parametersObject = modelObject.Property("parameters")?.Value as JObject;
            var parameters = parametersObject?.ToParametersDictionary() ?? new Dictionary<string, string>();
            var modelDeclaration = new ShapeDeclaration(modelName, loaderName, tags, parameters);
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