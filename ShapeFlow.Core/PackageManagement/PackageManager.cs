using System.Threading.Tasks;
using ShapeFlow.Declaration;

namespace ShapeFlow.PackageManagement
{
    public class PackageInfo
    {
        public PackageInfo(
            string packageName,
            string packageVersion,
            string packageRoot)
        {
            Name = packageName;
            Version = packageVersion;
            Root = packageRoot;
        }

        public string Name { get; }

        public string Version { get; }

        public string Root { get; }

        public bool IsInstalled => !string.IsNullOrWhiteSpace(Root);
    }

    public abstract class PackageManager
    {
        private readonly Solution _solution;

        protected PackageManager(Solution solution)
        {
            _solution = solution;
        }

        protected string SolutionRootDirectory => _solution.RootDirectory;

        public abstract Task<PackageInfo> GetPackageAsync(string packageName, string packageVersion);

        public abstract Task<PackageInfo> TryInstallPackage(string packageName, string packageVersion);

        public async Task<string> ResolvePackageRoot(string packageName, string packageVersion)
        {
            PackageInfo info = await GetPackageAsync(packageName, packageVersion);

            if (!info.IsInstalled)
            {
                info = await TryInstallPackage(packageName, packageVersion);
            }

            if (!info.IsInstalled)
            {
                return null;
            }

            return info.Root;
        }
    }
}
