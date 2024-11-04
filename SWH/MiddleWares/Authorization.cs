using Interfaces;
using LunaHost.Attributes.MiddleWares;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost.Interfaces;
using MiddleWares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost.MiddleWares
{
    [AsMiddleWare]
    [AttributeUsage(AttributeTargets.Method)]
    public class AuthorizationAttribute : Attribute, IMiddleWare
    {
       
        public Task<IMiddleWareResult<IHttpResponse>> ExcuteAsync(HttpRequest request, Type ClassType)
        {
            if (!request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult<IMiddleWareResult<IHttpResponse>>(new MiddleWareResult<IHttpResponse>(HttpResponse.Unauthorized(),false));
            }
                return Task.FromResult<IMiddleWareResult<IHttpResponse>>(new MiddleWareResult<IHttpResponse>(HttpResponse.OK(),false));
        }
    }
}
