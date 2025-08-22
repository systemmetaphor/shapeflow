using ShapeFlow.Declaration;

namespace ShapeFlow.PackageManagement.NuGet
{
    public class NugetPackageManagerFactory : PackageManagerFactory
    {
        public override PackageManager Create(SolutionDeclaration solutionDeclaration)
        {
            return new ShapeFlowNugetPackageManager(solutionDeclaration);
        }
    }
}