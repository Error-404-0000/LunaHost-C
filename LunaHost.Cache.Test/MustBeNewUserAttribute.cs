using LunaHost.Attributes;
using LunaHost.Attributes.MiddleWares;
using LunaHost.Enums;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost.Interfaces;
using LunaHost.MiddleWares;

namespace LunaHost.Test
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [AsMiddleWare]
    public class MustBeNewUserAttribute : Attribute, IMiddleWare
    {
        public async Task<IMiddleWareResult<IHttpResponse>> ExecuteAsync(HttpRequest request,[ObjectPrefer(Preferred.ParameterValue)] dynamic obj)
        {
            if (UserRoute.UserDb.Any(u => u.Email == obj))
            {
                return new MiddleWareResult<IHttpResponse>(HttpResponse.BadRequest("Username already exists"), false);
            }
            return new MiddleWareResult<IHttpResponse>(HttpResponse.OK(), true);
        }
    }

}
