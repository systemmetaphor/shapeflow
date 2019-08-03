using System;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
using ShapeFlow.Shapes;

namespace ShapeFlow.Loaders
{
    public class FileSetLoader : ILoader
    {
        public string Name { get; } = "FileSetLoader";

        public ShapeFormat Format { get; } = ShapeFormat.FileSet;

        public Task<ShapeContext> Load(ShapeDeclaration context)
        {
            throw new NotImplementedException();
        }

        public Task Save(ShapeContext context)
        {
            throw new NotImplementedException();
        }

        public ShapeContext Create(ShapeDeclaration decl)
        {
            var fileSetOutput = new FileSetShape(decl.Name);
            var c = new ShapeContext(decl, fileSetOutput);
            return c;
        }

        public bool ValidateArguments(ShapeDeclaration context)
        {
            throw new NotImplementedException();
        }
    }
}