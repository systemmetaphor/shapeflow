using ShapeFlow.Declaration;

namespace ShapeFlow.PackageManagement.NuGet
{
    public class NugetPackageManagerFactory : PackageManagerFactory
    {
        public override PackageManager Create(Solution solution)
        {
            return new ShapeFlowNugetPackageManager(solution);
        }
    }
}