using LunaHost;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using MiddleWares;
namespace LunaHost_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (LunaHostBuilder Builder = new LunaHostBuilder())
            {
                Builder.Add(new Logger());
                Builder.BuildAsync().Wait();
            }
        }
        public class Logger : PageContent
        {
            public Logger() : base("/logs")
            { }
            [GetMethod("/get-all-logs")]
            public IHttpResponse GetLogs()
            {
                var re = new HttpResponse()
                {
                    Body = string.Join(",\n", LoggerAttribute.Loggers),
                    StatusCode = 200,
                };
                re.Headers["Content-Type"] = "application/json";
                return re;
            }
            [GetMethod,Logger]
            public IHttpResponse Get()
            {
                return HttpResponse.OK();
            }
        }
    }
}
