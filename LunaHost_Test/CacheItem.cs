using LunaHost_Test;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public  unsafe struct CacheItem<T>:ICacheable where T : ICacheable
{
    public  int TimeExpire;
    public  T* TValue;
    public int CacheCode { get; set; }
    public bool IsNotNullOrDefault() => !IsNull();
    public bool IsNull()
    {
        if (TimeExpire == 0)
            return true;
        if (CacheCode == 0) return true;
        return false;
    }
    public bool CacheEquals(int CacheCode)
     =>this.CacheCode == CacheCode;
    public bool IsExpired(int Timespan) 
        =>this.TimeExpire <= Timespan;
   
    public override int GetHashCode()
    {
        return (this as ICacheable).GetCacheCode(); 
    }

}

