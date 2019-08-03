using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShapeFlow.Collections;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
{
    public class OutputDeclaration
    {
        private OutputDeclaration()
        {
        }

        public string Format
        {
            get;
            private set;
        }

        public string LoaderName
        {
            get;
            private set;
        }

        public IDictionary<string, string> OutputParameters
        {
            get;
            private set;
        }

        public static OutputDeclaration Create(string format, string loaderName,
            IDictionary<string, string> outputParameters)
        {
            return new OutputDeclaration
            {
                Format = format,
                LoaderName = loaderName,
                OutputParameters = outputParameters ?? new Dictionary<string, string>()
            };
        }

        public static OutputDeclaration Parse(JObject outputObject)
        {
            var outputType = outputObject.GetStringPropertyValue("format");
            var loaderName = outputObject.GetStringPropertyValue("loaderName");
            var outputParametersObject = outputObject.GetValue("parameters") as JObject;

            var outputParameters = outputParametersObject?.ToParametersDictionary() ?? new Dictionary<string, string>();
            var output = new OutputDeclaration
            {
                Format = outputType,
                LoaderName = loaderName,
                OutputParameters = outputParameters
            };

            return output;
        }

        public static void WriteTo(JsonTextWriter writer, OutputDeclaration value)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(Format).ToCamelCase());
            writer.WriteValue(value.Format);

            writer.WritePropertyName(nameof(LoaderName).ToCamelCase());
            writer.WriteValue(value.LoaderName);

            writer.WritePropertyName(nameof(OutputParameters).ToCamelCase());
            value.OutputParameters.WriteAsObjectLiteral(writer);

            writer.WriteEndObject();
        }
    }
}