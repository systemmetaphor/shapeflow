using System.IO;
using ShapeFlow.Declaration;
using ShapeFlow.Shapes;

namespace ShapeFlow.Loaders
{
    public interface ILoader
    {
        string Name { get; }

        ShapeFormat Format { get; }

        ShapeContext Load(ShapeDeclaration context);

        bool ValidateArguments(ShapeDeclaration context);
    }
}
