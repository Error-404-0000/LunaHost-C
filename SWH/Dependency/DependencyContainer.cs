using Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost.Dependency
{
    public class DependencyContainer : IDependencyContainer
    {
        private readonly Dictionary<Type, ServiceRegistration> _registrations = new();

        

        public void AddSingleton<TService>(Func<IDependencyContainer, TService> factory) where TService : class
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _registrations[typeof(TService)] = new ServiceRegistration(container => factory(container), ServiceLifetime.Singleton);
        }

        public void AddTransient<TService>(Func<IDependencyContainer, TService> factory) where TService : class
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _registrations[typeof(TService)] = new ServiceRegistration(container => factory(container), ServiceLifetime.Transient);
        }

    

        
        public void AddSingleton<TService, TImplementation>() where TService : class where TImplementation : class, TService
        {
            _registrations[typeof(TService)] = new ServiceRegistration(container => Activator.CreateInstance(typeof(TImplementation))!, ServiceLifetime.Singleton);
        }

        public void AddTransient<TService, TImplementation>()
                  where TService : class
                  where TImplementation : class, TService
        {
            _registrations[typeof(TService)] = new ServiceRegistration(container => Activator.CreateInstance(typeof(TImplementation))!, ServiceLifetime.Transient);
        }
        public object? GetService<TServiceType>(TServiceType service)  where TServiceType : Type
        {
            if (_registrations.TryGetValue(service, out var registration))
            {
                if (registration.Lifetime == ServiceLifetime.Singleton && registration.SingletonInstance != null)
                {
                    return (object)registration.SingletonInstance;
                }
                var instance = registration.Factory(this);
                if (registration.Lifetime == ServiceLifetime.Singleton)
                {
                   registration.SingletonInstance = instance;
                }
                return (object)instance;
            }
            //service not registered, return default value
            return null;
        }

        public void AddTransient<TService>() where TService : class, new()
        {
            _registrations[typeof(TService)] = new ServiceRegistration(container => new TService(), ServiceLifetime.Transient);
        }

        public void AddSingleton<TService>() where TService : class, new()
        {
            _registrations[typeof(TService)] = new ServiceRegistration(container => new TService(), ServiceLifetime.Singleton);
        }
    }
}
