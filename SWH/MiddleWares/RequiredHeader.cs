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
    /// <summary>
    /// checks if request contains a header
    /// </summary>
    ///
    [AsMiddleWare]
    [AttributeUsage(AttributeTargets.Method)]
    public class RequiredHeaderAttribute(string header) : Attribute, IMiddleWare
    {
        public Task<IMiddleWareResult<IHttpResponse>> ExecuteAsync(HttpRequest request, dynamic? none)
        {
            
            if (!request.Headers.ContainsKey(header))
                return Task.FromResult<IMiddleWareResult<IHttpResponse>>(new MiddleWareResult<IHttpResponse>(HttpResponse.BadRequest(), false));
            else
            {
                return Task.FromResult<IMiddleWareResult<IHttpResponse>>(new MiddleWareResult<IHttpResponse>(HttpResponse.OK(),true));
            }

        }
    }
}
