using NuGet.Configuration;
using NuGet.ProjectManagement;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using NuGet.PackageManagement;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NuGet.Protocol;
using System.Linq;
using NuGet.Common;
using NuGet.Packaging;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using NuGet.Frameworks;
using NuGet.Packaging.PackageExtraction;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.PackageManagement.NuGet
{
    public class ShapeFlowNugetPackageManager : PackageManager
    {
        private readonly ISettings _defaultSettings;
        private readonly string _globalPackagesFolder;
        private readonly string _localPackagesFolder;
        private readonly PackageSourceProvider _packageSourceProvider;
        private readonly FolderNuGetProject _nugetProject;
        private readonly SourceRepositoryProvider _sourceRepositoryProvider;

        public ShapeFlowNugetPackageManager(Solution solution) : base(solution)
        {
            _defaultSettings = Settings.LoadDefaultSettings(SolutionRootDirectory, null, new MachineWideSettings());
            _localPackagesFolder = Path.Combine(SolutionRootDirectory, ".shapeflow");
            _globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(_defaultSettings);
            _packageSourceProvider = new PackageSourceProvider(_defaultSettings);
            _nugetProject = new FolderNuGetProject(_localPackagesFolder);
            _sourceRepositoryProvider = new SourceRepositoryProvider(_defaultSettings);
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
            _sourceRepositoryProvider.AddGlobalDefaults();

            var identity = new PackageIdentity(packageName, new NuGetVersion(packageVersion));
            NuGetPackageManager nugetPackageManager =
                new NuGetPackageManager(_sourceRepositoryProvider, _defaultSettings, _globalPackagesFolder);
            var resolutionContext = new ResolutionContext();
            var projectContext = new NuGetProjectContext();
            
            var repositories = _sourceRepositoryProvider.GetDefaultRepositories();
            
            await nugetPackageManager.InstallPackageAsync(
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
            FrameworkReducer reducer = new FrameworkReducer();
            NuGetFramework mostCompatibleFramework
                = reducer.GetNearest(projectTargetFramework, itemGroups.Select(i => i.TargetFramework));
            if (mostCompatibleFramework != null)
            {
                FrameworkSpecificGroup mostCompatibleGroup
                    = itemGroups.FirstOrDefault(i => i.TargetFramework.Equals(mostCompatibleFramework));

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

    internal class MachineWideSettings : IMachineWideSettings
    {
        private readonly Lazy<ISettings> _settings;

        public MachineWideSettings()
        {
            string baseDirectory = NuGetEnvironment.GetFolderPath(NuGetFolderPath.MachineWideConfigDirectory);
            _settings = new Lazy<ISettings>(
                () => global::NuGet.Configuration.Settings.LoadMachineWideSettings(baseDirectory));
        }

        public ISettings Settings => _settings.Value;
    }

    internal class SourceRepositoryProvider : ISourceRepositoryProvider
    {
        private static readonly string[] DefaultSources =
        {
            "https://api.nuget.org/v3/index.json"
        };

        private readonly List<SourceRepository> _defaultRepositories = new List<SourceRepository>();

        private readonly ConcurrentDictionary<PackageSource, SourceRepository> _repositoryCache
            = new ConcurrentDictionary<PackageSource, SourceRepository>();

        private readonly List<Lazy<INuGetResourceProvider>> _resourceProviders;

        public SourceRepositoryProvider(ISettings settings)
        {
            // Create the package source provider (needed primarily to get default sources)
            PackageSourceProvider = new PackageSourceProvider(settings);

            // Add the v3 provider as default
            _resourceProviders = new List<Lazy<INuGetResourceProvider>>();
            _resourceProviders.AddRange(Repository.Provider.GetCoreV3());
        }

        /// <summary>
        /// Add the global sources to the default repositories.
        /// </summary>
        public void AddGlobalDefaults()
        {
            _defaultRepositories.AddRange(PackageSourceProvider.LoadPackageSources()
                .Where(x => x.IsEnabled)
                .Select(x => new SourceRepository(x, _resourceProviders)));
        }

        public void AddDefaultPackageSources()
        {
            foreach (string defaultSource in DefaultSources)
            {
                AddDefaultRepository(defaultSource);
            }
        }

        /// <summary>
        /// Adds a default source repository to the front of the list.
        /// </summary>
        public void AddDefaultRepository(string packageSource) =>
            _defaultRepositories.Insert(0, CreateRepository(packageSource));

        public IReadOnlyList<SourceRepository> GetDefaultRepositories() => _defaultRepositories;

        /// <summary>
        /// Creates or gets a non-default source repository.
        /// </summary>
        public SourceRepository CreateRepository(string packageSource) =>
            CreateRepository(new PackageSource(packageSource), FeedType.Undefined);

        /// <summary>
        /// Creates or gets a non-default source repository by PackageSource.
        /// </summary>
        public SourceRepository CreateRepository(PackageSource packageSource) =>
            CreateRepository(packageSource, FeedType.Undefined);

        /// <summary>
        /// Creates or gets a non-default source repository by PackageSource.
        /// </summary>
        public SourceRepository CreateRepository(PackageSource packageSource, FeedType feedType) =>
            _repositoryCache.GetOrAdd(packageSource, x => new SourceRepository(packageSource, _resourceProviders));

        /// <summary>
        /// Gets all cached repositories.
        /// </summary>
        public IEnumerable<SourceRepository> GetRepositories() => _repositoryCache.Values;

        public IPackageSourceProvider PackageSourceProvider { get; }
    }

    internal class NuGetProjectContext : INuGetProjectContext
    {
        private Guid _operationId;

        public NuGetProjectContext()
        {
            PackageExtractionContext = new PackageExtractionContext(
                PackageSaveMode.Defaultv2,
                PackageExtractionBehavior.XmlDocFileSaveMode,
                clientPolicyContext: null,
                logger: NullLogger.Instance);
        }

        public void Log(MessageLevel level, string message, params object[] args)
        {
            switch (level)
            {
                case MessageLevel.Warning:
                    AppTrace.Warning(message, args);
                    break;
                case MessageLevel.Error:
                    AppTrace.Error(message, args);
                    break;
                default:
                    AppTrace.Verbose(message, args);
                    break;
            }
        }

        public FileConflictAction ResolveFileConflict(string message) => FileConflictAction.Ignore;

        public PackageExtractionContext PackageExtractionContext { get; set; }

        public XDocument OriginalPackagesConfig { get; set; }

        public ISourceControlManagerProvider SourceControlManagerProvider => null;

        public global::NuGet.ProjectManagement.ExecutionContext ExecutionContext => null;

        public void ReportError(string message)
        {
            AppTrace.Error(message);
        }

        public NuGetActionType ActionType { get; set; }

        public Guid OperationId
        {
            get
            {
                if (_operationId == Guid.Empty)
                {
                    _operationId = Guid.NewGuid();
                }

                return _operationId;
            }

            set { _operationId = value; }
        }
    }
}
