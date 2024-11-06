using HTTP.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost.Attributes
{
    public class FromBody(string Name = null!) : Attribute , IHTTPParameter
    {
        public string Name { get; } = Name;
        public bool IsSet
        {
           
            get => Name != null;
        }

    }
}
