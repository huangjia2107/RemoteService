using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Models
{
    static class PacketType
    {
        public const string REQ_ConnectionEstablished = "ConnectionEstablished";

        public const string REQ_ClientInfo = "ClientInfo";
        public const string REQ_SyncClientInfoList = "SyncClientInfoList";

        public const string REQ_P2PRequest = "P2PRequest";
        public const string REQ_P2PSpecifiedClient = "P2PSpecifiedClient";
        public const string REQ_P2PLocalClient = "P2PLocalClient";

        public const string REQ_P2PReady = "P2PReady";

        public const string REQ_P2PStart = "P2PStart";



        public const string REQ_P2PConnectionEstablished = "P2PConnectionEstablished";
    }
}
