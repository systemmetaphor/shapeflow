using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.ProjectManagement;

namespace ShapeFlow.PackageManagement.NuGet
{
    internal class CustomFolderNugetProject : FolderNuGetProject
    {
        public CustomFolderNugetProject(string root) : base(root)
        {
        }

        public CustomFolderNugetProject(string root, PackagePathResolver packagePathResolver) : base(root, packagePathResolver)
        {
        }

        public CustomFolderNugetProject(string root, PackagePathResolver packagePathResolver, NuGetFramework targetFramework) : base(root, packagePathResolver, targetFramework)
        {
        }
    }
}