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
    /// <summary>
    /// checks if request contains a header
    /// </summary>
    ///
    [AsMiddleWare]
    public class RequiredHeaderAttribute(string header) : Attribute, IMiddleWare
    {
        public Task<(bool successful, IHttpResponse if_failed)> ExcuteAsync(HttpRequest request, Type ClassType)
        {
            
            if (!request.Headers.ContainsKey(header))
                return Task.FromResult<(bool sucessfull, IHttpResponse if_failed)>((false, HttpResponse.BadRequest()));
            else
            {
                return Task.FromResult<(bool sucessfull, IHttpResponse if_failed)>((true, HttpResponse.OK()));
            }

        }
    }
}
