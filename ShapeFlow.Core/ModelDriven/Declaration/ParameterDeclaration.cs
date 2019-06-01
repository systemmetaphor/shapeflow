using ShapeFlow.Infrastructure;
using Newtonsoft.Json.Linq;

namespace ShapeFlow
{
    public class ParameterDeclaration
    {
        public ParameterDeclaration(string parameterName, string parameterDescription, string parameterLabel, string parameterType, bool optional)
        {
            ParameterName = parameterName;
            ParameterDescription = parameterDescription;
            ParameterLabel = parameterLabel;
            ParameterType = parameterType;
            Optional = optional;
        }

        public string ParameterName { get; }
        public string ParameterDescription { get; }
        public string ParameterLabel { get; }
        public string ParameterType { get; }
        public bool Optional { get; }

        public static ParameterDeclaration Parse(JObject parametersObject)
        {
            var parameterName = parametersObject.GetStringPropertyValue("name");
            var parameterDescription = parametersObject.GetStringPropertyValue("description");
            var parameterLabel = parametersObject.GetStringPropertyValue("label");
            var parameterType = parametersObject.GetStringPropertyValue("type");
            var optional = parametersObject.GetValue("optional").Value<bool>();
            var parameterDeclaration = new ParameterDeclaration(parameterName, parameterDescription, parameterLabel, parameterType, optional);
            return parameterDeclaration;
        }
    }
}