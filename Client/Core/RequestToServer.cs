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
            _mainConnection.SendObject<ClientInfo>(PacketType.REQ_ClientInfo, _localClientInfo);
        }

        private void RequestTempConnectionToServer(string targetGuid)
        {
            _tempConnection = TCPConnection.GetConnection(new ConnectionInfo(ServerIP, ServerPort));
            _tempConnection.AppendIncomingPacketHandler<string>(PacketType.REQ_ConnectionEstablished, (header, conn, msg) => HandleTempConnectionEstablished(conn, targetGuid));
            _tempConnection.AppendShutdownHandler(HandleConnectionShutdown);
        }

        public void RequestP2PConnection(string targetGuid)
        {
            RequestTempConnectionToServer(targetGuid);
        }
    }
}
