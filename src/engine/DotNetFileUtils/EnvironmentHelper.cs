namespace DotNetFileUtils
{
    public static class EnvironmentHelper
    {
        public static bool IsUnix()
        {
            return false;
        }

        public static PlatformFamily GetPlatformFamily()
        {
            return PlatformFamily.Windows;
        }
    }
}