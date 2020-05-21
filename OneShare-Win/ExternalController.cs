using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using HttpMultipartParser;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace OneShare
{
    public class ExternalController : WebApiController
    {
        [Route(HttpVerbs.Post, "/input")]
        public async Task<string> PostInput()
        {

            var parser = await MultipartFormDataParser.ParseAsync(Request.InputStream);
            var content = parser.GetParameterValue("content");

            API.Keyboard.SendString(content);

            return "200 - OK";
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

            string helloRaw = "hello " + userid;

            string helloEncrypted = API.Encoding.ByteArrayToHexString(
                                        API.TripleDES.Encrypt(
                                            API.Encoding.StringToByteArray(helloRaw),
                                            crypto)
                                        );


            // ----------------------------------------

            API.TripleDES.UserDict.Add(userid, crypto);

            await HttpContext.SendStringAsync(helloEncrypted, "text/plain", Encoding.UTF8);
            return;
        }


        [Route(HttpVerbs.Post, "/test")]
        public async Task<string> PostTest()
        {

            var parser = await MultipartFormDataParser.ParseAsync(Request.InputStream);
            var all = parser.Files;

            if (all.Count != 1) throw HttpException.BadRequest();

            Stream sourceStream = all[0].Data;



            byte[] Bytes = API.Encoding.ReadFully(sourceStream);

            Console.WriteLine(API.Encoding.ByteArrayToHexString(Bytes));



            return "200 - OK";
        }


        [Route(HttpVerbs.Get, "/public-key")]
        public async Task GetKey()
        {
            await HttpContext.SendStringAsync($"const PUBLICKEY = '{API.RSA.PublicKey}'", "text/javascript", Encoding.UTF8);
            return;
        }
    }
}