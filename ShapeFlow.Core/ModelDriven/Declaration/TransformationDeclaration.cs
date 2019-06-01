using System;
using System.Linq;
using ShapeFlow.Infrastructure;
using Newtonsoft.Json.Linq;

namespace ShapeFlow
{
    /// <summary>
    /// A transformation applies a generator to an input model and optionally overrides its conventions.
    /// </summary>
    public class TransformationDeclaration
    {
        private TransformationDeclaration()
        {

        }

        public TransformationDeclaration(string name, string generatorName)
            : this()
        {
            Name = name;
            GeneratorName = generatorName;            
        }

        public string Name
        {
            get;
            private set;
        }

        public string GeneratorName
        {
            get;
            private set;
        }       

        public static TransformationDeclaration Parse(JObject transformationObject)
        {
            var name = transformationObject.GetStringPropertyValue("name");
            var generator = transformationObject.GetStringPropertyValue("generator");
            var inputObject = transformationObject.GetValue("input") as JObject;
           
            var result = new TransformationDeclaration
            {
                Name = name,
                GeneratorName = generator
            };

            return result;
        }
    }
}
