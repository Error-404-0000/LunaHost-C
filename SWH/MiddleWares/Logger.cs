using Interfaces;
using LunaHost;
using LunaHost.Attributes.MiddleWares;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiddleWares
{
    [AsMiddleWare]
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
    public class LoggerAttribute :Attribute, IMiddleWare
    {
        public static List<string> Loggers = new List<string>();
        public Task<IMiddleWareResult<IHttpResponse>> ExcuteAsync(HttpRequest request, Type ClassType)
        {
          //Example
            Loggers.Add($"{{\n\tDate : {DateTime.Now},\n\tMethod : {request.Method},\n\tPath : {request.Path},\n\tUrl : {request.Headers["Host"]+request.Path}\n}}");
            return Task.FromResult<IMiddleWareResult<IHttpResponse>>( new MiddleWareResult<IHttpResponse>(HttpResponse.OK(), true));
        }
    }
}
