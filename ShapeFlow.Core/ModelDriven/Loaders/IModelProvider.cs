using System.IO;
using ShapeFlow.Models;

namespace ShapeFlow.Loaders
{
    public interface IModelLoader
    {
        string Name { get; }

        ModelFormat Format { get; }

        ModelContext LoadModel(ModelDeclaration context);

        bool ValidateArguments(ModelDeclaration context);
    }
}
