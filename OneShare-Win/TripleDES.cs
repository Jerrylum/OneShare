using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace OneShare.API
{
    public class TripleDES
    {
        public static Dictionary<string, TripleDESCryptoServiceProvider> UserDict = new Dictionary<string, TripleDESCryptoServiceProvider>();

        public static TripleDESCryptoServiceProvider createDESCrypto(String key, String iv)
        {
            byte[] keyArray = API.Encoding.HexStringToBytArray(key);
            byte[] ivArray = API.Encoding.HexStringToBytArray(iv);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.PKCS7;
            tdes.IV = ivArray;

            return tdes;
        }

        public static byte[] Encrypt(byte[] plain, TripleDESCryptoServiceProvider tdes)
        {
            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(plain, 0, plain.Length);
            cTransform.Dispose();

            return resultArray;
        }


        public static byte[] Decrypt(byte[] cipher, TripleDESCryptoServiceProvider tdes)
        {
            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(cipher, 0, cipher.Length);
            cTransform.Dispose();

            return resultArray;
        }
    }
}
