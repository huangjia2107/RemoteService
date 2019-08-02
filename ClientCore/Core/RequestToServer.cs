using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Models;
using NetworkCommsDotNet.Connections;
using System.Net;
using NetworkCommsDotNet.Connections.TCP;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Tools;
using NetworkCommsDotNet.Connections.UDP;
using ClientCore.Utils;

namespace ClientCore
{
    public partial class MainClient
    {
        public void TestUDP1()
        {
            if (_mainConnection != null && _mainConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
            {
                _udpTraversal.Send(PacketType.REQ_NATInfo, LocalClientInfo.Client.Guid, IPAddress.Parse(_serverConfig.IP), _serverConfig.P2P_Port);
            }
        }

        public void TestUDP2()
        {
            if (_mainConnection != null && _mainConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
            {
                _udpTraversal.Send(PacketType.REQ_NATInfo, LocalClientInfo.Client.Guid, IPAddress.Parse(_serverConfig.IP), _serverConfig.Test_Port);
            }
        }

        public void Send(string targetGuid, string message)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(message.Trim())
                || _mainConnection == null || _mainConnection.ConnectionInfo.ConnectionState != ConnectionState.Established)
                return;

            var targetClient = _clientInfoList.FirstOrDefault(c => c.Client.Guid == targetGuid && c.Established);
            if (targetClient == null)
                return;

            P2PMessageReceivedAction(string.Format("[ {0} ]: {1}", "Local", message));

            _udpTraversal.Send(PacketType.REQ_P2PMessage, message, IPAddress.Parse(targetClient.IP), targetClient.Port);
        }

        public bool ShareScreenshot(IPAddress ip, int port, Screenshot screenshot)
        {
            if (_mainConnection == null || _mainConnection.ConnectionInfo.ConnectionState != ConnectionState.Established || screenshot == null)
                return false;

            return _udpTraversal.Send(PacketType.REQ_P2PScreenshot, screenshot, ip, port);
        }

        public void RequestP2PConnection(string targetGuid)
        {
            if (_mainConnection != null && _mainConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
            {
                var targetClient = _clientInfoList.FirstOrDefault(clientEx => clientEx.Client.Guid == targetGuid);
                if (targetClient == null)
                    return;

                ServerMessageReceivedAction(string.Format("Source request to {0}", targetClient.Client.Name));
                _udpTraversal.Request(new P2PRequest { SourceGuid = LocalClientInfo.Client.Guid, TargetGuid = targetGuid });
            }
        }

        public void RefreshOnlieClients()
        {
            if (_mainConnection != null && _mainConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
            {
                ServerMessageReceivedAction("Refresh online client info list");
                _mainConnection.SendObject<string>(PacketType.REQ_OnlineClientInfos, LocalClientInfo.Client.Guid);
            }
        }

        private void SendLocalClientInfo()
        {
            ServerMessageReceivedAction("Established with Main server, send client info");
            _mainConnection.SendObject<ClientInfo>(PacketType.REQ_ClientInfo, LocalClientInfo.Client);
        }
    }
}
