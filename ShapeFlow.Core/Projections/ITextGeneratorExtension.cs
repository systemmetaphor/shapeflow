using ShapeFlow.Declaration;

namespace ShapeFlow.Projections
{
    public interface ITextGeneratorExtension
    {       
        ProjectionDeclaration Declaration { get; }

        string Location { get; }
    }
}
