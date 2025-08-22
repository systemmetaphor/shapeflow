using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeFlow.Infrastructure;
using Unity.Injection;

namespace ShapeFlow.Loaders
{
    public class LoaderRegistry
    {
        private readonly Dictionary<string, ILoader> _loaders;
        private readonly IExtensibilityService _extensibilityService;

        public LoaderRegistry(IExtensibilityService extensibilityService)
        {
            _loaders = new Dictionary<string, ILoader>(StringComparer.OrdinalIgnoreCase);
            _extensibilityService = extensibilityService;

            // TODO: move this to an initialization method
            Load();
        }

        public bool TryGet(string name, out ILoader generator)
        {
            return _loaders.TryGetValue(name, out generator);
        }               
        
        private void Load()
        {
            var loaderExtensions = _extensibilityService.LoadExtensions<ILoader>();

            foreach (var extension in loaderExtensions)
            {
                _loaders.Add(extension.Name, extension);
            }
        }
    }
}
