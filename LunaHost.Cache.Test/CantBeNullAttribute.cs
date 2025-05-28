using Attributes.MiddleWare;
using LunaHost.Attributes;
using LunaHost.Attributes.MiddleWares;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost.Interfaces;
using LunaHost.MiddleWares;

namespace LunaHost.Test
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [AsMiddleWare]
    [ParameterMiddleWare] // This attribute indicates that this is a parameter middleware
    public class CantBeNullAttribute : Attribute, IMiddleWare
    {
        public async Task<IMiddleWareResult<IHttpResponse>> ExecuteAsync(HttpRequest request,[ObjectPrefer(Enums.Preferred.ParameterValue)] dynamic obj)
        {
            if (obj == null || (obj is string str && string.IsNullOrWhiteSpace(str)))
            {
                return new MiddleWareResult<IHttpResponse>(HttpResponse.BadRequest("Field cannot be null or empty"), false);
            }
            return new MiddleWareResult<IHttpResponse>(HttpResponse.OK(), true);
        }
    }

}
