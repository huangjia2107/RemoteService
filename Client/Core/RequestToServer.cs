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
        private void SendLocalClientInfo()
        {
            ServerMessageReceivedAction("Send self info and request online client info list");
            _mainConnection.SendObject<ClientInfo>(PacketType.REQ_ClientInfo, LocalClientInfo);
        }

        private void RequestTempConnectionToServer(string targetGuid)
        {
            ServerMessageReceivedAction("Start temp connection to server for P2P"); 

            _tempConnection = TCPConnection.GetConnection(new ConnectionInfo(ServerIP, ServerP2PPort));
            _tempConnection.AppendIncomingPacketHandler<string>(PacketType.REQ_ConnectionEstablished, (header, conn, msg) => HandleTempConnectionEstablished(conn, targetGuid));
            _tempConnection.AppendShutdownHandler(HandleConnectionShutdown);
        }

        public void RequestP2PConnection(string targetGuid)
        {
            _isP2PSource = true;
            RequestTempConnectionToServer(targetGuid);
        }

       
    }
}
