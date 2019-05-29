using System.IO;
using ShapeFlow.ModelDriven.Models;

namespace ShapeFlow.ModelDriven.Loaders
{
    public interface IModelLoader
    {
        string Name { get; }

        ModelFormat Format { get; }

        ModelContext LoadModel(ModelDeclaration context);

        bool ValidateArguments(ModelDeclaration context);
    }
}
