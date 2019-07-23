﻿using System;
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
                new JsonShape(JObject.Parse(File.ReadAllText(modelFilePath)), ShapeFormat.Json, declaration.ModelName, declaration.Tags));
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
            var fileSetOutput = new FileSetShape(decl.ModelName);
            var c = new ShapeContext(decl, fileSetOutput);
            return c;
        }

        public ShapeContext CreateSet(ShapeDeclaration decl)
        {
            throw new NotImplementedException();
        }

        public bool ValidateArguments(ShapeDeclaration context)
        {
            throw new NotImplementedException();
        }
    }
}
