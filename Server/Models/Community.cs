using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Models
{
    class ClientInfo
    {
        public string Guid { get; set; }
        public string Name { get; set; }

        public bool CanAccess { get; set; }
    } 

    class P2PRequest
    {
        public string FromGuid { get; set; }
        public string ToGuid { get; set; }
    }

    class P2PClient
    {
        public string GUID { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
    }
}
