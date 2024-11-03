using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunaHost.HTTP.Interface
{
    public interface IHTTPMethod
    {
        public HttpMethod Type { get; }
    }

}
