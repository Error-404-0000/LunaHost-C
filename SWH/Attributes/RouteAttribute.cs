using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RouteAttribute(string route):Attribute
    {
        public string Route { get; set; } = route;
    }
}
