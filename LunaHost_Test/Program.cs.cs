using LunaHost;

using LunaHost_Test.Endpoints_1v;
using (LunaHostBuilder  lunaHost = new())
{
    lunaHost.Add(new UserContent());
    lunaHost.Add(new ConfigurationContent());
    lunaHost.Add(new Redirector());
    lunaHost.UseSwagger = true;
    lunaHost.BuildAsync().Wait();
}