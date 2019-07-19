using System.Collections.Generic;
using System.Linq;

namespace ShapeFlow.Shapes
{
    public class FileSetShape : Shape
    {
        public FileSetShape(string name) : this(name, Enumerable.Empty<string>())
        {
        }

        public FileSetShape(string name, IEnumerable<string> tags) : base(ShapeFormat.FileSet, name, tags)
        {
            FileSet = new FileSet();
        }

        public FileSet FileSet { get; }

        public override object GetInstance()
        {
            return FileSet;
        }
    }
}