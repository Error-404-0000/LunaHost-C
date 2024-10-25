using SWH.Attributes.MiddleWares;
using SWH.HTTP.Interface;
using SWH.HTTP.Main;
using SWH.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWH.MiddleWares
{
    
    [AsMiddleWare]
    public abstract class MiddleWare : IMiddleWare
    {
#pragma warning disable
        public async Task<(bool successful, IHttpResponse if_failed)> ExcuteAsync(HttpRequest request, Type ClassType)
        {
           return (true, HttpResponse.OK());
        }
#pragma warning restore
    }
}
