using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShapeFlow.Infrastructure;
using ShapeFlow.ModelDriven.Loaders;

namespace ShapeFlow.ModelDriven.Models
{
    internal class ModelManager : IModelManager
    {
        private readonly Dictionary<string, ModelContext> _models;
        private readonly LoaderRegistry _loaderRegistry;

        public ModelManager(IExtensibilityService extensibilityService)
        {
            _models = new Dictionary<string, ModelContext>();
            _loaderRegistry = new LoaderRegistry(extensibilityService);
        }

        public bool Validate(ModelDeclaration context)
        {
            if(_loaderRegistry.TryGet(context.LoaderName, out IModelLoader loader))
            {
                return loader.ValidateArguments(context);
            }

            return false;
        }

        public ModelContext GetOrLoad(ModelDeclaration declaration) 
        {
            ModelContext modelRoot = Get(declaration.ModelName);
            if(modelRoot != null)
            {
                return modelRoot;
            }

            if (_loaderRegistry.TryGet(declaration.LoaderName, out IModelLoader loader))
            {
                // load the domain model                                              
                modelRoot = loader?.LoadModel(declaration) ??
                    throw new InvalidOperationException($"It was not possible to load a model provider for '{ declaration.LoaderName }'. Check the transformation configuration to see if there is one and just one provider for '{ declaration.LoaderName }'.");

                _models.Add(declaration.ModelName, modelRoot);
            }

            return modelRoot;
        }

        public ModelContext Get(string modelName)
        {
            if (_models.ContainsKey(modelName))
            {
                return _models[modelName];
            }

            return null;
        }

        public SolutionEventContext Process(SolutionEventContext ev)
        {
            foreach (var modelDecl in ev.Solution.Models)
            {
                GetOrLoad(modelDecl);
            }

            return ev;
        }
    }
}
