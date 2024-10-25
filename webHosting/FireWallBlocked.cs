using SWH.Attributes.HttpMethodAttributes;
using SWH.HTTP.Interface;
using SWH.HTTP.Main;
namespace webHosting
{
    public class FireWallBlocked:PageContent
    {
        public FireWallBlocked():base("/FirewallBlocked")
        {
            
        }
        [GetMethod("/blocked")]
        public IHttpResponse Get()
        {
            return HttpResponse.OK(File.ReadAllText("C:\\Users\\Demon\\source\\repos\\webHosting\\webHosting\\FirewallBlocked.html"));
        }
        [GetMethod("/blocked/m")]
        public IHttpResponse GetM()
        {
            return HttpResponse.OK("MMMM");
        }
    }
    
}
