using System;
using System.Collections.Generic;

namespace ShapeFlow.Infrastructure
{
    /// <summary>
    /// A dependency injection container.
    /// </summary>
    public interface IContainer : IDisposable
    {
        /// <summary>
        /// Registers the service represented by the <typeparamref name="TFrom" /> contract and implemented by the given object.
        /// </summary>
        /// <typeparam name="TFrom">The type of the contract.</typeparam>
        /// <param name="instance">The instance.</param>
        void RegisterService<TFrom>(TFrom instance);

       
        void RegisterMany<TFrom, TTo>() where TTo : TFrom;

        /// <summary>
        /// Registers the service represented by the <typeparamref name="TFrom"/> contract and implemented by the <typeparamref name="TTo"/> class.
        /// </summary>
        /// <typeparam name="TFrom">The type of the contract.</typeparam>
        /// <typeparam name="TTo">The type of the implementation.</typeparam>
        void RegisterService<TFrom, TTo>() where TTo : TFrom;

      
        /// <summary>
        /// Gets the instance of the provided contract.
        /// </summary>
        /// <typeparam name="TType">The type of the contract.</typeparam>
        /// <returns>The instance of the provided contract.</returns>
        TType Resolve<TType>() where TType : class;

        /// <summary>
        /// Gets all the instances of the provided contract.
        /// </summary>
        /// <typeparam name="TType">The type of the contract.</typeparam>
        /// <returns>The instances of the provided contract.</returns>
        IEnumerable<TType> ResolveAll<TType>() where TType : class;

        
        T Activate<T>();
    }
}
