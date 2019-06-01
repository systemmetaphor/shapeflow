using System;

namespace ShapeFlow
{
    public class GeneratorRefDeclaration
    {
        public GeneratorRefDeclaration(string packageName, string packageVersion)
            : this(packageName, packageVersion, null)
        {

        }

        public GeneratorRefDeclaration(string packageName, string packageVersion, GeneratorDeclaration inlineDecl)
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
            InlinDecl = inlineDecl;
        }

        public string PackageName { get; }

        public string PackageVersion { get; }

        public GeneratorDeclaration InlinDecl { get; }

        public bool IsInline => InlinDecl != null;
    }
}