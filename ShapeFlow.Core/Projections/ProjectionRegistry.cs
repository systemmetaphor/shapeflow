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

        public ProjectionRegistry(IExtensibilityService extensibilityService, PackageManagerFactory packageManagerFactory)
        {
            _packageManagerFactory = packageManagerFactory;
            _targets = new HashSet<TargetRegistration>();
            _extensibilityService = extensibilityService;

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

        public IProjectionExtension GetExtension(ProjectionDeclaration t)
        {
            return _targets.First(element => element.Configuration.Name == t.Name).Extension;
        }

        public async Task<SolutionEventContext> Process(SolutionEventContext ev)
        {
            // load packages
            var packageManager = _packageManagerFactory.Create(ev.Solution);

            // handle package declarations
            foreach (var generator in ev.Solution.Projections.Where(g => !g.IsInline))
            {
                var packageInfo = await packageManager.ResolvePackage(generator.PackageName, generator.PackageVersion);
                if (!string.IsNullOrWhiteSpace(packageInfo.Root))
                {
                    var packageMetadata = GetPackageMetadata(packageInfo);
                    if (packageMetadata != null)
                    {
                        packageMetadata.OverrideName(generator.Projection.Name);
                        Add(packageMetadata, packageInfo);
                    }
                }
            }

            // handle inline projection declarations
            foreach (var generator in ev.Solution.Projections.Where(g => g.IsInline))
            {
                Add(generator.Projection, string.Empty);
            }

            return ev;
        }

        private ProjectionDeclaration GetPackageMetadata(PackageInfo packageInfo)
        {
            // packageRootDirectory = GetInstalledPath

            var dir = new DirectoryPath(packageInfo.Root);
            var content = dir.Combine("Content");

            return  ProjectionDeclaration.FromPackageDirectory(packageInfo);
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
