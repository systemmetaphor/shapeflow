using System.Collections.Generic;
using ShapeFlow.Models;

namespace ShapeFlow
{
    public class ModelContext
    {
        public ModelContext(ModelDeclaration declaration, Model model)
        {
            Declaration = declaration;
            Model = model;
        }

        public Solution Solution => Declaration.Solution;

        public ModelDeclaration Declaration { get; }

        public Model Model { get; }

        public IDictionary<string, string> Parameters => Solution.Parameters;

        public string GetParameter(string name)
        {
            return Declaration.GetParameter(name);
        }
    }
}
