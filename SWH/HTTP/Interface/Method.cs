using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LunaHost.HTTP.Interface
{
    public interface IMethod
    {
        public string Path { get; set; }
        public UrlType UrlType { get; }
        public bool IgoneQue { get; set; }
        public bool ContainsStaticValue
        {
            get=>Regex.Match(Path, (@"{[a-zA-Z_][a-zA-Z0-9_]*}")).Success;
        }
       
    }


}
