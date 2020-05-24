using EmbedIO;
using EmbedIO.WebApi;
using OneShare.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneShare.API
{
    public static class Server
    {
        private static ushort _WebpagePort = 8888;

        public static ushort WebpagePort
        {
            get => _WebpagePort;
            set => _WebpagePort = Math.Min((ushort)65535, value);
        }
        public static ushort WebSocketPort { get => (ushort)(_WebpagePort + 1); }

        private static string ExternalUrl { get => $"http://+:{WebpagePort}/"; }
        private static string ExternalWebSocketUrl { get => $"http://+:{WebSocketPort}/"; }


        public static WebServer ExternalServer = null;
        public static WebServer ExternalWebSocketServer = null;
        public static WebSocketInputModule ExternalWebsocketInputModule = null;

        public static void StartServer()
        {
            StopServer();

            if (Program.OptionsForm != null)
                WebpagePort = (ushort)Program.OptionsForm.PortNum.Value;

            ExternalServer = new WebServer(o => o
                   .WithUrlPrefix(ExternalUrl)
                   .WithMode(HttpListenerMode.EmbedIO))
               .WithLocalSessionManager()
               .WithWebApi("/api", m => m.WithController<ExternalController>())
               .WithStaticFolder("/", "external", false); // Add static files after other modules to avoid conflicts
            ServerBasicSetup(ExternalServer);
            ExternalServer.StateChanged += (sender, e) =>
            {
                if (Program.OptionsForm != null)
                {
                    Program.OptionsForm.UpdateServerState();
                }
            };
            ExternalServer.RunAsync();

            ExternalWebSocketServer = new WebServer(o => o
                   .WithUrlPrefix(ExternalWebSocketUrl)
                   .WithMode(HttpListenerMode.EmbedIO))
               .WithLocalSessionManager()
               .WithModule(ExternalWebsocketInputModule = new WebSocketInputModule("/default"));
            ServerBasicSetup(ExternalWebSocketServer);
            ExternalWebSocketServer.RunAsync();

        }

        public static void StopServer()
        {
            if (ExternalServer != null)
                ExternalServer.Dispose();

            if (ExternalWebSocketServer != null)
                ExternalWebSocketServer.Dispose();

            if (Program.OptionsForm != null)
                Program.OptionsForm.ServerStatusCB.Text = "Stopped";
        }

        private static void ServerBasicSetup(WebServer server)
        {
            server.HandleHttpException(async (ctx, ex) =>
            {
                ctx.Response.StatusCode = ex.StatusCode;

                switch (ex.StatusCode)
                {
                    case 404:
                        await ctx.SendStringAsync("404 Not Found", "text/html", System.Text.Encoding.UTF8);
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
