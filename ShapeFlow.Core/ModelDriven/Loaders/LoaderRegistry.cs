using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Loaders
{
    public class LoaderRegistry
    {
        private readonly HashSet<ILoader> _targets;
        private readonly IExtensibilityService _extensibilityService;

        public LoaderRegistry(IExtensibilityService extensibilityService)
        {
            _targets = new HashSet<ILoader>();
            _extensibilityService = extensibilityService;

            // TODO: move this to an initialization method
            Load();
        }

        public bool TryGet(string name, out ILoader generator)
        {
            generator = null;

            var tentative = _targets.FirstOrDefault(t => t.Name != null && t.Name == name);
            if (tentative != null)
            {
                generator = tentative;
                return true;
            }

            return false;
        }               
        
        private void Load()
        {
            var targetExtensions = _extensibilityService.LoadExtensions<ILoader>();

            foreach (var extension in targetExtensions)
            {
                _targets.Add(extension);
            }
        }
    }
}
