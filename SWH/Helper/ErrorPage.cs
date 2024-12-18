using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost.MiddleWares;

namespace LunaHost
{
    public partial class LunaHostBuilder
    {
        [Logger] 
        public class ErrorPage : PageContent
        {
            public ErrorPage() : base("/") { }

            [GetMethod("/{any}")]
            [Logger]
            public  IHttpResponse Get([FromRoute("any")]string invalid_url)
            {
                return HttpResponse.NotFound($"Page not found : {invalid_url}. ");
            }
        }

    }
   
}
