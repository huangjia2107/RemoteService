using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Config
{
    [Serializable]
    class ServerConfig
    {
        public string IP { get; set; }
        public int Port { get; set; }
    }
}
