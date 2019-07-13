using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
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
            ProjectionName = generatorName;            
        }

        public string Name
        {
            get;
            private set;
        }

        public string ProjectionName
        {
            get;
            private set;
        }       

        public static TransformationDeclaration Parse(JObject transformationObject)
        {
            var name = transformationObject.GetStringPropertyValue("name");
            var generator = transformationObject.GetStringPropertyValue("projection");
                       
            var result = new TransformationDeclaration
            {
                Name = name,
                ProjectionName = generator
            };

            return result;
        }
    }
}
