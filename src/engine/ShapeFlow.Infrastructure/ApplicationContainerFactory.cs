using System;

namespace ShapeFlow.Infrastructure
{   
    public static class ApplicationContainerFactory
    {        
        internal static Func<IContainer> Factory { get; private set; } = () => new DefaultContainer();

        public static IContainer Create(Action<IContainer> registrationFunction = null)
        {
            if (Factory == null)
            {
                throw new InvalidOperationException(InfrastructureErrorMessages.ValidFactoryIsRequired);
            }

            try
            {
                var result = Factory();
                result.RegisterService(result);

                registrationFunction?.Invoke(result);

                return result;
            }
            catch(Exception e)
            {
                throw new InvalidOperationException(InfrastructureErrorMessages.UnexpectedErrorOcurred, e);
            }
        }

        public static void SetFactory(Func<IContainer> factory)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }
    }
}
