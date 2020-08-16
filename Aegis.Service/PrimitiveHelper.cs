using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Aegis.Service
{
    public static class PrimitiveHelper
    {
        public static byte[] StringToByteArray(string hex) {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public static byte[] ComputeHash(string value)
        {
            byte[] pwdBytes = Encoding.UTF8.GetBytes(value);

            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(pwdBytes);

            return hash;
        }

        public static bool ArrayEquals<T>(T[] item1, T[] item2)
        {
            return ArrayEquals<T>(item1, item2, EqualityComparer<T>.Default);
        }

        public static bool ArrayEquals<T>(T[] item1, T[] item2, IEqualityComparer<T> equalityComparer)
        {
            if (item1.Length != item2.Length)
                return false;

            for (int i = 0; i < item1.Length; i++)
            {
                if (!equalityComparer.Equals(item1[i], item2[i]))
                    return false;
            }

            return true;
        }
    }
}