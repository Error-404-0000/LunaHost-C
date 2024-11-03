using LunaHost.Attributes.MiddleWares;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost.MiddleWares
{
    [AsMiddleWare]
    public class AuthorizationAttribute : Attribute, IMiddleWare
    {
        public Task<(bool successful, IHttpResponse if_failed)> ExcuteAsync(HttpRequest request, Type ClassType)
        {
            if (!request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult< (bool successful, IHttpResponse if_failed)>((false,HttpResponse.Unauthorized()));
               
            }
            return Task.FromResult<(bool successful, IHttpResponse if_failed)>((true, HttpResponse.OK()));
        }
    }
}
