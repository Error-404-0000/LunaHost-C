using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost
{
    public unsafe struct CacheItem<T>
    {
        public int TimeExpire;
        public int HashCode;
        public T* Returned;
        public bool IsNull()
        {
            if (TimeExpire is 0)
                return true;
            if(HashCode == 0) return false;
            return true;
        }
    }
}
