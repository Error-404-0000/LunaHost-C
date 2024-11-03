using LunaHost;
namespace LunaHost_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using(LunaHostBuilder Builder= new LunaHostBuilder())
            {
                Builder.BuildAsync().Wait();
            }  
        }
    }
}
