namespace LunaHost.Cache
{
    public unsafe struct CacheItem<T> : ICacheable where T : ICacheable
    {
        public int TTL;
        public T* TValue;
        public int CacheCode { get; set; }
        public bool IsNotNullOrDefault()
        {
            return !IsNull();
        }

        public bool IsNull()
        {
            return (nint)TValue == 0 || CacheCode == 0;
        }
        public bool SubTSO(int index)
        {
            TTL -= index;
            return true;
        }
        public bool CacheEquals(int _CacheCode)
        {
            return CacheCode == _CacheCode;
        }

        public bool IsExpired()
        {
            return TTL <= 0;
        }

        public override int GetHashCode()
        {
            return (this as ICacheable).GetCacheCode();
        }

    }


}