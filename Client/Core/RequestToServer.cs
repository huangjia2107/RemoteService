using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Models;
using NetworkCommsDotNet.Connections;
using System.Net;
using NetworkCommsDotNet.Connections.TCP;
using NetworkCommsDotNet;

namespace Client.Core
{
    partial class ClientCore
    {
        public void SendLocalClientInfo()
        {
            _mainConnection.SendObject<ClientInfo>(PacketType.REQ_ClientInfo, _localClientInfo);
        }

        public void RequestP2P(string guid)
        {
            //P2P connection
            _p2pConnection = TCPConnection.GetConnection(new ConnectionInfo(ServerIP, ServerPort));
            _p2pConnection.AppendIncomingPacketHandler<string>(PacketType.REQ_ConnectionEstablished, (header, conn, msg) => HandleP2PConnectionEstablished(conn, guid));
        }
    }
}
