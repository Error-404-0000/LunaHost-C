using CacheLily.Attributes;
using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost_Test.Db;
using LunaHost_Test.Routes;
using LunaHost_Test.Visitors;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Http.Headers;
using System.Web;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace LunaHost_Test.Endpoints_1v
{
    [NoCaching]
    [Route("/sv")]
    [LunaHost.MiddleWares.Security.DDoSProtection.Redirector]
    public class Redirector : PageContent
    {


        [GetMethod("/{config_id}/{request}")]
        public IHttpResponse HandleGet([FromRoute("config_id")] string configId, [FromRoute("request")] string requestPath)
        {
            return new RequestHandler(this.request).ProcessRequest(configId, requestPath, "GET");
        }

        [PostMethod("/{config_id}/{request}")]
        public IHttpResponse HandlePost([FromRoute("config_id")] string configId, [FromRoute("request")] string requestPath)
        {
            return new RequestHandler(this.request).ProcessRequest(configId, requestPath, "POST");
        }
        [PutMethod("/{config_id}/{request}")]
        public IHttpResponse HandlePut([FromRoute("config_id")] string configId, [FromRoute("request")] string requestPath)
        {
            return new RequestHandler(this.request).ProcessRequest(configId, requestPath, "PUT");
        }
        [DeleteMethod("/{config_id}/{request}")]
        public IHttpResponse HandleDelete([FromRoute("config_id")] string configId, [FromRoute("request")] string requestPath)
        {
            return new RequestHandler(this.request).ProcessRequest(configId, requestPath, "DELETE");
        }
        private string Host = "127.0.0.1";



    }
}
