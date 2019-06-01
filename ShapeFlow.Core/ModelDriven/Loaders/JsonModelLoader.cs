using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;
using ShapeFlow.Models;

namespace ShapeFlow.Loaders
{
    internal class JsonModelLoader : IModelLoader
    {        
                
        public const string ModelPathParameter = "model-path";

        public JsonModelLoader()
        {            
        }

        public string Name => "JsonLoader";

        public ModelFormat Format => ModelFormat.Json;

        public ModelContext LoadModel(ModelDeclaration declaration)
        {            
            var modelFilePath = declaration.GetParameter(ModelPathParameter);
            var modelRoot = new ModelContext(
                declaration, 
                new JsonModel(JObject.Parse(File.ReadAllText(modelFilePath)), ModelFormat.Json, declaration.ModelName, declaration.Tags));
            return modelRoot;
        }

        public bool ValidateArguments(ModelDeclaration context)
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

    public class JsonModel : Model
    {
        public JsonModel(JObject root, ModelFormat format, string name, IEnumerable<string> tags) : base(format, name, tags)
        {
            Root = root;
        }

        public JObject Root { get; }

        public override object GetModelInstance()
        {
            return Root;
        }
    }
}
