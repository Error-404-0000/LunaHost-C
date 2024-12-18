using LunaHost.Attributes;
using LunaHost.Enums;
using LunaHost.HTTP.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost.Interfaces
{
    /// <summary>
    /// All MiddleWare Must import IMiddleWare 
    /// </summary>
    public interface IMiddleWare
    {
        /// <summary>
        /// All IMiddleWare must have ExecuteAsync Method.
        /// </summary>
        /// <param name="method">determines if the code should break or continue</param>
        /// <returns></returns>
        
        Task<IMiddleWareResult<IHttpResponse>> ExecuteAsync(HttpRequest request,[ObjectPrefer(Preferred.None)]dynamic? obj);
    }
}
