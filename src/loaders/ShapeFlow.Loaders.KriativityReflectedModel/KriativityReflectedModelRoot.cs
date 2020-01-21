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

        
        public KriativityReflectedModelRoot()
        {
            _businessObjects = new List<ReflectedObject>();
            _eventObjects = new List<ReflectedObject>();
            _stateProjections = new List<StateProjection>();
        }

        public IEnumerable<ReflectedObject> BusinessObjects => _businessObjects;

        public IEnumerable<ReflectedObject> EventObjects => _eventObjects;

        public IEnumerable<StateProjection> StateProjections => _stateProjections;

        public IEnumerable<string> StateProjectionsNamespaces
        {
            get
            {
                var objs = StateProjections.Select(p => p.BusinessObject).Concat(StateProjections.Select(p => p.StateObject));
                var nss = objs.Select(o => o.DotNetNamespace);
                return nss.Distinct().OrderBy(n => n).ToArray();
            }
        }

        public void AddModels(IEnumerable<ReflectedObject> businessObjects, IEnumerable<ReflectedObject> eventObjects, IEnumerable<StateProjection> stateProjections)
        {
            _businessObjects.AddRange(ModelSorter.Sort(businessObjects));
            _eventObjects.AddRange(ModelSorter.Sort(eventObjects));
            _stateProjections.AddRange(stateProjections);
        }

        public bool HasEventObject(string name)
        {
            return _eventObjects.Any(e => e.Name.Equals(name));
        }

        public bool HasBusinessbject(string name)
        {
            return _businessObjects.Any(e => e.Name.Equals(name));
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
