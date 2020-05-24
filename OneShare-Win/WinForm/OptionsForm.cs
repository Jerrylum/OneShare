using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneShare.API
{
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();


            pictureBox1.Refresh();

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            string rawip = API.Util.GetLocalIPAddress();
            ushort port = API.Server.WebpagePort;


            string finalHostName;
            string finalUrl;

            if (rawip == null)
            {
                finalHostName = "localhost";
            } else
            {
                finalHostName = rawip;
            }

            finalUrl = "http://" + finalHostName + ":" + port;
            linkLabel1.Text = finalHostName + ":" + port;

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(finalUrl, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(8);
            e.Graphics.DrawImage(qrCodeImage, -18, -18);
        }

        public void UpdateServerState()
        {
            pictureBox1.Refresh();
            ServerStatusCB.Text = API.Server.ExternalServer.State.ToString();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string link = "http://" + linkLabel1.Text;

            var browser = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo(link) { UseShellExecute = true }
            };
            browser.Start();
        }

        private void ClipboardCB_CheckedChanged(object sender, EventArgs e)
        {
            API.Keyboard.EnableClipboard = ClipboardCB.Checked;
        }

        private void AutoTypingCB_CheckedChanged(object sender, EventArgs e)
        {
            API.Keyboard.EnableAutoTyping = AutoTypingCB.Checked;
        }

        private void ServerStatusCB_CheckedChanged(object sender, EventArgs e)
        {
            ServerStatusCB.Text = "Loading";
            if (ServerStatusCB.Checked)
            {
                API.Server.StartServer();
            } else
            {
                API.Server.StopServer();
            }
        }
    }
}
