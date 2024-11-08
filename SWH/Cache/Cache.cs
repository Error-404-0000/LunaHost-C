using LunaHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost.Cache
{
    public class Cache<T> where T : ICacheable
    {
        public int Capacity { get; }
        public CacheItem<T>[] CacheItems { get; }
        private int _index;
        private object index_lock = new object();
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
        private int Expire { get; set; }

        public Cache(int Capacity, int ExpireAfterCall = 10)
        {
            Expire = ExpireAfterCall;
            CacheItems = new CacheItem<T>[this.Capacity = Capacity];
        }
        public int GetExpiredOrCloseToExpired()
        {
            CacheItem<T>? item = CacheItems.OrderBy(x => x.TTL).FirstOrDefault();
            if (item is null)
                return 0;

            return CacheItems.ToList().IndexOf(item.Value);
        }
        private (int CacheCode, int index, int exp) last_return = default;
        public (bool any, int TValue_Index) Any(int CacheCode)
        {
            int index_c = 0;
            if (last_return.CacheCode == CacheCode && last_return.exp != 0)
            {
                CacheItems[last_return.index].TTL = --last_return.exp;
                return (true, last_return.index);

            }
            else if ((CacheItems.FirstOrDefault(x => x.CacheEquals(CacheCode) && (index_c++ == index_c - 1)) is CacheItem<T> cache && cache.IsNotNullOrDefault()))
            {
                if (cache.IsExpired())
                    return (false, default);
                index_c = index_c - 1 < 0 ? 0 : index_c;
                last_return = (CacheCode, index_c, CacheItems[index_c].TTL--);

                return (true, index_c);

            }
            return (false, -1);
        }

        public unsafe ref T NewRef(ref T obj)
        {
            if (last_return != default && last_return.CacheCode == obj.CacheCode)
            {
                GC.SuppressFinalize(obj);
                return ref *CacheItems[last_return.index].TValue;
            }
            if (Any(obj.GetCacheCode()) is var result && result.any)
            {
                GC.SuppressFinalize(obj);
                return ref *CacheItems[result.TValue_Index].TValue;
            }
            if (obj is ICacheable cache)
                return ref *CacheItems[Pin(ref cache)].TValue;
            else
                throw new Exception("Not Match.T must be ICacheable");
            throw new Exception("TResult does not match the value returned.");


        }
        public unsafe T New(ref T obj)
        {
            if (last_return != default && last_return.CacheCode == obj.CacheCode)
            {
                GC.SuppressFinalize(obj);
                return *CacheItems[last_return.index].TValue;
            }
            if (Any(obj.GetCacheCode()) is var result && result.any)
            {
                GC.SuppressFinalize(obj);
                return *CacheItems[result.TValue_Index].TValue;
            }
            if (obj is ICacheable cache)
                return *CacheItems[Pin(ref cache)].TValue;
            else
                throw new Exception("Not Match.T must be ICacheable");
            throw new Exception("TResult does not match the value returned.");


        }
        public T Invoke(Delegate func, params object[] args)
        {
            return Invoke<T>(func, args);
        }
        //awaiting impl
        public unsafe TResult Invoke<TResult>(Delegate func, params object[] args) where TResult : T
        {
#pragma warning disable
            var hashcode = (ICacheable.GenerateCacheHashCode(func.Method.Name, args));
            hashcode = hashcode < 0 ? -hashcode : hashcode;
            if (Any(hashcode) is var result && result.any)
            {
                GC.SuppressFinalize(func);
                GC.SuppressFinalize(args);
                Console.WriteLine("Did not invoke method");
                return *(TResult*)CacheItems[result.TValue_Index].TValue;
            }
            Console.WriteLine("Called and invoked");
            var res = (TResult)func.DynamicInvoke(args)!;
            T* ptr = (T*)Unsafe.AsPointer<TResult>(ref res);
            var object_size = (int)GetObjectSize(res);
            var new_ptr = Marshal.AllocHGlobal(object_size);
            Buffer.MemoryCopy((void*)ptr, (void*)new_ptr, object_size, object_size);
            CacheItem<T> cacheItem = new CacheItem<T>()
            {
                CacheCode = hashcode,
                TValue = (T*)(TResult*)new_ptr,
                TTL = Expire
            };
            Pin(ref cacheItem);
            return (TResult)res;

#pragma warning restore
        }
        unsafe void FreeMem(T* obj)
        {
            if (obj is not null &&(nint)obj != 0)
            {
                Marshal.FreeCoTaskMem((nint)obj);
            }
        }
        public void Pin(ref CacheItem<T> cacheItem)
        {
            index = index = GetExpiredOrCloseToExpired();
            unsafe
            {
                FreeMem(CacheItems[index].TValue);
            }
            CacheItems[index] = cacheItem;
        }
        public int Pin(ref ICacheable cacheable)
        {
            index = index = GetExpiredOrCloseToExpired();
            unsafe
            {
                FreeMem(CacheItems[index].TValue);
            }
            unsafe
            {
#pragma warning disable

                ICacheable* ptr = (ICacheable*)Unsafe.AsPointer<ICacheable>(ref cacheable);
                var object_size = (int)GetObjectSize(cacheable);
                var new_ptr = Marshal.AllocHGlobal(object_size);
                Buffer.MemoryCopy((void*)ptr, (void*)new_ptr, object_size, object_size);
                CacheItem<T> new_pin = new CacheItem<T>()
                {
                    TTL = Expire,
                    TValue = (T*)new_ptr,
                    CacheCode = ptr->GetCacheCode()
                };

                CacheItems[index] = new_pin;
                GC.SuppressFinalize(cacheable);
                return index - 1 < 0 ? 0 : index - 1;
#pragma warning restore
            }



        }

        public static long GetObjectSize(object obj)
        {
            if (obj == null) return 0;

            var processedObjects = new HashSet<object>();
            return CalculateObjectSize(obj, processedObjects);
        }

        private static long CalculateObjectSize(object obj, HashSet<object> processedObjects)
        {
            if (obj == null || processedObjects.Contains(obj))
                return 0;

            processedObjects.Add(obj);
            Type type = obj.GetType();

            // Handle primitive and fixed-size types directly
            if (type.IsPrimitive || obj is decimal || obj is IntPtr)
            {
                return System.Runtime.InteropServices.Marshal.SizeOf(type);
            }

            // Estimate size for arrays by summing up each element's size
            if (type.IsArray)
            {
                long size = 0;
                foreach (var element in (Array)obj)
                {
                    size += CalculateObjectSize(element, processedObjects);
                }
                return size;
            }

            long totalSize = IntPtr.Size;  // Start with object header size (approximate)

            // Process each field in the object, recursively adding their sizes
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object fieldValue = field.GetValue(obj);
                totalSize += CalculateObjectSize(fieldValue, processedObjects);
            }

            return totalSize;
        }
    }

}
