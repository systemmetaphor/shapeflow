using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ShapeFlow.Declaration
{
    public static class ParametersExtensions
    {
        public static IEnumerable<ParameterDeclaration> ParseParameters(this JObject root, string propertyName)
        {
            var parameters = new List<ParameterDeclaration>();
            var parametersArray = root.GetValue(propertyName) as JArray ?? new JArray();
            foreach (var jToken in parametersArray)
            {
                var parametersObject = (JObject)jToken;
                var parameterDeclaration = ParameterDeclaration.Parse(parametersObject);
                parameters.Add(parameterDeclaration);
            }

            return parameters;
        }
    }
}
