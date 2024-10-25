using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWH.Attributes
{
    public class FromUrlQuery(string Name =null!):Attribute
    {
        public string Name = Name;
        public bool IsSet
        {
            get => Name != null;
        }
    }
   
    
}
