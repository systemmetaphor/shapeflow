using System;
using System.Collections.Generic;
using System.Linq;

namespace ShapeFlow.Loaders.DbModel
{
    public class TableModel
    {
        private List<ColumnModel> _properties;

        public TableModel()
        {
            _properties = new List<ColumnModel>();
        }

        public string ObjectSchema { get; set; }

        public string ObjectName { get; set; }

        public IEnumerable<ColumnModel> Properties => _properties.AsReadOnly();

        public void AddColumns(ColumnModel property)
        {
            _properties.Add(property);
        }

        public void AddColumn(IEnumerable<ColumnModel> properties)
        {
            foreach(var property in properties)
            {
                AddColumns(property);
            }
        }

        public ColumnModel GetColumn(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return _properties.FirstOrDefault(p => name.Equals(p.Name));
        }
    }
}