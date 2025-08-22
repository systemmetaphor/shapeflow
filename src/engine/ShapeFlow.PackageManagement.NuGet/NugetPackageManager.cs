using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.PackageManagement.NuGet
{
    public class ShapeFlowNugetPackageManager : PackageManager
    {
        private readonly string _globalPackagesFolder;
        private readonly string _localPackagesFolder;
        private readonly CustomFolderNugetProject _nugetProject;
        private readonly SourceRepositoryProvider _sourceRepositoryProvider;
        private readonly NuGetPackageManager _nugetPackageManager;

        public ShapeFlowNugetPackageManager(SolutionDeclaration solutionDeclaration) : base(solutionDeclaration)
        {
            List<Lazy<INuGetResourceProvider>> providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());

            //PackageSource packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
            //_sourceRepository = new SourceRepository(packageSource, providers);

            var defaultSettings = Settings.LoadDefaultSettings(SolutionRootDirectory, null, new MachineWideSettings());

            
            _localPackagesFolder = Path.Combine(SolutionRootDirectory, ".shapeflow");
            _globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(defaultSettings);

            _nugetProject = new CustomFolderNugetProject(_localPackagesFolder);
            var packageSourceProvider = new PackageSourceProvider(defaultSettings);
            
            _sourceRepositoryProvider = new SourceRepositoryProvider(packageSourceProvider, providers);
            _nugetPackageManager = new NuGetPackageManager(_sourceRepositoryProvider, defaultSettings, _globalPackagesFolder);
        }

        public string LocalPackagesFolder => _localPackagesFolder;

        public string GlobalPackagesFolder => _globalPackagesFolder;

        public override Task<PackageInfo> GetPackageAsync(string packageName, string packageVersion)
        {
            var identity = new PackageIdentity(packageName, new NuGetVersion(packageVersion));
            var path = _nugetProject.GetInstalledPath(identity);
            var filePath = _nugetProject.GetInstalledPackageFilePath(identity);

            var result = new PackageInfo(packageName, packageVersion, path, filePath);
            if (result.IsInstalled)
            {
                PopulateContents(result);
            }

            return Task.FromResult(result);
        }

        public override async Task<PackageInfo> TryInstallPackage(string packageName, string packageVersion)
        {   
            var identity = new PackageIdentity(packageName, new NuGetVersion(packageVersion));
            
            var resolutionContext = new ResolutionContext(DependencyBehavior.Ignore, false, true, VersionConstraints.None);
            var projectContext = new NuGetProjectContext();

            var repositories = _sourceRepositoryProvider.GetRepositories();
            
            await _nugetPackageManager.InstallPackageAsync(
                _nugetProject,
                identity,
                resolutionContext,
                projectContext,
                repositories,
                Array.Empty<SourceRepository>(),
                CancellationToken.None);

            var path = _nugetProject.GetInstalledPath(identity);
            var filePath = _nugetProject.GetInstalledPackageFilePath(identity);

            var result = new PackageInfo(packageName, packageVersion, path, filePath);
            PopulateContents(result);
            return result;
        }

        private void PopulateContents(PackageInfo result)
        {
            var archiveReader = new PackageArchiveReader(result.PackageFilePath);
            var referenceItems = archiveReader.GetReferenceItems().ToList();
            var currentFramework = GetCurrentFramework();
            var referenceGroup = GetMostCompatibleGroup(currentFramework, referenceItems);

            if (referenceGroup != null)
            {
                AppTrace.Verbose($"Found compatible reference group {referenceGroup.TargetFramework.DotNetFrameworkName} for package {result.Name}");
                foreach (string itemPath in referenceGroup.Items
                    .Where(x => Path.GetExtension(x) == ".dll" || Path.GetExtension(x) == ".exe"))
                {
                    var assemblyPath = Path.Combine(result.Root, itemPath);
                    result.AddAssembly(assemblyPath);
                    AppTrace.Verbose($"Added NuGet reference {assemblyPath} from package {result.Name} for loading");
                }
            }
            else if (referenceItems.Count == 0)
            {
                AppTrace.Verbose($"Could not find any reference items in package {result.Name}");
            }
            else
            {
                AppTrace.Verbose($"Could not find compatible reference group for package {result.Name} (found {string.Join(",", referenceItems.Select(x => x.TargetFramework.DotNetFrameworkName))})");
            }


            var contentGroup = GetMostCompatibleGroup(currentFramework, archiveReader.GetContentItems().ToList());
            if (contentGroup != null)
            {
                foreach (var contentSegment in contentGroup.Items.Distinct())
                {
                    var contentPath = Path.Combine(result.Root, contentSegment);
                    AppTrace.Verbose($"Added content path {contentPath} from compatible content group {contentGroup.TargetFramework.DotNetFrameworkName} from package {result.Name} to included paths");
                    result.AddContentPath(contentPath);
                }
            }
        }

        private NuGetFramework GetCurrentFramework()
        {
            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            string frameworkName = assembly.GetCustomAttributes(true)
                .OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
                .Select(x => x.FrameworkName)
                .FirstOrDefault();
            return frameworkName == null
                ? NuGetFramework.AnyFramework
                : NuGetFramework.ParseFrameworkName(frameworkName, new DefaultFrameworkNameProvider());
        }

        private static FrameworkSpecificGroup GetMostCompatibleGroup(
            NuGetFramework projectTargetFramework,
            IEnumerable<FrameworkSpecificGroup> itemGroups)
        {
            var reducer = new FrameworkReducer();
            var mostCompatibleFramework = reducer.GetNearest(projectTargetFramework, itemGroups.Select(i => i.TargetFramework));
            if (mostCompatibleFramework != null)
            {
                var mostCompatibleGroup = itemGroups.FirstOrDefault(i => i.TargetFramework.Equals(mostCompatibleFramework));

                if (IsValid(mostCompatibleGroup))
                {
                    return mostCompatibleGroup;
                }
            }

            return null;
        }

        private static bool IsValid(FrameworkSpecificGroup frameworkSpecificGroup)
        {
            if (frameworkSpecificGroup != null)
            {
                return frameworkSpecificGroup.HasEmptyFolder
                       || frameworkSpecificGroup.Items.Any()
                       || !frameworkSpecificGroup.TargetFramework.Equals(NuGetFramework.AnyFramework);
            }

            return false;
        }
    }
}
