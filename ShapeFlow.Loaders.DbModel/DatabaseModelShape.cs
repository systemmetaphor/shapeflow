using System.Collections.Generic;
using ShapeFlow.Shapes;

namespace ShapeFlow.Loaders.DbModel
{
    public class DatabaseModelShape : Shape
    {
        public DatabaseModelShape(DatabaseModelRoot root, ShapeFormat format, string name, IEnumerable<string> tags) : base(format, name, tags)
        {
            Root = root;
        }

        public DatabaseModelRoot Root { get; }

        public override object GetInstance()
        {
            return Root;
        }
    }
}
