using System;
using System.Collections.Generic;

namespace ShapeFlow.Infrastructure
{
    public interface IContainer : IDisposable
    {       
        void RegisterMany<TFrom, TTo>() where TTo : TFrom;

        void RegisterService<TFrom, TTo>() where TTo : TFrom;

        void RegisterService<T>(T instance);

        void RegisterService<T>();

        TType Resolve<TType>() where TType : class;

        IEnumerable<TType> ResolveAll<TType>() where TType : class;
        
        T Activate<T>();

        T Activate<T>(Type t);
    }
}
