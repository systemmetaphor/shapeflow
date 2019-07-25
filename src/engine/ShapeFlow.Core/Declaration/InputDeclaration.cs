using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
{
    public class InputDeclaration
    {
        private InputDeclaration()
        {
        }

        public string Format { get; private set; }

        public string MetaModel { get; private set; }

        public static InputDeclaration Parse(JObject inputObject)
        {
            var type = inputObject.GetStringPropertyValue("format");
            var metaModel = inputObject.GetStringPropertyValue("type");
            return new InputDeclaration
            {
                Format = type,
                MetaModel = metaModel 
            };
        }
    }

}
