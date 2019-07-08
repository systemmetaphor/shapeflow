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
using System.Threading;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.PackageManagement.NuGet
{
    public class ShapeFlowNugetPackageManager : PackageManager
    {
        private readonly ISettings _defaultSettings;
        private readonly string _globalPackagesFolder;
        private readonly PackageSourceProvider _packageSourceProvider;
        private readonly FolderNuGetProject _nugetProject;
        private readonly SourceRepositoryProvider _sourceRepositoryProvider;

        public ShapeFlowNugetPackageManager(Solution solution) : base(solution)
        {
            _defaultSettings = Settings.LoadDefaultSettings(SolutionRootDirectory, null, new MachineWideSettings());
            _globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(_defaultSettings);
            _packageSourceProvider = new PackageSourceProvider(_defaultSettings);
            _nugetProject = new FolderNuGetProject(_globalPackagesFolder);
            _sourceRepositoryProvider = new SourceRepositoryProvider(_defaultSettings);
        }

        public override Task<PackageInfo> GetPackageAsync(string packageName, string packageVersion)
        {
            var identity = new PackageIdentity(packageName, new NuGetVersion(packageVersion));
            var path = _nugetProject.GetInstalledPath(identity);
            return Task.FromResult(new PackageInfo(packageName, packageVersion, path));
        }

        public override async Task<PackageInfo> TryInstallPackage(string packageName, string packageVersion)
        {
            var identity = new PackageIdentity(packageName, new NuGetVersion(packageVersion));
            NuGetPackageManager nugetPackageManager = new NuGetPackageManager(_sourceRepositoryProvider, _defaultSettings, _globalPackagesFolder);
            var resolutionContext = new ResolutionContext();
            var projectContext = new NuGetProjectContext();
            await nugetPackageManager.InstallPackageAsync(
                _nugetProject,
                identity,
                resolutionContext,
                projectContext,
                _sourceRepositoryProvider.GetDefaultRepositories(),
                        Array.Empty<SourceRepository>(),
                        CancellationToken.None);

            var path = _nugetProject.GetInstalledPath(identity);

            return new PackageInfo(packageName, packageVersion, path);
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
        public void AddDefaultRepository(string packageSource) => _defaultRepositories.Insert(0, CreateRepository(packageSource));

        public IReadOnlyList<SourceRepository> GetDefaultRepositories() => _defaultRepositories;

        /// <summary>
        /// Creates or gets a non-default source repository.
        /// </summary>
        public SourceRepository CreateRepository(string packageSource) => CreateRepository(new PackageSource(packageSource), FeedType.Undefined);

        /// <summary>
        /// Creates or gets a non-default source repository by PackageSource.
        /// </summary>
        public SourceRepository CreateRepository(PackageSource packageSource) => CreateRepository(packageSource, FeedType.Undefined);

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
        }

        public NuGetActionType ActionType { get; set; }

        public Guid OperationId { get; set; }
    }
}
