using LunaHost.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost.Attributes
{
    
    public class ObjectPrefer(Preferred pre):Attribute
    {
        public Preferred preferred = pre;
    }
}
