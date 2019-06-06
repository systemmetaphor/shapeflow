using System.Collections.Generic;
using ShapeFlow.Shapes;

namespace ShapeFlow.Loaders.DbModel
{
    public class DbShape : Shape
    {
        public DbShape(EntityModelRoot root, ShapeFormat format, string name, IEnumerable<string> tags) : base(format, name, tags)
        {
            Root = root;
        }

        public EntityModelRoot Root { get; }

        public override object GetInstance()
        {
            return Root;
        }
    }
}
