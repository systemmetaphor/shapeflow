using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
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

        public async Task<ShapeContext> GetOrLoad(ShapeDeclaration declaration) 
        {
            var modelRoot = Get(declaration.Name);
            if(modelRoot != null)
            {
                return modelRoot;
            }

            if (_loaderRegistry.TryGet(declaration.LoaderName, out var loader))
            {
                // load the domain model                                              
                modelRoot = await loader.Load(declaration);
                _shapes.Add(declaration.Name, modelRoot);
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
    }
}
