using System;
using System.Collections.Generic;
using ShapeFlow.ModelDriven.Models;

namespace ShapeFlow.ModelDriven
{
    public class ProjectionInput
    {
        public ProjectionInput(ModelContext model)
        {
            ModelContext = model;            
        }

        public ModelContext ModelContext { get; }

        public Model Model => ModelContext.Model;

        public ModelFormat Format => ModelContext.Model?.Format ?? ModelFormat.None;

        public string ModelName => Model.Name;

        public IEnumerable<KeyValuePair<string, string>> Parameters => ModelContext.Declaration.Parameters;
    }
}
