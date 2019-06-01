using System.Collections.Generic;
using ShapeFlow.Infrastructure;
using ShapeFlow;

namespace ShapeFlow
{
    public interface ITextGeneratorExtension
    {       
        GeneratorDeclaration Declaration { get; }

        string Location { get; }
    }
}
