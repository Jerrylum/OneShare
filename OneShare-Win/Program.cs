
namespace OneShare
{
    using System;
    using System.Text;
    using System.Threading;
    using EmbedIO;
    using EmbedIO.Files;
    using EmbedIO.WebApi;


    public class Program
    {
        private const string InternalUrl = "http://localhost:8080/";
        private const string ExternalUrl = "http://+:8888/";


        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            API.RSA.Init();

            //var t = TripleDES.createDESCrypto("0123456789012345", "00000000");


            //byte[] plain = Encoding.ASCII.GetBytes("Hello World");
            //byte[] en = TripleDES.Encrypt(plain, t);
            //byte[] de = TripleDES.Decrypt(en, t);
            //Console.WriteLine(API.ByteArrayToHexString(en));
            //Console.WriteLine(API.ByteArrayToString(de));

            var internalServer = new WebServer(o => o
                   .WithUrlPrefix(InternalUrl)
                   .WithMode(HttpListenerMode.EmbedIO))
               .WithLocalSessionManager()
               .WithStaticFolder("/", "internal", true);
            ServerBasicSetup(internalServer);
            internalServer.RunAsync();

            var externalServer = new WebServer(o => o
                   .WithUrlPrefix(ExternalUrl)
                   .WithMode(HttpListenerMode.EmbedIO))
               .WithLocalSessionManager()
               .WithWebApi("/api", m => m.WithController<ExternalController>())
               .WithStaticFolder("/", "external", false); // TODO set false
            ServerBasicSetup(externalServer);
            externalServer.RunAsync();

            //var browser = new System.Diagnostics.Process()
            //{
            //    StartInfo = new System.Diagnostics.ProcessStartInfo(InternalUrl) { UseShellExecute = true }
            //};
            //browser.Start();

            Console.ReadKey(true);

        }

        private static void ServerBasicSetup(WebServer server)
        {
            server.HandleHttpException(async (ctx, ex) =>
            {
                ctx.Response.StatusCode = ex.StatusCode;

                switch (ex.StatusCode)
                {
                    case 404:
                        await ctx.SendStringAsync("404 Not Found", "text/html", Encoding.UTF8);
                        break;
                    default:
                        // Handle other HTTP Status codes or call the default handler 'SendStandardHtmlAsync'
                        await ctx.SendStandardHtmlAsync(ex.StatusCode);
                        break;
                }
            });
        }

    }
}
