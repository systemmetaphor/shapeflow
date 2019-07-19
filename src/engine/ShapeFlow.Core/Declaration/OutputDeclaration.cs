using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
{
    public class OutputDeclaration
    {
        public OutputDeclaration(string outputType, Dictionary<string, string> outputerParameters)
        {
            OutputType = outputType;
            OutputerParameters = outputerParameters;
        }

        public string OutputType { get; }
        public Dictionary<string, string> OutputerParameters { get; }

        public static OutputDeclaration Parse(JObject outputObject)
        {
            var outputType = outputObject.GetStringPropertyValue("type");
            var outputParametersObject = outputObject.GetValue("parameters") as JObject;
            var outputParameters = outputParametersObject.ToParametersDictionary();
            var output = new OutputDeclaration(outputType, outputParameters);
            return output;
        }
    }
}