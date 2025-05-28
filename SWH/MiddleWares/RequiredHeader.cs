
using LunaHost.Attributes.MiddleWares;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost.MiddleWares
{
    /// <summary>
    /// checks if request contains a header
    /// </summary>
    ///
    [AsMiddleWare]
    [AttributeUsage(AttributeTargets.Method |AttributeTargets.Class)]
    public class RequiredHeaderAttribute(params string[] headers) : Attribute, IMiddleWare
    {
        public Task<IMiddleWareResult<IHttpResponse>> ExecuteAsync(HttpRequest request, dynamic? none)
        {

            foreach (var header in headers)
            {
                if (!request.Headers.ContainsKey(header))
                    return Task.FromResult<IMiddleWareResult<IHttpResponse>>(new MiddleWareResult<IHttpResponse>(HttpResponse.BadRequest($"Missing Header : {header}"), false));

            }

            return Task.FromResult<IMiddleWareResult<IHttpResponse>>(new MiddleWareResult<IHttpResponse>(HttpResponse.OK(), true));


        }
    }
}
