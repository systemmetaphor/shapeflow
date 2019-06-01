using System.Collections.Generic;
using ShapeFlow.Infrastructure;
using Newtonsoft.Json.Linq;

namespace ShapeFlow
{
    public class OutputDeclaration
    {
        public OutputDeclaration(string outputerType, Dictionary<string, string> outputerParameters)
        {
            OutputerType = outputerType;
            OutputerParameters = outputerParameters;
        }

        public string OutputerType { get; }
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