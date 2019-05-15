using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Models;
using NetworkCommsDotNet.Connections;
using System.Net;
using NetworkCommsDotNet.Connections.TCP;
using NetworkCommsDotNet;
using ClientCore.Interface;

namespace ClientCore
{
    public partial class OneServerCore
    {
        public void RequestP2PConnection(string targetGuid)
        {
            if (_mainConnection != null && _mainConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
            {
                _isP2PSource = true;
                InnerRequestP2PConnection(targetGuid);
            }
        }

        public void RefreshOnlieClients()
        {
            if (_mainConnection != null && _mainConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
            {
                ServerMessageReceivedAction("Refresh online client info list");
                _mainConnection.SendObject<string>(PacketType.REQ_OnlineClientInfos, LocalClientInfo.Guid);
            }
        }

        private void SendLocalClientInfo()
        {
            ServerMessageReceivedAction("Established with Main server, send client info");
            _mainConnection.SendObject<ClientInfo>(PacketType.REQ_ClientInfo, LocalClientInfo);
        }

        private void InnerRequestP2PConnection(string targetGuid)
        {
            var targetClient = _clientInfoList.FirstOrDefault(client => client.Guid == targetGuid);
            if (targetClient == null)
            {
                ServerMessageReceivedAction(string.Format("Not found client named {0}", targetClient.Name));
                return;
            }

            var ipEndPoint = (IPEndPoint)_mainConnection.ConnectionInfo.LocalEndPoint;
            var listenings = Connection.StartListening(ConnectionType.TCP, ipEndPoint);

            if (listenings != null && listenings.Count > 0 && listenings[0].IsListening)
            {
                ServerMessageReceivedAction(string.Format("Start P2P connection listening on {0}:{1}", ipEndPoint.Address, ipEndPoint.Port));

                _p2pListener = listenings[0];
                _p2pListener.AppendIncomingPacketHandler<string>(PacketType.REQ_P2PEstablished, HandleP2PEstablished);

                ServerMessageReceivedAction("Request P2P connection with " + targetClient.Name);
                _mainConnection.SendObject<P2PRequest>(PacketType.REQ_P2PRequest, new P2PRequest { SourceGuid = LocalClientInfo.Guid, TargetGuid = targetGuid });
                return;
            }

            ServerMessageReceivedAction(string.Format("Fail P2P connection listening on {0}:{1}", ipEndPoint.Address, ipEndPoint.Port));
        }
    }
}
