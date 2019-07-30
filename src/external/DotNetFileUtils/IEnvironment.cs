using System;

namespace DotNetFileUtils
{
    public interface IEnvironment
    {
        DirectoryPath WorkingDirectory { get; }

        string ExpandEnvironmentVariables(string fullPath);
    }
}