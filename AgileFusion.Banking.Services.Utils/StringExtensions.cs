using System;
using System.Text;

namespace AgileFusion.Banking.Services.Utils
{
    public static class StringExtensions
    {
        public static Encoding DefaultEncoding => Encoding.UTF8;
        public static string ToBase64(this string str, Encoding encoding = null)
        {
            var bytes = str.ToBytes(encoding);
            return Convert.ToBase64String(bytes);
        }

        public static string ToBase64(this byte[] bytes, Encoding encoding = null)
        {
            return Convert.ToBase64String(bytes);
        }

        public static string FromBase64(this string str, Encoding encoding = null)
        {
            var bytes = Convert.FromBase64String(str);
            return bytes.FromBytes();
        }

        public static byte[] ToBytes(this string str, Encoding encoding = null)
        {
            var enc = encoding ?? DefaultEncoding;
            return enc.GetBytes(str);
        }

        public static string FromBytes(this byte[] bytes, Encoding encoding = null)
        {
            var enc = encoding ?? DefaultEncoding;
            return enc.GetString(bytes);
        }
    }
}
