using ShapeFlow.Infrastructure;
using Newtonsoft.Json.Linq;

namespace ShapeFlow
{
    public class InputDeclaration
    {
        public InputDeclaration(string selector)
        {
            Selector = selector;
        }

        public string Selector { get; }

        public static InputDeclaration Parse(JObject inputObject)
        {
            var selector = inputObject.GetStringPropertyValue("selector");
            return new InputDeclaration(selector);
        }
    }

}
