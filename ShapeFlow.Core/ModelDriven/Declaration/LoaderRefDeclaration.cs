using System;

namespace ShapeFlow.ModelDriven
{
    public class LoaderRefDeclaration
    {
        public LoaderRefDeclaration(string packageName, string packageVersion)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                throw new ArgumentNullException(nameof(packageName));
            }

            if (string.IsNullOrWhiteSpace(packageVersion))
            {
                throw new ArgumentNullException(nameof(packageVersion));
            }

            PackageName = packageName;
            PackageVersion = packageVersion;
        }

        public string PackageName { get; }

        public string PackageVersion { get; }
    }
}