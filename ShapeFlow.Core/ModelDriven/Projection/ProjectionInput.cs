using System;
using System.Collections.Generic;
using ShapeFlow.Shapes;

namespace ShapeFlow
{
    public class ProjectionInput
    {
        public ProjectionInput(ShapeContext model)
        {
            ModelContext = model;            
        }

        public ShapeContext ModelContext { get; }

        public Shape Model => ModelContext.Model;

        public ShapeFormat Format => ModelContext.Model?.Format ?? ShapeFormat.None;

        public string ModelName => Model.Name;

        public IEnumerable<KeyValuePair<string, string>> Parameters => ModelContext.Declaration.Parameters;
    }
}
