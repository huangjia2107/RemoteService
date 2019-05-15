using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Config
{
    [Serializable]
    public class ServerConfig
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public int P2P_Port { get; set; }
    }
}
