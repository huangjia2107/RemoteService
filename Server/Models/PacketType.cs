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

        public const string REQ_NATInfo = "NATInfo"; 

        public const string REQ_UDPP2PRequest = "UDP_P2P_Request";
        public const string REQ_UDPP2PConnect = "UDP_P2P_Connect";

        public const string REQ_P2PEstablished = "P2PEstablished"; 

        public const string REQ_P2PMessage = "P2PMessage";
        public const string REQ_P2PScreenshot = "P2PScreenshot";
    }
}
