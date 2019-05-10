using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkCommsDotNet.Connections;

namespace Server.Models
{
    class ClientSession : ClientInfo
    {
        public Connection Connection { get; set; } 
    }
}
