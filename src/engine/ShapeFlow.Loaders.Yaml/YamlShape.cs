using System.Collections.Generic;
using ShapeFlow.Shapes;
using YamlDotNet.RepresentationModel;

namespace ShapeFlow.Loaders.Yaml
{
    public class YamlShape : Shape
    {
        public YamlShape(YamlNode root, ShapeFormat format, string name, IEnumerable<string> tags) : base(format, name, tags)
        {
            Root = root;
        }

        public YamlNode Root { get; }

        public override object GetInstance()
        {
            return Root;
        }
    }
}