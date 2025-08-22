using System.Collections.Generic;
using System.Linq;

namespace ShapeFlow.Loaders.DbModel
{
    public class DatabaseModelRoot
    {
        private readonly List<TableModel> _entities;        

        public DatabaseModelRoot()
        {
            _entities = new List<TableModel>();
        }

        public IEnumerable<TableModel> Entities => _entities;
        
        public void AddModels(IEnumerable<TableModel> businessObjects)
        {
            _entities.AddRange(businessObjects);            
        }               

        public bool HasModel(string name)
        {
            return _entities.Any(e => e.ObjectName.Equals(name));
        }
    }
}
