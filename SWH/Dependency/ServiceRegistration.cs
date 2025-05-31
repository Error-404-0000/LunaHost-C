namespace LunaHost.Dependency
{
    public record class ServiceRegistration
    {
        public Func<DependencyContainer, object> Factory { get; }
        public ServiceLifetime Lifetime { get; }
        public object? SingletonInstance { get; set; }

        public ServiceRegistration(Func<DependencyContainer, object> factory, ServiceLifetime lifetime)
        {
            Factory = factory;
            Lifetime = lifetime;
        }
    }
}
