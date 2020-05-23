using EmbedIO.WebSockets;
using OneShare.API;
using OneShare.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OneShare.Module
{

    public class WebSocketInputModule : WebSocketModule
    {
        public WebSocketInputModule(string urlPath)
            : base(urlPath, true)
        {
            // placeholder
        }

        /// <inheritdoc />
        protected override Task OnMessageReceivedAsync(IWebSocketContext context,
                                                       byte[] rxBuffer,
                                                       IWebSocketReceiveResult rxResult)
        {
            var user_id = context.HttpContextId;
            TripleDESCryptoServiceProvider crypto;

            UserProfile profile;
            if (API.TripleDES.UserDict.TryGetValue(user_id, out profile))
            {
                // ----------------------------------------

                crypto = profile.crypto;
                MsgRequest req = new MsgRequest(crypto, rxBuffer);


                if (req.MsgId != profile.next_msg_id)
                    return Task.CompletedTask;

                // ----------------------------------------

                if (req.Type == "str")
                {
                    string content = API.Encoding.ByteArrayToString(req.Data);

                    API.Keyboard.SendString(content);
                }

                // ----------------------------------------


                string pongEncrypted = req.GetResponse(profile);


                SendToOthersAsync(context, req.GetRepeat(profile));

                return SendAsync(context, pongEncrypted);
            }
            else
            {
                // ----------------------------------------

                byte[] bytes = API.RSA.RSADecrypt(rxBuffer);
                string[] splitted = API.Encoding.ByteArrayToString(bytes).Split(';');

                string type = splitted[0], key = splitted[1], iv = splitted[2];

                // ----------------------------------------

                if (type != "reg" || key.Length != 32 || iv.Length != 16)
                    return Task.CompletedTask;

                crypto = API.TripleDES.createDESCrypto(key, iv);

                string next_msg_id = API.Util.GetRandomHexString(4);

                string helloRaw = "acc;" + user_id + ";" + next_msg_id;

                string helloEncrypted = API.Encoding.ByteArrayToHexString(
                                            API.TripleDES.Encrypt(
                                                API.Encoding.StringToByteArray(helloRaw),
                                                crypto)
                                            );


                API.TripleDES.UserDict.Add(user_id, new UserProfile() { userid = user_id, crypto = crypto, next_msg_id = next_msg_id });

                return SendAsync(context, helloEncrypted);
            }
        }

        private void SendToOthersAsync(IWebSocketContext sender, byte[] payload)
        {
            foreach (var recipient in ActiveContexts)
            {
                if (sender != recipient)
                {
                    UserProfile profile;
                    if (API.TripleDES.UserDict.TryGetValue(recipient.HttpContextId, out profile))
                    {
                        byte[] encrypted = API.TripleDES.Encrypt(payload, profile.crypto);
                        SendAsync(recipient, API.Encoding.ByteArrayToHexString(encrypted));
                    }
                }
            }


        }

    }
}
