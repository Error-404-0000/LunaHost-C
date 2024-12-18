
using LunaHost.Attributes.MiddleWares;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost.MiddleWares
{

    [AsMiddleWare]
    public abstract class MiddleWareAttribute : Attribute,IMiddleWare
    {
#pragma warning disable
        public abstract  Task<IMiddleWareResult<IHttpResponse>> ExecuteAsync(HttpRequest request, dynamic? none);
#pragma warning restore
    }
}
