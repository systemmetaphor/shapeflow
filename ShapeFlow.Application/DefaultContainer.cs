using System;
using System.Collections.Generic;
using System.Linq;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace ShapeFlow.Infrastructure
{
    internal class DefaultContainer : IContainer
    {
        private IUnityContainer _container;
        private bool _disposed;
        private bool _alreadyDisposing;

        public DefaultContainer()
        {
            _container = new UnityContainer();
        }

        public void RegisterMany<TFrom, TTo>() where TTo : TFrom
        {
            _container.RegisterType(typeof(TFrom), typeof(TTo), typeof(TTo).Name, new ContainerControlledLifetimeManager(), new InjectionMember[] { });
        }

        public void RegisterService<TFrom, TTo>() where TTo : TFrom
        {
            _container.RegisterType<TFrom, TTo>(new ContainerControlledLifetimeManager());
        }

        public void RegisterService<TFrom>(TFrom instance)
        {
            _container.RegisterInstance<TFrom>(instance);
        }

        public TType Resolve<TType>() where TType : class
        {
            return _container.Resolve<TType>();
        }

        public IEnumerable<TType> ResolveAll<TType>() where TType : class
        {
            return _container.ResolveAll<TType>().ToArray();
        }

        public object Activate(Type type)
        {
            return _container.Resolve(type);
        }

        public T Activate<T>()
        {
            return (T)Activate(typeof(T));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed && !_alreadyDisposing)
            {
                if (disposing)
                {
                    if (_container != null)
                    {
                        // before disposing we must set this flag to detect when the container tries to dispose this class or else StackOverflowException ;) 

                        _alreadyDisposing = true;                                                                       

                        _container.Dispose();

                        _alreadyDisposing = false;
                    }
                }
                
                _disposed = true;
            }
        }        
    }
}
