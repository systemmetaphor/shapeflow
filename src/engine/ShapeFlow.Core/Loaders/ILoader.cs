using System.IO;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
using ShapeFlow.Shapes;

namespace ShapeFlow.Loaders
{
    public interface ILoader
    {
        string Name { get; }

        ShapeFormat Format { get; }

        Task<ShapeContext> Load(ShapeDeclaration context);

        Task Save(ShapeContext context);

        ShapeContext Create(ShapeDeclaration decl);

        bool ValidateArguments(ShapeDeclaration context);
    }
}
