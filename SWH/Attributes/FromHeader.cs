using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWH.Attributes
{
    public class FromHeader : Attribute
    {
        public string Name {  get; set; }  
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
