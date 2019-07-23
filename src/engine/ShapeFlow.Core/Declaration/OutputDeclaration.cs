using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
{
    public class OutputDeclaration
    {
        public OutputDeclaration(string outputType, Dictionary<string, string> outputParameters)
        {
            OutputType = outputType;
            OutputParameters = outputParameters;
        }

        public string OutputType { get; }
        public Dictionary<string, string> OutputParameters { get; }

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