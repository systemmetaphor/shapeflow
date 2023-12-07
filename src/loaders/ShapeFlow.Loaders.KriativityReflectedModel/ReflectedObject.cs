using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;

namespace ShapeFlow.Loaders.KriativityReflectedModel
{
    public class ReflectedObject
    {
        private readonly HashSet<ReflectedObjectProperty> _properties;     

        public ReflectedObject()
        {
            _properties = new HashSet<ReflectedObjectProperty>();
        }
        
        public string Name { get; set; }
        
        public string Namespace { get; set; }

        public IEnumerable<ReflectedObjectProperty> Properties  => _properties;
                
        public string Type { get; set; }
        
        public bool IsObsolete { get; set; }

        public string BaseType { get; set; }

        public bool HasBaseEventObject { get; set; }

        public string DtoName { get; internal set; }

        public void AddProperty(ReflectedObjectProperty property)
        {
            _properties.Add(property);
        }

        public ReflectedObjectProperty GetProperty(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return _properties.FirstOrDefault(p => name.Equals(p.Name));
        }
    }
}
