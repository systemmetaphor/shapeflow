using System;
using System.Collections.Generic;
using System.Linq;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Projections
{
    public class TemplateEngineProvider
    {
        private readonly IExtensibilityService _extensibilityService;        
        private readonly HashSet<ITextTemplateEngine> _engines;

        public TemplateEngineProvider(IExtensibilityService extensibilityService)
        {
            _extensibilityService = extensibilityService ?? throw new ArgumentNullException(nameof(extensibilityService));            
            _engines = new HashSet<ITextTemplateEngine>();
            Load();
        }

        public ITextTemplateEngine GetEngine(string language)
        {            
            var engine = _engines.Where(p => language.Equals(p.TemplateLanguage, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
            return engine;
        }

        private void Load()
        {
            var targetExtensions = _extensibilityService.LoadExtensions<ITextTemplateEngine>();

            foreach (var extension in targetExtensions)
            {
                _engines.Add(extension);
            }
        }
    }
}
