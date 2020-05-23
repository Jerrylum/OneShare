using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using HttpMultipartParser;
using OneShare.Object;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace OneShare
{
    public class ExternalController : WebApiController
    {
        [Route(HttpVerbs.Get, "/public-key")]
        public async Task GetKey()
        {
            await HttpContext.SendStringAsync($"const PUBLICKEY = '{API.RSA.PublicKey}'", "text/javascript", Encoding.UTF8);
            return;
        }
    }
}