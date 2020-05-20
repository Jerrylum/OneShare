using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using HttpMultipartParser;
using System;
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

            API.SendString(content);

            return "200 - OK";
        }
    }
}