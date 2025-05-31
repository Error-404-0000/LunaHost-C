using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IDependencyContainer
    {
     
           
            public void AddSingleton<TService, TImplementation>()
                where TService : class
                where TImplementation : class, TService;
            public void AddTransient<TService, TImplementation>()
                where TService : class
                where TImplementation : class, TService;
            
          
            public void AddSingleton<TService>(Func<IDependencyContainer, TService> factory)
                where TService : class;
            public void AddTransient<TService>(Func<IDependencyContainer, TService> factory)
                where TService : class;
        public void AddTransient<TService>()
                            where TService : class, new();
        public void AddSingleton<TService>()
                            where TService : class, new();
       
        public object? GetService<TServiceType>(TServiceType service)
                where TServiceType : Type;

           
        
    }
}
