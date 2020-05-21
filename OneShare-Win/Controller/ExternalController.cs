using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using HttpMultipartParser;
using OneShare.Object;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace OneShare
{
    public class ExternalController : WebApiController
    {
        [Route(HttpVerbs.Post, "/input")]
        public async Task PostInput()
        {
            var parser = await MultipartFormDataParser.ParseAsync(Request.InputStream);
            var user_id = parser.GetParameterValue("user_id");

            var all = parser.Files;
            if (all.Count != 1)
                throw HttpException.BadRequest();

            UserProfile profile;
            if (!API.TripleDES.UserDict.TryGetValue(user_id, out profile))
                throw HttpException.BadRequest();

            // ----------------------------------------

            var crypto = profile.crypto;
            MsgRequest req = new MsgRequest(crypto, all[0].Data);


            if (req.MsgId != profile.next_msg_id)
                throw HttpException.BadRequest();

            // ----------------------------------------

            if (req.Type == "str")
            {
                string content = API.Encoding.ByteArrayToString(req.Data);

                API.Keyboard.SendString(content);
            }

            // ----------------------------------------

            string next_msg_id = API.Util.GetRandomHexString(4);

            profile.next_msg_id = next_msg_id;

            string pong = req.MsgId + ";" + next_msg_id;

            string pongEncrypted = API.Encoding.ByteArrayToHexString(
                                       API.TripleDES.Encrypt(
                                           API.Encoding.StringToByteArray(pong),
                                           crypto)
                                       );

            await HttpContext.SendStringAsync(pongEncrypted, "text/plain", Encoding.UTF8);
            return;
        }

        [Route(HttpVerbs.Post, "/register")]
        public async Task PostRegister()
        {

            var parser = await MultipartFormDataParser.ParseAsync(Request.InputStream);
            var all = parser.Files;

            // ----------------------------------------

            if (all.Count != 1)
                throw HttpException.BadRequest();

            byte[] bytes = API.RSA.RSADecrypt(API.Encoding.ReadFully(all[0].Data));
            string[] splitted = Encoding.ASCII.GetString(bytes).Split(';');

            string userid = splitted[0], key = splitted[1], iv = splitted[2];

            // ----------------------------------------

            if (userid.Length != 16 || key.Length != 32 || iv.Length != 16)
                throw HttpException.BadRequest();

            var crypto = API.TripleDES.createDESCrypto(key, iv);

            string next_msg_id = API.Util.GetRandomHexString(4);

            string helloRaw = userid + ";" + next_msg_id;

            string helloEncrypted = API.Encoding.ByteArrayToHexString(
                                        API.TripleDES.Encrypt(
                                            API.Encoding.StringToByteArray(helloRaw),
                                            crypto)
                                        );


            // ----------------------------------------


            API.TripleDES.UserDict.Add(userid, new UserProfile() { userid = userid, crypto = crypto, next_msg_id = next_msg_id });

            await HttpContext.SendStringAsync(helloEncrypted, "text/plain", Encoding.UTF8);
            return;
        }

        [Route(HttpVerbs.Get, "/public-key")]
        public async Task GetKey()
        {
            await HttpContext.SendStringAsync($"const PUBLICKEY = '{API.RSA.PublicKey}'", "text/javascript", Encoding.UTF8);
            return;
        }
    }
}