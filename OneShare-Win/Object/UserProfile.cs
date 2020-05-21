using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OneShare.Object
{
    public class UserProfile
    {
        public string userid;
        public TripleDESCryptoServiceProvider crypto;
        public string next_msg_id;
    }
}
