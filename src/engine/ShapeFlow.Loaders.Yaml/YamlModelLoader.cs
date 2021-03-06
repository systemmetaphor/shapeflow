﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeFlow.Infrastructure;
using ShapeFlow;
using ShapeFlow.Declaration;
using ShapeFlow.Loaders;
using ShapeFlow.Shapes;
using YamlDotNet.RepresentationModel;

namespace ShapeFlow.Loaders.Yaml
{
    public class YamlModelLoader : ILoader
    {
        public const string ModelPathParameter = "model-path";

        public YamlModelLoader()
        {            
        }

        public string Name => "YamlLoader";

        public ShapeFormat Format => throw new NotImplementedException();

        public Task<ShapeContext> Load(ShapeDeclaration context)
        {
            var modelFilePath = context.GetParameter(ModelPathParameter);

            using (var reader = new StreamReader(modelFilePath))
            {
                var yamlStream = new YamlStream();
                yamlStream.Load(reader);

                var model = yamlStream.Documents.FirstOrDefault()?.RootNode;

                var modelContext = new ShapeContext(context, new YamlShape(model, ShapeFormat.Yaml, context.Name, context.Tags));

                return Task.FromResult(modelContext);
            }
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
            if (string.IsNullOrWhiteSpace(context.GetParameter(ModelPathParameter)))
            {
                // TODO: this should be a validation service
                AppTrace.Error($"The parameter {ModelPathParameter} is required.");
                return false;
            }

            return true;
        }
    }
}
