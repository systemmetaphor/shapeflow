using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShapeFlow.Infrastructure;
using ShapeFlow.ModelDriven;

namespace ShapeFlow.ModelDriven
{
    public class TextGeneratorRegistry
    {
        private readonly HashSet<TargetRegistration> _targets;
        private readonly IExtensibilityService _extensibilityService;

        public TextGeneratorRegistry(IExtensibilityService extensibilityService)
        {
            _targets = new HashSet<TargetRegistration>();
            _extensibilityService = extensibilityService;

            // TODO: move this to an initialization method
            Load();
        }

        public bool TryGet(string name, out GeneratorDeclaration generator)
        {
            generator = null;

            var tentative = _targets.FirstOrDefault(t => t.Configuration != null && t.Configuration.Name == name);
            if(tentative != null)
            {
                generator = tentative.Configuration;
                return true;
            }

            return false;
        }

        public void Add(GeneratorDeclaration declaration, string location)
        {
            _targets.Add(new TargetRegistration { Configuration = declaration, Location = location });
        }

        class TargetRegistration
        {
            public GeneratorDeclaration Configuration { get; set; }

            public ITextGeneratorExtension Extension { get; set; }

            public string Location { get; set; }
        }

        public ITextGeneratorExtension GetExtension(GeneratorDeclaration t)
        {
            return _targets.First(element => element.Configuration.Name == t.Name).Extension;
        }

        public SolutionEventContext Process(SolutionEventContext ev)
        {
            foreach (var generator in ev.Solution.Generators.Where(g => g.IsInline))
            {
                Add(generator.InlinDecl, string.Empty);
            }

            return ev;
        }

        private void Load()
        {   
            var targetExtensions = _extensibilityService.LoadExtensions<ITextGeneratorExtension>();

            foreach (var extension in targetExtensions)
            {
                _targets.Add(new TargetRegistration { Extension = extension, Configuration = extension.Declaration, Location = extension.Location });
            }            
        }
    }
}
