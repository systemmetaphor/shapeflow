using System.Collections.Generic;
using System.Linq;

namespace ShapeFlow.Loaders.DbModel
{
    public class EntityModelRoot
    {
        private readonly List<EntityModel> _entities;        

        public EntityModelRoot()
        {
            _entities = new List<EntityModel>();
        }

        public IEnumerable<EntityModel> Entities => _entities;
        
        public void AddModels(IEnumerable<EntityModel> businessObjects)
        {
            _entities.AddRange(businessObjects);            
        }               

        public bool HasModel(string name)
        {
            return _entities.Any(e => e.ObjectName.Equals(name));
        }
    }
}
