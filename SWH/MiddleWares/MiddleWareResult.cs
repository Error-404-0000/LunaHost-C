using Interfaces;
using LunaHost.HTTP.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiddleWares
{
    public class MiddleWareResult<T1> : IMiddleWareResult<T1> where T1 : IHttpResponse
    {
        public T1 Response { get; }

        public bool Success { get; }
        public MiddleWareResult(T1 response,bool Success)
        {
            this.Response = response;
            this.Success = Success;
        }
    }
}
