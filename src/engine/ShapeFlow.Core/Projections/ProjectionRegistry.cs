using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetFileUtils;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;
using ShapeFlow.PackageManagement;

namespace ShapeFlow.Projections
{
    public class ProjectionRegistry
    {
        private readonly HashSet<TargetRegistration> _targets;
        private readonly IExtensibilityService _extensibilityService;
        private readonly PackageManagerFactory _packageManagerFactory;
        private readonly RuleLanguageProvider _ruleLanguageProvider;

        public ProjectionRegistry(
            IExtensibilityService extensibilityService, 
            PackageManagerFactory packageManagerFactory,
            RuleLanguageProvider ruleLanguageProvider)
        {
            _packageManagerFactory = packageManagerFactory;
            _targets = new HashSet<TargetRegistration>();
            _extensibilityService = extensibilityService;
            _ruleLanguageProvider = ruleLanguageProvider;

            // TODO: move this to an initialization method
            Load();
        }

        public bool TryGet(string name, out ProjectionDeclaration generator)
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

        public void Add(ProjectionDeclaration declaration, string location)
        {
            _targets.Add(new TargetRegistration { Configuration = declaration, Location = location });
        }

        public void Add(ProjectionDeclaration declaration, PackageInfo package)
        {
            _targets.Add(new TargetRegistration { Configuration = declaration, Location = package.Root, PackageInfo = package });
        }

        class TargetRegistration
        {
            public ProjectionDeclaration Configuration { get; set; }

            public IProjectionExtension Extension { get; set; }

            public string Location { get; set; }

            public  PackageInfo PackageInfo { get; set; }
        }

        public async Task<SolutionDeclaration> Process(SolutionDeclaration solutionDeclaration)
        {
            // load packages
            var packageManager = _packageManagerFactory.Create(solutionDeclaration);

            // handle package declarations
            foreach (var generator in solutionDeclaration.Projections.Where(g => !g.IsInline))
            {
                var packageInfo = await packageManager.ResolvePackage(generator.PackageId, generator.Version);
                if (string.IsNullOrWhiteSpace(packageInfo.Root))
                {
                    continue;
                }

                var packageMetadata = AppendPackageMetadata(generator, packageInfo);
                if (packageMetadata != null)
                {
                    Add(packageMetadata, packageInfo);
                }
            }

            // handle inline projection declarations
            foreach (var generator in solutionDeclaration.Projections.Where(g => g.IsInline))
            {
                Add(generator, string.Empty);
            }

            return solutionDeclaration;
        }

        private ProjectionDeclaration AppendPackageMetadata(ProjectionDeclaration existingDeclaration, PackageInfo packageInfo)
        {
            var searchExpressions = _ruleLanguageProvider.RuleSearchExpressions;

            return  ProjectionDeclaration.LoadOrInferMetadata(existingDeclaration, packageInfo, searchExpressions);
        }

        private void Load()
        {   
            var targetExtensions = _extensibilityService.LoadExtensions<IProjectionExtension>();

            foreach (var extension in targetExtensions)
            {
                _targets.Add(new TargetRegistration { Extension = extension, Configuration = extension.Declaration, Location = extension.Location });
            }            
        }
    }
}
