using System.Collections.Generic;
using System.IO;
using ShapeFlow.Infrastructure;
using Newtonsoft.Json.Linq;

namespace ShapeFlow
{
    /// <summary>
    /// A generator converts an input model using a set of transformation rules into a set of output files.     
    /// This class holds the configuration required to set a new transformation.
    /// </summary>
    /// <remarks>
    /// The outputs represent what is expected on the end of the transformation process.
    /// </remarks>
    public class GeneratorDeclaration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransformationDeclaration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="modelType">Type of the domain model.</param>
        /// <param name="rules">The rules.</param>
        /// <param name="location">The location.</param>
        private  GeneratorDeclaration()
        {
        }

        /// <summary>
        /// Gets an user friendly name for the transformation.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the rules used to project the model into the desired output.
        /// </summary>
        /// <value>
        /// The rules used to project the model into the desired output..
        /// </value>
        public IEnumerable<TransformationRuleDeclaration> Rules
        {
            get;
            private set;
        }

        public IEnumerable<ParameterDeclaration> Parameters
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the location of the transformation.
        /// </summary>
        /// <value>
        /// The location of the transformation..
        /// </value>
        /// <remarks>
        /// This value will be used to locate the transformation templates and if applicable other transformation dependencies.
        /// </remarks>
        public string Location
        {
            get;
            private set;
        }

        public string Version
        {
            get;
            private set;
        }

        public static GeneratorDeclaration FromFile(string path)
        {
            var directory = Path.GetDirectoryName(path);

            if(string.IsNullOrWhiteSpace(directory))
            {
                directory = System.Environment.CurrentDirectory;
            }

            var text = File.ReadAllText(path);
            var jo = JObject.Parse(text);
            if (!jo.ContainsKey("location"))
            {
                jo.Add("location", directory);
            }

            var result = Parse(jo);
            return result;
        }

        public static GeneratorDeclaration Parse(JObject transformationObject, string transformationName = null)
        {
            // when its an inline decl it gets the name from the property holding the decl object
            transformationName = transformationName ?? transformationObject.GetStringPropertyValue("name");
            var version = transformationObject.GetStringPropertyValue("version");
            var parametersArray = transformationObject.GetValue("parameters") as JArray ?? new JArray();
            var parameters = new List<ParameterDeclaration>();
            foreach (JObject parametersObject in parametersArray)
            {
                var parameterDeclaration = ParameterDeclaration.Parse(parametersObject);
                parameters.Add(parameterDeclaration);
            }

            var rulesArray = transformationObject.GetValue("rules") as JArray ?? new JArray();
            var rules = new List<TransformationRuleDeclaration>();
            foreach (JObject ruleObject in rulesArray)
            {
                var ruleDeclaration = TransformationRuleDeclaration.Parse(ruleObject);
                rules.Add(ruleDeclaration);
            }

            var location = transformationObject.GetStringPropertyValue("location");

            var transformation = new GeneratorDeclaration();
            transformation.Name = transformationName;
            transformation.Parameters = parameters;
            transformation.Rules = rules;
            transformation.Location = location;
            transformation.Version = version;
            return transformation;
        }
    }
}
