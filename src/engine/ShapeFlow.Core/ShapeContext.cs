using System.Collections.Generic;
using ShapeFlow.Declaration;
using ShapeFlow.Shapes;

namespace ShapeFlow
{
    public class ShapeContext
    {
        public ShapeContext(ShapeDeclaration declaration, Shape model)
        {
            Declaration = declaration;
            Model = model;
        }

        public ShapeDeclaration Declaration { get; }

        public Shape Model { get; }
    }
}
