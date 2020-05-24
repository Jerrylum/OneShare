
namespace OneShare
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using EmbedIO;
    using EmbedIO.Files;
    using EmbedIO.WebApi;
    using OneShare.API;
    using OneShare.Module;

    public class Program
    {
        public static OptionsForm OptionsForm;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        [STAThread]
        public static void Main(string[] args)
        {
            API.RSA.Init();
            API.Server.StartServer();


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(OptionsForm = new OptionsForm());

        }


    }
}
