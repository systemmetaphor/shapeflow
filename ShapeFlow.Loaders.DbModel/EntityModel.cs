using System;
using System.Collections.Generic;
using System.Linq;

namespace ShapeFlow.Loaders.DbModel
{
    public class EntityModel
    {
        private List<PropertyModel> _properties;

        public EntityModel()
        {
            _properties = new List<PropertyModel>();
        }

        public string ObjectSchema { get; set; }

        public string ObjectName { get; set; }

        public IEnumerable<PropertyModel> Properties => _properties.AsReadOnly();

        public void AddProperty(PropertyModel property)
        {
            _properties.Add(property);
        }

        public void AddProperties(IEnumerable<PropertyModel> properties)
        {
            foreach(var property in properties)
            {
                AddProperty(property);
            }
        }

        public PropertyModel GetProperty(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return _properties.FirstOrDefault(p => name.Equals(p.PropertyName));
        }
    }
}