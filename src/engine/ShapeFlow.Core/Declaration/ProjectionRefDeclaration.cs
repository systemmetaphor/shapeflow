using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
{
    /// <summary>
    /// A projectionRef applies a projection to an input model and optionally overrides its conventions.
    /// </summary>
    public class ProjectionRefDeclaration
    {
        private ProjectionRefDeclaration()
        {

        }

        public string ProjectionName
        {
            get;
            private set;
        }       

        public static ProjectionRefDeclaration Parse(JObject transformationObject)
        {
            var generator = transformationObject.GetStringPropertyValue("projection");
                       
            var result = new ProjectionRefDeclaration
            {
                ProjectionName = generator
            };

            return result;
        }
    }
}
