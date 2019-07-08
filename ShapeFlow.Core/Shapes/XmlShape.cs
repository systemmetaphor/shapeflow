using System.Collections.Generic;
using System.Xml.Linq;

namespace ShapeFlow.Shapes
{
    public class XmlShape : Shape
    {
        public XmlShape(XDocument root,  ShapeFormat format, string name, IEnumerable<string> tags) : base(format, name, tags)
        {
            Root = root;
        }

        public XDocument Root { get; }

        public override object GetInstance()
        {
            return Root;
        }
    }
}
