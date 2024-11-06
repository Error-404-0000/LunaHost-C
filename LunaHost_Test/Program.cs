using LunaHost;
using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using MiddleWares;
using System.Net;
using System.Text.RegularExpressions;
namespace LunaHost_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (LunaHostBuilder Builder = new LunaHostBuilder())
            {
                //returns full error
                Builder.InDebugMode = true;
                Builder.Add(new Logger());
                Builder.Add(new AccountContent());
                Builder.UseSwagger = true;
                Builder.BuildAsync().Wait();
            }
        }
        public class AccountContent : PageContent
        {
            [PostMethod("/register")]
            public IHttpResponse Register([Required(5, 15, "Username is required",regex: "^[a-zA-Z0-9]*$"),FromBody] string username,[FromBody,Required]int id, [FromBody("bod")]HttpResponse r)
            {

                // Only called if the Required middleware validation passes
                return HttpResponse.OK(r.ToString());
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
            [GetMethod("/get-page")]
            public IHttpResponse GetTake([Required,FromUrlQuery("page-number")]int count)
            {
                var re = new HttpResponse()
                {
                    Body = string.Join(",\n", LoggerAttribute.Loggers.Take(count)),
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
