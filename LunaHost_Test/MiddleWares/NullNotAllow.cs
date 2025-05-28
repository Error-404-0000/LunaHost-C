using LunaHost;
using LunaHost.Attributes;
using LunaHost.Enums;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost.Interfaces;
using LunaHost.MiddleWares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost_Test.MiddleWares
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class NullNotAllow(string ParameterName) : MiddleWareAttribute
    {
        public override async Task<IMiddleWareResult<IHttpResponse>> ExecuteAsync(HttpRequest request, [ObjectPrefer(Preferred.ParameterValue)] dynamic? ParameterValue)
        {
            if((ParameterValue is string str && str.Trim() is "" )|| ParameterValue is null)
            {
                return new MiddleWareResult<HttpResponse>(new HttpResponse("application/json")
                {
                    Body = @$"{ParameterName} can't be null or """".",
                },false);
            }
           return new MiddleWareResult<IHttpResponse>(default!,true);
        }
    }
}
