namespace ShapeFlow.Declaration
{
    /// <summary>
    /// References a given projection, either a pre-packaged one or one that was declared in the project file.
    /// </summary>
    public class ProjectionRefDeclaration
    {
        public ProjectionRefDeclaration(ProjectionDeclaration decl)
        { 
            PackageName = decl.PackageId;
            PackageVersion = decl.Version;
            Declaration = decl;
        }

        /// <summary>
        /// Gets the package name.
        /// </summary>
        public string PackageName { get; }

        /// <summary>
        /// Gets the package version.
        /// </summary>
        public string PackageVersion { get; }

        /// <summary>
        /// Holds an inline declaration of a projection.
        /// </summary>
        public ProjectionDeclaration Declaration { get; }

        /// <summary>
        /// Gets a flag indicating if the referenced projection is an inline one.
        /// </summary>
        public bool IsInline => string.IsNullOrWhiteSpace(PackageName);
    }
}