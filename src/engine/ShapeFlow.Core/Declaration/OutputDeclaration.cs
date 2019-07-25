using System.Collections.Generic;
using Newtonsoft.Json.Linq;
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

        public Dictionary<string, string> OutputParameters
        {
            get;
            private set;
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
    }
}