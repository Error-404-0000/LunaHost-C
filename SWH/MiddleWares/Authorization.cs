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
    public class AuthorizationAttribute : Attribute, IMiddleWare
    {
        public Task<(bool successful, IHttpResponse if_failed)> ExcuteAsync(HttpRequest request, Type ClassType)
        {
            if (!request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult< (bool successful, IHttpResponse if_failed)>((false,HttpResponse.Unauthorized()));
               
            }
            return Task.FromResult<(bool successful, IHttpResponse if_failed)>((true, HttpResponse.OK()));
        }
    }
}
