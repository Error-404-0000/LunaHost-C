using LunaHost.Attributes;
using LunaHost.Attributes.MiddleWares;
using LunaHost.HTTP.Interface;
using LunaHost.Interfaces;
using LunaHost.MiddleWares;

namespace LunaHost.Test
{
    //this MiddleWare will be called on every request when the Method/Parameter is being called or set
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
    [AsMiddleWare]
    [NoPreferences]//Use to tell LunaHost to skip the prefer check on this method which means it wont try to  find what the Obj wants on the ExecuteAsync method
    public class LoggerAttribute : Attribute, IMiddleWare
    {
        public Task<IMiddleWareResult<IHttpResponse>> ExecuteAsync(HttpRequest request, dynamic obj)
        {
            Console.WriteLine($"[Log] {request.Method} {request.Path}");
            return Task.FromResult<IMiddleWareResult<IHttpResponse>>(new MiddleWareResult<IHttpResponse>(null!, true));
        }
    }

}
