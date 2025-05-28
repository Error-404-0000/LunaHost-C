using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;

namespace LunaHost.Test
{
    public sealed class HTMLContent : PageContent
    {
        public string OnGet { get; init; }
        public HTMLContent(string path = "/") : base(path)
        {

        }
        [GetMethod("/")]
        public IHttpResponse Get()
        {
            return new HttpResponse
            {
                Body = OnGet,
                StatusCode = 200,
                Headers = { ["Content-Type"] = "text/html" }
            };
        }
     
        public static implicit operator HTMLContent(string v)
        {
            return new HTMLContent { OnGet = v };
        }
        public static explicit operator string(HTMLContent v)
        {
            return v.OnGet;
        }
    }

}
