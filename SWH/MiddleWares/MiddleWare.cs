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

    [AsMiddleWare]
    public abstract class MiddleWare : IMiddleWare
    {
#pragma warning disable
        public async Task<IMiddleWareResult<IHttpResponse>> ExcuteAsync(HttpRequest request, Type ClassType)
        {
           return new MiddleWareResult<IHttpResponse>(default(IHttpResponse),true);
        }
#pragma warning restore
    }
}
