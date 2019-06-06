using System;
using System.Composition;
using System.IO;
using Newtonsoft.Json.Linq;
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

        public ShapeContext Load(ShapeDeclaration declaration)
        {            
            var modelFilePath = declaration.GetParameter(ModelPathParameter);
            var modelRoot = new ShapeContext(
                declaration, 
                new JsonShape(JObject.Parse(File.ReadAllText(modelFilePath)), ShapeFormat.Json, declaration.ModelName, declaration.Tags));
            return modelRoot;
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
