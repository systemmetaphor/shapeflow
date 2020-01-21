using System.Collections.Generic;

namespace ShapeFlow.Loaders.KriativityReflectedModel
{
    public class StateProjection
    {
        private HashSet<PropertyProjection> _propertyProjections;

        public StateProjection()
        {
            _propertyProjections = new HashSet<PropertyProjection>();
        }

        public string ProjectionName => BusinessObject?.DtoName.Replace("Data", "") ?? string.Empty;

        public ReflectedObject BusinessObject { get; set; }

        public ReflectedObject StateObject { get; set; }

        public IEnumerable<PropertyProjection> PropertyProjections => _propertyProjections;

        public void AddPropertyProjection(PropertyProjection projection)
        {
            _propertyProjections.Add(projection);
        }
    }
}
