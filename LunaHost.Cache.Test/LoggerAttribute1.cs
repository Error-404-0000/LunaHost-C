using LunaHost.Attributes;
using LunaHost.Attributes.MiddleWares;
using LunaHost.HTTP.Interface;
using LunaHost.Interfaces;
using LunaHost.MiddleWares;

namespace LunaHost.Test
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    [AsMiddleWare]
    [NoPreferences]
    public class _LoggerAttribute : Attribute, IMiddleWare
    {
        public Task<IMiddleWareResult<IHttpResponse>> ExecuteAsync(HttpRequest request, dynamic obj)
        {
            Console.WriteLine($"[Log] {request.Method} {request.Path}");
            return Task.FromResult<IMiddleWareResult<IHttpResponse>>(new MiddleWareResult<IHttpResponse>(null!, true));
        }
    }

}
