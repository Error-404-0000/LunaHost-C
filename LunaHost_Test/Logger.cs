using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using MiddleWares;
namespace LunaHost.Test
{
    public class Logger : PageContent
    {
        public Logger() : base("/logs")
        { }
        [GetMethod("/get-all-logs")]
        public IHttpResponse GetLogs()
        {
            Task.Delay(10000).Wait();
            HttpResponse re = new()
            {
                Body = string.Join(",\n", LoggerAttribute.Loggers),
                StatusCode = 200,
            };
            re.Headers["Content-Type"] = "application/json";
            return re;
        }
        [GetMethod("/get-page")]
        public IHttpResponse GetTake([Required, FromUrlQuery("page-number")] int count)
        {
            HttpResponse re = new()
            {
                Body = string.Join(",\n", LoggerAttribute.Loggers.Take(count)),
                StatusCode = 200,
            };
            re.Headers["Content-Type"] = "application/json";
            return re;
        }
        [GetMethod, Logger]
        public IHttpResponse Get()
        {
            return HttpResponse.OK();
        }

    }
}
