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
