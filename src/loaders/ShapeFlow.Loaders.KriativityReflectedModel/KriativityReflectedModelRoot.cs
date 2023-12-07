using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeFlow.Loaders.KriativityReflectedModel
{

    public class KriativityReflectedModelRoot
    {
        private readonly List<ReflectedObject> _businessObjects;
        private readonly List<ReflectedObject> _eventObjects;
        private readonly List<StateProjection> _stateProjections;
        private readonly List<ReflectedObject> _dataObjects; 
        
        public KriativityReflectedModelRoot()
        {
            _businessObjects = new List<ReflectedObject>();
            _eventObjects = new List<ReflectedObject>();
            _stateProjections = new List<StateProjection>();
            _dataObjects = new List<ReflectedObject>();
        }

        public IEnumerable<ReflectedObject> BusinessObjects => _businessObjects;

        public IEnumerable<ReflectedObject> EventObjects => _eventObjects;

        public IEnumerable<StateProjection> StateProjections => _stateProjections;

        public IEnumerable<ReflectedObject> DataObjects => _dataObjects;

        public IEnumerable<string> StateProjectionsNamespaces
        {
            get
            {
                var objs = StateProjections.Select(p => p.BusinessObject).Concat(StateProjections.Select(p => p.StateObject));
                var nss = objs.Select(o => o.Namespace);
                return nss.Distinct().OrderBy(n => n).ToArray();
            }
        }

        public void AddModels(IEnumerable<ReflectedObject> businessObjects, IEnumerable<ReflectedObject> eventObjects, IEnumerable<StateProjection> stateProjections, IEnumerable<ReflectedObject> dataObjects)
        {
            _businessObjects.AddRange(ModelSorter.Sort(businessObjects));
            _eventObjects.AddRange(ModelSorter.Sort(eventObjects));
            _stateProjections.AddRange(stateProjections);
            _dataObjects.AddRange(dataObjects);
        }

        public bool HasEventObject(string name)
        {
            return _eventObjects.Any(e => e.Name.Equals(name));
        }

        public bool HasBusinessObject(string name)
        {
            return _businessObjects.Any(e => e.Name.Equals(name));
        }
        
        public bool HasDataObject(string name)
        {
            return _dataObjects.Any(e => e.Name.Equals(name));
        }

        public void RegisterDomainTypes(Action<Type, string[]> register)
        {
            register(typeof(ReflectedObject), typeof(ReflectedObject).GetProperties().Select(p => p.Name).ToArray());
            register(typeof(ReflectedObjectProperty), typeof(ReflectedObjectProperty).GetProperties().Select(p => p.Name).ToArray());

            register(typeof(StateProjection), typeof(StateProjection).GetProperties().Select(p => p.Name).ToArray());
            register(typeof(PropertyProjection), typeof(PropertyProjection).GetProperties().Select(p => p.Name).ToArray());
        }
    }
}
