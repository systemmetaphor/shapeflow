﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ShapeFlow.Infrastructure;
using ShapeFlow.ModelDriven;
using ShapeFlow.ModelDriven.Loaders;
using ShapeFlow.ModelDriven.Models;
using YamlDotNet.RepresentationModel;

namespace ShapeFlow.Loaders.Yaml
{
    public class YamlModelLoader : IModelLoader
    {
        public const string ModelPathParameter = "model-path";

        private readonly ILoggingService _loggingService;


        public YamlModelLoader(ILoggingService loggingService)
        {
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        public string Name => "YamlLoader";

        public ModelFormat Format => throw new NotImplementedException();

        public ModelContext LoadModel(ModelDeclaration context)
        {
            var modelFilePath = context.GetParameter(ModelPathParameter);

            using (var reader = new StreamReader(modelFilePath))
            {
                var yamlStream = new YamlStream();
                yamlStream.Load(reader);

                var model = yamlStream.Documents.FirstOrDefault()?.RootNode;

                var modelContext = new ModelContext(context, new YamlModel(model, ModelFormat.Yaml, context.ModelName, context.Tags));

                return modelContext;
            }
        }

        public bool ValidateArguments(ModelDeclaration context)
        {
            if (string.IsNullOrWhiteSpace(context.GetParameter(ModelPathParameter)))
            {
                // TODO: this should be a validation service
                _loggingService.Error($"The parameter {ModelPathParameter} is required.");
                return false;
            }

            return true;
        }
    }

    public class YamlModel : Model
    {
        public YamlModel(YamlNode root, ModelFormat format, string name, IEnumerable<string> tags) : base(format, name, tags)
        {
            Root = root;
        }

        public YamlNode Root { get; }

        public override object GetModelInstance()
        {
            return Root;
        }
    }
}
