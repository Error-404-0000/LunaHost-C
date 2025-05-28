using LunaHost.Attributes;
using LunaHost.Enums;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost.Interfaces;
using LunaHost.MiddleWares;
using LunaHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunaHost_Test.Db;
using LunaHost_Test.Routes;

namespace LunaHost_Test.MiddleWares
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class ConfigurationUpdate : MiddleWareAttribute
    {
        public override async Task<IMiddleWareResult<IHttpResponse>> ExecuteAsync(HttpRequest request, [ObjectPrefer(Preferred.ParameterValue)] dynamic? ParameterValue)
        {
            if (AppDb.Configurations.FirstOrDefault(x => x.Id.ToString().Replace("-", "") == ParameterValue?.ToString()) is Configuration configuration)
            {

                if (configuration.isDisabled)
                    goto something_went_wrong;
                configuration.TotalRequest += 1;
                return new MiddleWareResult<IHttpResponse>(default!, true);
            }
            something_went_wrong:
            return new MiddleWareResult<IHttpResponse>(new HttpResponse("application/json")
            {
                Body = @$"Something went wrong....",
            }, false);
        }
    }
}
