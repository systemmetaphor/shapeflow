using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ShapeFlow.ModelDriven
{
    public static class ParametersObjectExtensions
    {
        public static Dictionary<string, string> ToParametersDictionary(this JObject parametersObject)
        {

            var parameters = new Dictionary<string, string>();

            foreach (var parameterProperty in parametersObject)
            {
                var parameterName = parameterProperty.Key;
                var parameterValue = parameterProperty.Value.Value<string>();
                parameters.Add(parameterName, parameterValue);
            }

            return parameters;
        }
    }
}