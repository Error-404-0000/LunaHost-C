using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public static class CacheHashCodeGenerator
{
    public static int GenerateCacheHashCode(params object[] values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));

        unchecked
        {
            int hash = 17;
            foreach (var value in values)
            {
                hash = hash * 31 + GetStableHashCode(value);
            }
            return hash;
        }
    }

    private static int GetStableHashCode(object value)
    {
        if (value == null)
        {
            return 0;
        }
        else if (value is string strValue)
        {
            // Use a consistent hash function for strings
            return GetMD5HashCode(strValue);
        }
        else if (value is IEnumerable enumerable)
        {
            unchecked
            {
                int hash = 17;
                foreach (var item in enumerable)
                {
                    hash = hash * 31 + GetStableHashCode(item);
                }
                return hash;
            }
        }
        else if (value.GetType().IsValueType)
        {
            return value.GetHashCode();
        }
        else
        {
           
            return GetMD5HashCode(value.ToString());
        }
    }

    private static int GetMD5HashCode(string input)
    {
        using (var md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToInt32(hashBytes, 0);
        }
    }
}

