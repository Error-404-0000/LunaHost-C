using LunaHost;
using LunaHost.Test;

namespace LunaHost_Test
{
  public class Program
    {
        private static void Main(string[] args)
        {
            using (LunaHostBuilder Builder = new(1))
            {
                Builder.Add(new Logger());
                Builder.Add(new UserApi());
                Builder.Add(new AccountContent());
                Builder.UseSwagger = true;
                Builder.BuildAsync().Wait();
            }
     
        }
    }
}