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
        /// All IMiddleWare must have ExcuteAsync mothod.
        /// </summary>
        /// <param name="method">determines if the code should break or continue</param>
        /// <returns></returns>
        
        Task<(bool successful, IHttpResponse if_failed)> ExcuteAsync(HttpRequest request,Type ClassType);
    }
}
