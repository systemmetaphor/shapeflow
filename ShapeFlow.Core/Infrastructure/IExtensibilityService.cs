using System.Collections.Generic;

namespace ShapeFlow.Infrastructure
{
    public interface IExtensibilityService
    {
        IEnumerable<T> LoadExtensions<T>() where T : class;
    }
}