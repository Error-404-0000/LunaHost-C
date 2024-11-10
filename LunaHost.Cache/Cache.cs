using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LunaHost.Cache
{
    public class Cache<T> where T : ICacheable
    {
        public int Capacity { get; }
        public CacheItem<T>[] CacheItems { get; }
        private int _index;
        private readonly object index_lock = new();
        private int index
        {
            get => _index;
            set
            {
                lock (index_lock)
                {
                    _index = value;
                }
            }
        }
        private readonly int _expireAfterCalls;
        private (int CacheCode, int index, int exp) last_return = default;

        public Cache(int capacity, int expireAfterCalls = 10)
        {
            Capacity = capacity;
            _expireAfterCalls = expireAfterCalls;
            CacheItems = new CacheItem<T>[capacity];
       
        }

        public (bool found, T value) TryGet(int cacheCode)
        {
            var item = CacheItems.FirstOrDefault(x => x.IsNotNullOrDefault() && x.CacheCode == cacheCode);
            if (item.IsNotNullOrDefault() && !item.IsExpired())
            {
                item.TTL--;
                return (true, item.TValue);
            }
            return (false, default);
        }
        
 

        
        


        public void AddOrUpdate(T item)
        {
            var cacheCode = item.GetCacheCode();
            var cacheItem = new CacheItem<T>
            {
                CacheCode = cacheCode,
                TValue = item,
                TTL = _expireAfterCalls
            };

            index = GetExpiredOrCloseToExpired();
            CacheItems[index] = cacheItem;
        }

        public int GetExpiredOrCloseToExpired()
        {
            CacheItem<T>? item = CacheItems.OrderBy(x => x.TTL).FirstOrDefault();
            return item is null ? 0 : Array.IndexOf(CacheItems, item);
        }

        public void CleanupExpiredItems()
        {
            for (int i = 0; i < CacheItems.Length; i++)
            {
                if (CacheItems[i].IsNotNullOrDefault() && CacheItems[i].IsExpired())
                {
                    CacheItems[i] = default;
                }
            }
        }

        public unsafe ref T NewRef(ref T obj)
        {
            if (last_return != default && last_return.CacheCode == obj.GetCacheCode())
            {
                GC.SuppressFinalize(obj);
                return ref CacheItems[last_return.index].TValue;
            }
            if (Any(obj.GetCacheCode()) is var result && result.any)
            {
                GC.SuppressFinalize(obj);
                return ref CacheItems[result.TValue_Index].TValue;
            }
            if (obj is ICacheable cache)
            {
                return ref CacheItems[Pin(ref cache)].TValue;
            }
            else
            {
                throw new Exception("Not Match. T must be ICacheable");
            }
        }

        public T New(ref T obj)
        {
            if (last_return != default && last_return.CacheCode == obj.GetCacheCode())
            {
                GC.SuppressFinalize(obj);
                return CacheItems[last_return.index].TValue;
            }
            if (Any(obj.GetCacheCode()) is var result && result.any)
            {
                GC.SuppressFinalize(obj);
                return CacheItems[result.TValue_Index].TValue;
            }
            return obj is ICacheable cache ? CacheItems[Pin(ref cache)].TValue : throw new Exception("Not Match. T must be ICacheable");
        }

        public T Invoke(Delegate func, params object[] args)
        {
            return Invoke<T>(func, args);
        }

        public unsafe TResult Invoke<TResult>(Delegate func, params object[] args) where TResult : T
        {
            var hashcode = ICacheable.GenerateCacheHashCode(func.Method.Name, args);
            hashcode = hashcode < 0 ? -hashcode : hashcode;
            if (Any(hashcode) is var result && result.any)
            {
                GC.SuppressFinalize(func);
                GC.SuppressFinalize(args);
                return (TResult)CacheItems[result.TValue_Index].TValue;
            }
            var res = (TResult)func.DynamicInvoke(args)!;
            ICacheable newItem = new CacheItem<TResult>
            {
                CacheCode = hashcode,
                TValue = res,
                TTL = _expireAfterCalls
            };
            Pin<TResult>(ref newItem);
            return res;
        }

        public (bool any, int TValue_Index) Any(int cacheCode)
        {
            if (last_return.CacheCode == cacheCode && last_return.exp > 0 && CacheItems[last_return.index].IsNotNullOrDefault())
            {
                CacheItems[last_return.index].TTL = --last_return.exp;
                return (true, last_return.index);
            }

            for (int i = 0; i < CacheItems.Length; i++)
            {
                var item = CacheItems[i];
                if (item.IsNotNullOrDefault() && item.CacheCode == cacheCode)
                {
                    if (item.IsExpired()) return (false, -1);
                    last_return = (cacheCode, i, item.TTL - 1);
                    item.TTL--;
                    return (true, i);
                }
            }
            return (false, -1);
        }

        public int Pin(ref ICacheable cacheable)
        {
            return Pin<T>(ref  cacheable);
        }
        public int Pin<Timpl>(ref ICacheable cacheable) where Timpl : T
        {
            index = GetExpiredOrCloseToExpired();
            if(cacheable is not CacheItem<Timpl> )
            {
                ICacheable newItem = new CacheItem<Timpl>
                {
                    CacheCode = cacheable.GetCacheCode(),
                    TValue = (Timpl)cacheable,
                    TTL = _expireAfterCalls
                }; 
                GC.SuppressFinalize(cacheable);
                CacheItems[index] = (CacheItem<T>)cacheable;
                return index;
            }
            CacheItems[index] = (CacheItem<T>)cacheable;
            return index;
        }
 

    }
}