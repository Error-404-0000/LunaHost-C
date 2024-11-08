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
            // Handle collections in a deterministic manner
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
            // For value types, use their default hash code
            return value.GetHashCode();
        }
        else
        {
            // Use a consistent hash function for reference types
            return GetMD5HashCode(value.ToString());
        }
    }

    private static int GetMD5HashCode(string input)
    {
        using (var md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            // Convert first 4 bytes of hash to an integer
            return BitConverter.ToInt32(hashBytes, 0);
        }
    }
}

// Example usage
// int hash = HashCodeGenerator.GenerateCacheHashCode("example", new int[] { 1, 2, 3 }, 42);
