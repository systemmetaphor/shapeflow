using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;
using ShapeFlow.Shapes;

namespace ShapeFlow.Loaders
{
    internal class JsonLoader : ILoader
    {        
                
        public const string ModelPathParameter = "model-path";

        public JsonLoader()
        {            
        }

        public string Name => "JsonLoader";

        public ShapeFormat Format => ShapeFormat.Json;

        public Task<ShapeContext> Load(ShapeDeclaration declaration)
        {            
            var modelFilePath = declaration.GetParameter(ModelPathParameter);
            var modelRoot = new ShapeContext(
                declaration, 
                new JsonShape(JObject.Parse(File.ReadAllText(modelFilePath)), ShapeFormat.Json, declaration.Name, declaration.Tags));
            return Task.FromResult(modelRoot);
        }

        public Task Save(ShapeContext context)
        {
            throw new NotImplementedException();
        }

        public ShapeContext Create(ShapeDeclaration decl)
        {
            throw new NotImplementedException();
        }

        public ShapeContext CreateSet(ShapeDeclaration decl)
        {
            throw new NotImplementedException();
        }

        public bool ValidateArguments(ShapeDeclaration context)
        {
            if(string.IsNullOrWhiteSpace(context.GetParameter(ModelPathParameter)))
            {
                // TODO: this should be a validation service
                AppTrace.Error($"The parameter {ModelPathParameter} is required.");
                return false;
            }

            return true;
        }
    }
}
