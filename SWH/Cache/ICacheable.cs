using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost.Cache
{
    public interface ICacheable
    {
        public int CacheCode { get; set; }
        public int GetCacheCode()
        {
            if (CacheCode == 0)
                return (CacheCode = GenerateCacheHashCode(this));
            return CacheCode;
        }
        public static int GenerateCacheHashCode(params object[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            unchecked
            {
                int hash = 17;
                foreach (var value in values)
                {
                    if (value != null)
                    {
                        hash = hash * 31 + GetDeterministicHashCode(value);
                    }
                }
                return hash;
            }
        }
        private static int GetDeterministicHashCode(object value)
        {
            if (value is string strValue)
            {
                return StringComparer.Ordinal.GetHashCode(strValue);
            }
            else if (value is IEnumerable<object> enumerable)
            {
                int hash = 17;
                foreach (var item in enumerable)
                {
                    hash = hash * 31 + (item?.GetHashCode() ?? 0);
                }
                return hash;
            }
            else
            {
                return value.GetHashCode();
            }
        }

        public static int GenerateCacheHashCode(ICacheable obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            int hash = 17;
            var properties = obj.GetType().GetProperties();

            foreach (var property in properties)
            {
                //skipping the CacheCode 
                if (property.Name == nameof(CacheCode))
                    continue;
                var value = property.GetValue(obj);
                hash = hash * 31 + (value?.GetHashCode() ?? 0);
            }
            if (hash <= 0)
            {
                hash = -hash;
            }
            return hash;
        }
    }


}
