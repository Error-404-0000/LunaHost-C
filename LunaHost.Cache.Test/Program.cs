using LunaHost.HTTP.Interface;

namespace LunaHost.Test
{

    public class Program
    {
        static void Main(string[] args)
        {
            using (LunaHostBuilder host = new LunaHostBuilder())
            {
                host.Errorpage = (HTMLContent)"<h1>Error Page</h1>";
                host.Add(new HelloWorld());
                host.Add(new UserRoute());
                host.Port = 80;
                host.UseSwagger = true;
                ////
                //host.onRequestReceived += Host_onRequestReceived;
                //host.onResponseSent += Host_onResponseSent;
                host.BuildAsync().Wait();
            }
        }

        private static void Host_onResponseSent(object sender, HttpRequest request, IHttpResponse respond)
        {
            Console.WriteLine($"[RESPOND] :{respond}");
        }

        private static void Host_onRequestReceived(object sender, HttpRequest request)
        {
            Console.WriteLine($"[REC] : {request}");
        }
    }

}
