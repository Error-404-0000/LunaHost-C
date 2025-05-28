using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost.Attributes
{
    /// <summary>
    /// used to skip the prefer check on methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
    public class NoPreferences:Attribute;
}
