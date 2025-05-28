using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;

namespace LunaHost.Test
{
    [Route("/hello")]
    public class HelloWorld : PageContent
    {

        
        [GetMethod("/")]
        public IHttpResponse SayHello() => HttpResponse.OK("Hello from LunaHost!");
    }

}
