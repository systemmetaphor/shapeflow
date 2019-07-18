using System;
using System.Collections.Generic;
using System.Linq;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Projections
{
    public class RuleLanguageProvider
    {
        private readonly IExtensibilityService _extensibilityService;        
        private readonly HashSet<IProjectionRuleEngine> _engines;

        public RuleLanguageProvider(IExtensibilityService extensibilityService)
        {
            _extensibilityService = extensibilityService ?? throw new ArgumentNullException(nameof(extensibilityService));            
            _engines = new HashSet<IProjectionRuleEngine>();
            Load();
        }

        public IEnumerable<string> RuleSearchExpressions
        {
            get
            {
                return _engines.Select(e => e.RuleSearchExpression).Distinct().ToArray();
            }
        }

        public IProjectionRuleEngine GetEngine(string language)
        {            
            var engine = _engines.Where(p => language.Equals(p.RuleLanguage, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
            return engine;
        }

        private void Load()
        {
            var targetExtensions = _extensibilityService.LoadExtensions<IProjectionRuleEngine>();

            foreach (var extension in targetExtensions)
            {
                _engines.Add(extension);
            }
        }
    }
}
