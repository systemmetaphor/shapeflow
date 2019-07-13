using System.Threading.Tasks;
using ShapeFlow.Declaration;

namespace ShapeFlow.PackageManagement
{
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

        public async Task<PackageInfo> ResolvePackage(string packageName, string packageVersion)
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

            return info;
        }
    }
}
