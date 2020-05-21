using OneShare.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OneShare.Object
{
    class MsgRequest
    {
        public string Type;
        public string MsgId;
        public byte[] Data;

        public MsgRequest(TripleDESCryptoServiceProvider crypto, Stream s)
        {
            byte[] raw = API.TripleDES.Decrypt(API.Encoding.ReadFully(s), crypto);

            Type = API.Encoding.ByteArrayToString(raw.Slice(0, 3));
            MsgId = API.Encoding.ByteArrayToString(raw.Slice(3, 4));

            Data = raw.Slice(7, raw.Length - 7);
        }
    }
}
