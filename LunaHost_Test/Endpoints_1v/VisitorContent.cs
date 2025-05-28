using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost.MiddleWares;
using LunaHost_Test.Db;
using LunaHost_Test.MiddleWares;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost_Test.Endpoints_1v
{
    [Route("/visitor")]
    [RequiredHeader("Authorization", "Token")]
    public class VisitorContent:PageContent
    {

        [GetMethod("/{configId}")]
        public IHttpResponse GetAllVisitor([FromHeader("Authorization")] string auth,[FromHeader("Token")] string token, [FromRoute("configId"),NullNotAllow("configId")] string configId)
        {
            if(AppDb.UserisAuth(token, auth))
            {
                var resp = HttpResponse.OK(JsonConvert.SerializeObject(AppDb.GetVisitors(configId, token, auth)));
                resp.Headers["Content-Type"] = "application/json";
                return resp;
            }
            else
            {
                return HttpResponse.Unauthorized();
            }
        }
        [GetMethod("/{config}/search")]
        public IHttpResponse SearchByName([FromHeader("Authorization")] string auth, [FromHeader("Token")] string token, [FromUrlQuery("q")]string Que, [FromRoute("configId"), NullNotAllow("configId")] string configId)
        {
            if (AppDb.UserisAuth(token, auth))
            {
                var resp= HttpResponse.OK(JsonConvert.SerializeObject(AppDb.GetVisitors( configId,token, auth).Where(x=>x.NickName.ToLower().Contains(Que.ToLower()))));
                resp.Headers["Content-Type"] = "application/json";
                return resp;
            }
            else
            {
                return HttpResponse.Unauthorized();
            }
        }
   

    }
}
