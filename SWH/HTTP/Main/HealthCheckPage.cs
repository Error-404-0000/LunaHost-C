using SWH.Attributes.HttpMethodAttributes;
using SWH.HTTP.Interface;
using SWH.MiddleWares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWH.HTTP.Main
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
