using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkCommsDotNet.Connections;
using System.Net;
using System.Diagnostics;
using Server.Models;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections.TCP;
using ClientCore.Interface;

namespace ClientCore
{
    public partial class TwoServerCore
    {
        private void HandleP2PEstablished(PacketHeader header, Connection connection, string guid)
        {
            var clientInfo = _clientInfoList.FirstOrDefault(client => client.Guid == guid);
            var ipEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;

            P2PMessageReceivedAction(string.Format("Test P2P connection from {0}:{1}({2})", ipEndPoint.Address, ipEndPoint.Port, clientInfo.Name));

            if (_p2pListener != null)
            {
                //test P2P connection
                connection.SendObject<string>(PacketType.REQ_P2PEstablished, LocalClientInfo.Guid);
            }
            else
            {
                //for show in server
                _mainConnection.SendObject<P2PRequest>(PacketType.REQ_P2PEstablished, new P2PRequest { SourceGuid = LocalClientInfo.Guid, TargetGuid = guid });
            }
        }
    }
}
