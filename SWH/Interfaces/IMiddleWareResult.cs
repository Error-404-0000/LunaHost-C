using LunaHost.HTTP.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{

    public interface IMiddleWareResult<T1> where T1 : IHttpResponse
    {
        T1 Response { get;  }
        bool Success { get;  }
      
    }

}
