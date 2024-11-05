using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTTP.Interface
{
    public interface IHTTPParameter
    {
        string Name { get;  }
        bool IsSet
        {
            get => Name != null;
        }
    }
}
