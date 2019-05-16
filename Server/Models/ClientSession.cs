using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkCommsDotNet.Connections;
using System.Net;

namespace Server.Models
{
    class ClientSession : ClientInfo
    {
        public Connection Connection { get; set; }

        public IPEndPoint UDPEndPoint { get; set; }
    }
}
