using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
namespace LunaHost.Test
{

    public class AccountContent : PageContent
    {
        [PostMethod("/register")]
        public IHttpResponse Register([Required(5, 15, "Username is required", regex: "^[a-zA-Z0-9]*$"), FromBody] string username, [FromBody, Required] int id, [FromBody("bod")] HttpResponse r)
        {
            return HttpResponse.OK(r.ToString());
        }
    }

}
