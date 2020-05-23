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

        //public MsgRequest(TripleDESCryptoServiceProvider crypto, Stream s)
        //{
        //    byte[] raw = API.TripleDES.Decrypt(API.Encoding.ReadFully(s), crypto);

        //    Type = API.Encoding.ByteArrayToString(raw.Slice(0, 3));
        //    MsgId = API.Encoding.ByteArrayToString(raw.Slice(3, 4));

        //    Data = raw.Slice(7, raw.Length - 7);
        //}

        public MsgRequest(TripleDESCryptoServiceProvider crypto, byte[] bytes)
        {
            byte[] raw = API.TripleDES.Decrypt(
                             API.Encoding.HexStringToByteArray(
                                 API.Encoding.ByteArrayToString(bytes)
                             ), crypto);

            Type = API.Encoding.ByteArrayToString(raw.Slice(0, 3));
            MsgId = API.Encoding.ByteArrayToString(raw.Slice(3, 4));

            Data = raw.Slice(7, raw.Length - 7);
        }

        public string GetResponse(UserProfile profile)
        {
            string next_msg_id = API.Util.GetRandomHexString(4);

            profile.next_msg_id = next_msg_id;

            string pong = "res;" + this.MsgId + ";" + next_msg_id;

            byte[] pongEncrypted = API.TripleDES.Encrypt(API.Encoding.StringToByteArray(pong), profile.crypto);

            return API.Encoding.ByteArrayToHexString(pongEncrypted);
        }

        public byte[] GetRepeat(UserProfile profile)
        {
            byte[] prefix = API.Encoding.StringToByteArray("msg;" + profile.userid + ";");
            byte[] rtn = new byte[prefix.Length + Data.Length];
            prefix.CopyTo(rtn, 0);
            Data.CopyTo(rtn, prefix.Length);

            return rtn;
        }
    }
}
