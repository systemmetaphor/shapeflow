using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShapeFlow.Infrastructure
{
    internal class ExtensibilityService : IExtensibilityService
    {
        private readonly IContainer _container;

        public ExtensibilityService(IContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }   

        public IEnumerable<T> LoadExtensions<T>()
            where T : class
        {            
            var extensions = _container.ResolveAll<T>();

            foreach(var extension in extensions)
            {
                if(extension is IInitializable initializable)
                {
                    initializable.Initialize();
                }
            }

            return extensions;
        }                
    }
}
