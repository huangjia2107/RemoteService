﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkCommsDotNet.Connections;
using System.Net;
using System.Diagnostics;
using Server.Models;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections.TCP;

namespace Client.Core
{
    partial class ClientCore
    {
        private void HandleP2PConnectionEstablished(PacketHeader header, Connection connection, string message)
        { 
            var ipEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;
            P2PMessageReceivedAction(string.Format("P2P connection is established with {0}:{1}", ipEndPoint.Address, ipEndPoint.Port)); 
        }
    }
}
