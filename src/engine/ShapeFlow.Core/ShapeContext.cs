using System.Collections.Generic;
using ShapeFlow.Declaration;
using ShapeFlow.Shapes;

namespace ShapeFlow
{
    public class ShapeContext
    {
        public ShapeContext(ShapeDeclaration declaration, Shape shape)
        {
            Declaration = declaration;
            Shape = shape;
        }

        public ShapeDeclaration Declaration { get; }

        public Shape Shape { get; private set; }

        public bool IsLoaded => Shape != null;

        internal void SetShape(Shape s)
        {
            Shape = s;
        }
    }
}
