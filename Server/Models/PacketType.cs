using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Models
{
    static class PacketType
    {
        public const string REQ_ClientInfo = "ClientInfo";
        public const string REQ_OnlineClientInfos = "OnlineClientInfos";

        public const string REQ_P2PRequest = "P2PRequest";
        public const string REQ_P2PSpecifiedClient = "P2PSpecifiedClient";

        public const string REQ_P2PEstablished = "P2PEstablished";
        public const string REQ_P2PFailed = "P2PFailed"; 
    }
}
