using System;
using System.Collections.Generic;
using IoC.InstallGenerator.Abstractions;

namespace Demo.Host
{
    // T059: Simple container adapter for demonstration
    // This is a minimal implementation for demo purposes
    public class SimpleContainerAdapter : IIoCContainer
    {
        private readonly Dictionary<Type, object> _registrations = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Type> _typeRegistrations = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Func<IIoCContainer, object>> _factoryRegistrations = new Dictionary<Type, Func<IIoCContainer, object>>();

        public void Register<TService, TImplementation>() where TImplementation : TService
        {
            _typeRegistrations[typeof(TService)] = typeof(TImplementation);
        }

        public void Register<TService>(TService instance)
        {
            _registrations[typeof(TService)] = instance!;
        }

        public void Register<TService>(Func<IIoCContainer, TService> factory)
        {
            _factoryRegistrations[typeof(TService)] = (container) => factory(container)!;
        }

        public TService Resolve<TService>()
        {
            if (_registrations.TryGetValue(typeof(TService), out var instance))
            {
                return (TService)instance;
            }

            if (_typeRegistrations.TryGetValue(typeof(TService), out var implementationType))
            {
                return (TService)Activator.CreateInstance(implementationType)!;
            }

            if (_factoryRegistrations.TryGetValue(typeof(TService), out var factory))
            {
                return (TService)factory(this);
            }

            throw new InvalidOperationException($"Service {typeof(TService).Name} is not registered.");
        }
    }
}

