using System.Threading.Tasks;
using ShapeFlow.Declaration;

namespace ShapeFlow.PackageManagement
{
    public abstract class PackageManager
    {
        private readonly SolutionDeclaration _solutionDeclaration;

        protected PackageManager(SolutionDeclaration solutionDeclaration)
        {
            _solutionDeclaration = solutionDeclaration;
        }

        protected string SolutionRootDirectory => _solutionDeclaration.RootDirectory;

        public abstract Task<PackageInfo> GetPackageAsync(string packageName, string packageVersion);

        public abstract Task<PackageInfo> TryInstallPackage(string packageFilePath);

        public abstract Task<PackageInfo> TryInstallPackage(string packageName, string packageVersion);

        public async Task<PackageInfo> ResolvePackage(string packageName, string packageVersion)
        {
            var info = await GetPackageAsync(packageName, packageVersion);

            if (!info.IsInstalled)
            {
                info = await TryInstallPackage(packageName, packageVersion);
            }

            return !info.IsInstalled ? null : info;
        }
    }
}
