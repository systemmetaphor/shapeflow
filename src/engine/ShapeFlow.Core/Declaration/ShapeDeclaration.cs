using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShapeFlow.Collections;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
{
    public class ShapeDeclaration
    {
        private readonly Dictionary<string, string> _parameters;

        private ShapeDeclaration()
        {
            _parameters = new Dictionary<string, string>();
        }
        
        public string Name { get; private set; }

        public string LoaderName { get; private set; }

        public IEnumerable<string> Tags { get; private set; }

        public IDictionary<string, string> Parameters => _parameters;

        public static ShapeDeclaration Create(string modelName, string loaderName, IEnumerable<string> tags, IDictionary<string, string> parameters)
        {
            var modelDeclaration = new ShapeDeclaration
            {
                Name = modelName,
                LoaderName = loaderName,
                Tags = tags,
            };

            modelDeclaration._parameters.AddOrUpdate(parameters);

            return modelDeclaration;
        }

        public static ShapeDeclaration Parse(JObject modelObject, string modelName)
        {
            var loaderName = modelObject.GetStringPropertyValue("loaderName");
            var tagsText = modelObject.GetStringPropertyValue("tags");
            var tags = TagsParser.Parse(tagsText);
            var parametersObject = modelObject.Property("parameters")?.Value as JObject;
            var parameters = parametersObject?.ToParametersDictionary() ?? new Dictionary<string, string>();
            return Create(modelName, loaderName, tags, parameters);
        }

        public string GetParameter(string name)
        {
            _parameters.TryGetValue(name, out var result);
            return result;
        }

        public void SetParameter(string name, string value)
        {
            _parameters.AddOrUpdate(name, value);
        }

        internal static void WriteTo(JsonTextWriter writer, ShapeDeclaration shape)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(LoaderName).ToCamelCase());
            writer.WriteValue(shape.LoaderName);

            writer.WritePropertyName(nameof(Tags).ToCamelCase());
            writer.WriteValue(string.Join(";", shape.Tags));

            writer.WritePropertyName(nameof(Parameters).ToCamelCase());
            shape.Parameters.WriteAsObjectLiteral(writer);

            writer.WriteEndObject();
        }
    }
}