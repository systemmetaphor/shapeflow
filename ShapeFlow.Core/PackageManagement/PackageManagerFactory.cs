using ShapeFlow.Declaration;

namespace ShapeFlow.PackageManagement
{
    public abstract class PackageManagerFactory
    {
        public abstract  PackageManager Create(Solution solution);
    }
}