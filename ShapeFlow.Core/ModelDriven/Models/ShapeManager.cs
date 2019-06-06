using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShapeFlow.Infrastructure;
using ShapeFlow.Loaders;

namespace ShapeFlow.Shapes
{
    public class ShapeManager 
    {
        private readonly Dictionary<string, ShapeContext> _shapes;
        private readonly LoaderRegistry _loaderRegistry;

        public ShapeManager(IExtensibilityService extensibilityService)
        {
            _shapes = new Dictionary<string, ShapeContext>();
            _loaderRegistry = new LoaderRegistry(extensibilityService);
        }

        public bool Validate(ShapeDeclaration context)
        {
            if(_loaderRegistry.TryGet(context.LoaderName, out ILoader loader))
            {
                return loader.ValidateArguments(context);
            }

            return false;
        }

        public ShapeContext GetOrLoad(ShapeDeclaration declaration) 
        {
            ShapeContext modelRoot = Get(declaration.ModelName);
            if(modelRoot != null)
            {
                return modelRoot;
            }

            if (_loaderRegistry.TryGet(declaration.LoaderName, out ILoader loader))
            {
                // load the domain model                                              
                modelRoot = loader?.Load(declaration) ??
                    throw new InvalidOperationException($"It was not possible to load a model provider for '{ declaration.LoaderName }'. Check the transformation configuration to see if there is one and just one provider for '{ declaration.LoaderName }'.");

                _shapes.Add(declaration.ModelName, modelRoot);
            }

            return modelRoot;
        }

        public ShapeContext Get(string modelName)
        {
            if (_shapes.ContainsKey(modelName))
            {
                return _shapes[modelName];
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
