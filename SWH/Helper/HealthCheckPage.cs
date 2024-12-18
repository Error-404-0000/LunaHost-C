using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost.MiddleWares;


namespace LunaHost.HTTP.Helper
{
    [Logger]
    public class HealthCheckPage : PageContent
    {
        public HealthCheckPage() : base("/health")
        {

        }

        [GetMethod("/{token}/check"), RequiredHeader("Host")]
        public  IHttpResponse Get([FromRoute]string token)
        {
            if(token == LunaHostBuilder.Build_Token)
                return HttpResponse.OK();
            return HttpResponse.InternalServerError();
        }
    }
}
