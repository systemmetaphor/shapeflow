using System;

namespace ShapeFlow
{
    public class ModelLoaderDeclaration
    {        
        public ModelLoaderDeclaration(string packageName, string packageVersion)
        {
            if(string.IsNullOrWhiteSpace(packageName))
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