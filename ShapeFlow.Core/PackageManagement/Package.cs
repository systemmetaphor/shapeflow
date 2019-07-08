using ShapeFlow.Declaration;

namespace ShapeFlow.PackageManagement
{
    public class Package
    {
        public Package(string id, string version, MetadataPart metadataPart)
        {
            Id = id;
            Version = version;
            Metadata = metadataPart;
        }

        public string Id { get; }

        // we are using string to try to be as agnostic as possible
        // to the format of the physical payload (nuget, npm)
        public string Version { get; }
        
        public MetadataPart Metadata { get; }

    }
}
