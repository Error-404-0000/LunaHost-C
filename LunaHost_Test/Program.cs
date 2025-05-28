using LunaHost;

using LunaHost_Test.Endpoints_1v;

using (LunaHostBuilder  lunaHost = new())
{
    lunaHost.IP = new System.Net.IPAddress([10, 0, 0, 71]);

    lunaHost.Add(new UserContent());
    lunaHost.Add(new ConfigurationContent());
    lunaHost.Add(new Redirector());
    
    lunaHost.Add(new VisitorContent());
    lunaHost.UseSwagger = true;
    lunaHost.BuildAsync().Wait();
}