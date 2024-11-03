using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;

namespace LunaHost
{
    public partial class LunaHostBuilder
    {
        public class ErrorPage : PageContent
        {
            public ErrorPage() : base("/") { }

            [GetMethod("/{any}")]
            public override IHttpResponse Get()
            {
                return HttpResponse.NotFound("ERROR: Page not found.");
            }
        }

    }
}
