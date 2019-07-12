using System.Collections.Generic;
using System.Net;

namespace ShapeFlow.PackageManagement
{
    public class PackageInfo
    {
        private readonly List<string> _assemblies;
        private readonly List<string> _contentPaths;

        public PackageInfo(
            string packageName,
            string packageVersion,
            string packageRoot,
            string packageFilePath)
        {
            Name = packageName;
            Version = packageVersion;
            Root = packageRoot;
            PackageFilePath = packageFilePath;
            _assemblies = new List<string>();
            _contentPaths = new List<string>();
        }

        public string Name { get; }

        public string Version { get; }

        public string Root { get; }

        public  string PackageFilePath { get; }

        public bool IsInstalled => !string.IsNullOrWhiteSpace(Root);

        public IEnumerable<string> Assemblies => _assemblies;

        public IEnumerable<string> ContentPaths => _contentPaths;

        public void AddAssembly(string assemblyPath)
        {
            _assemblies.Add(assemblyPath);
        }

        public void AddContentPath(string contentPath)
        {
            _contentPaths.Add(contentPath);
        }
    }
}