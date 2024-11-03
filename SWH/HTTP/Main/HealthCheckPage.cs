using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.MiddleWares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost.HTTP.Main
{
    public class HealthCheckPage: PageContent
    {
        public HealthCheckPage():base("/health")
        {
            
        }
        [GetMethod,RequiredHeader("Host")]
        public  IHttpResponse Get()
        {
            return HttpResponse.OK();
        }
    }
}
