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

        public Solution Solution => Declaration.Solution;

        public ShapeDeclaration Declaration { get; }

        public Shape Model { get; }

        public IDictionary<string, string> Parameters => Solution.Parameters;

        public string GetParameter(string name)
        {
            return Declaration.GetParameter(name);
        }
    }
}
