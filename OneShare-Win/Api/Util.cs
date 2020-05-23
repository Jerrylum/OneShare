using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OneShare.API
{
    public static class Util
    {
        private static Random random = new Random();

        public static string GetRandomHexString(int digits)
        {
            byte[] buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            if (digits % 2 == 0)
                return result;
            return result + random.Next(16).ToString("X");
        }

        public static string StringToEncryptHex(string payload, TripleDESCryptoServiceProvider crypto)
        {
            return ByteArrayToEncryptHex(API.Encoding.StringToByteArray(payload), crypto);
        }

        public static string ByteArrayToEncryptHex(byte[] payload, TripleDESCryptoServiceProvider crypto)
        {
            return API.Encoding.ByteArrayToHexString(API.TripleDES.Encrypt(payload, crypto));
        }


        public static byte[] Slice(this byte[] bytes, int from, int len)
        {
            byte[] rtnBytes = new byte[len];
            for (int i = 0; i < len; i++)
            {
                rtnBytes[i] = bytes[from + i];
            }
            return rtnBytes;
        }
    }
}
