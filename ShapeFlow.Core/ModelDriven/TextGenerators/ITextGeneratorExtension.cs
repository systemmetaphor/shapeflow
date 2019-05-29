using System.Collections.Generic;
using ShapeFlow.Infrastructure;
using ShapeFlow.ModelDriven;

namespace ShapeFlow.ModelDriven
{
    public interface ITextGeneratorExtension
    {       
        GeneratorDeclaration Declaration { get; }

        string Location { get; }
    }
}
