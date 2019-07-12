using ShapeFlow.Declaration;

namespace ShapeFlow.Projections
{
    public interface IProjectionExtension
    {       
        ProjectionDeclaration Declaration { get; }

        string Location { get; }
    }
}
