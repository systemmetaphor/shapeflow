using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ShapeFlow.Shapes
{
    public class JsonShape : Shape
    {
        public JsonShape(JObject root, ShapeFormat format, string name, IEnumerable<string> tags) : base(format, name, tags)
        {
            Root = root;
        }

        public JObject Root { get; }

        public override object GetInstance()
        {
            return Root;
        }
    }
}
