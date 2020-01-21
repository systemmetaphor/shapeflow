using System.Collections.Generic;
using System.Linq;
using ShapeFlow.Shapes;

namespace ShapeFlow.Loaders.KriativityReflectedModel
{
    public class KriativityReflectedModelShape : Shape
    {
        public KriativityReflectedModelShape(KriativityReflectedModelRoot model, ShapeFormat format, string name) : this(model, format, name, Enumerable.Empty<string>())
        {
        }

        public KriativityReflectedModelShape(KriativityReflectedModelRoot model, ShapeFormat format, string name, IEnumerable<string> tags) : base(format, name, tags)
        {
            Root = model;
        }

        public KriativityReflectedModelRoot Root { get; }

        public override object GetInstance()
        {
            return Root;
        }
    }
}