using System.IO;

namespace ShapeFlow.ModelDriven.Models
{
    public interface IModelManager
    {
        SolutionEventContext Process(SolutionEventContext solution);

        ModelContext GetOrLoad(ModelDeclaration context);

        ModelContext Get(string selector);

        bool Validate(ModelDeclaration context);
    }
}