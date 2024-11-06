using HTTP.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost.Attributes
{
    public class FromHeader : Attribute,IHTTPParameter
    {
        public string Name {  get; }  
        public bool IsSet {  get; set; }
        public FromHeader()
        {
            
        }
        public FromHeader(string name)
        {
            Name = name;
            IsSet = true;
        }
    }
  
}
