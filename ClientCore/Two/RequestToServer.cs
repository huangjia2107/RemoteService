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
using NetworkCommsDotNet.Connections.UDP;

namespace ClientCore
{
    public partial class TwoServerCore
    {
        public void RequestP2PConnection(string targetGuid)
        {
            if (_mainConnection != null && _mainConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
            {
                _isP2PSource = true;
                _targetGuid = targetGuid;

                RequestTempConnectionToServer();
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

        private void RequestTempConnectionToServer()
        {
            ServerMessageReceivedAction("Start connection to P2P server");

            _tempConnection = UDPConnection.GetConnection(new ConnectionInfo(_serverConfig.IP, _serverConfig.P2P_Port), UDPOptions.None);
            _tempConnection.AppendIncomingPacketHandler<string>(PacketType.REQ_P2PEstablished, HandleP2PEstablished);

            if (_tempConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
            {
                ServerMessageReceivedAction("Send UDP Info to P2P server");
                _tempConnection.SendObject<string>(PacketType.REQ_UDPInfo, LocalClientInfo.Guid);
            }
            else
                _tempConnection = null;
        } 
    }
}
