using Attributes;
using Enums;
using Interfaces;
using LunaHost;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiddleWares
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Required(string message = null!) :Attribute, IMiddleWare
    {
        public async Task<IMiddleWareResult<IHttpResponse>> ExecuteAsync(HttpRequest request, [ObjectPrefer(Preferred.ParameterValue)] dynamic obj)
        {
            if (obj is null or default(object) || obj  is string s && (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s)))
            {
                return new MiddleWareResult<IHttpResponse>(HttpResponse.BadRequest(message ?? "404 Bad Request"), false);
            }
            return new MiddleWareResult<IHttpResponse>(HttpResponse.OK(), true);

        }
    }
}
