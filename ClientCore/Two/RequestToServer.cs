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
using ClientCore.Interface;
using NetworkCommsDotNet.Connections.UDP;
using ClientCore.Utils;

namespace ClientCore
{
    public partial class TwoServerCore
    {
        public void TestNAT()
        {
            if (_mainConnection != null && _mainConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
            {
                if (_udpConnection == null)
                    _udpConnection = CreateLocalUDPConnection();

                SendToIPEndPoint(PacketType.REQ_NATInfo, LocalClientInfo.Client.Guid, IPAddress.Parse(_serverConfig.IP), _serverConfig.P2P_Port);
                SendToIPEndPoint(PacketType.REQ_NATInfo, LocalClientInfo.Client.Guid, IPAddress.Parse(_serverConfig.IP), _serverConfig.Test_Port);
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

            if (_udpConnection == null)
                _udpConnection = CreateLocalUDPConnection();

            P2PMessageReceivedAction(string.Format("[ {0} ]: {1}", "Local", message));

            SendToIPEndPoint(PacketType.REQ_P2PMessage, message, IPAddress.Parse(targetClient.IP), targetClient.Port);
        }

        public bool ShareScreenshot(IPAddress ip, int port, Screenshot screenshot)
        {
            if (_mainConnection == null || _mainConnection.ConnectionInfo.ConnectionState != ConnectionState.Established || screenshot == null)
                return false;

            if (_udpConnection == null)
                _udpConnection = CreateLocalUDPConnection();

            return SendToIPEndPoint(PacketType.REQ_P2PScreenshot, screenshot, ip, port);
        }

        public void RequestP2PConnection(string targetGuid)
        {
            if (_mainConnection != null && _mainConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
            {
                _isP2PSource = true;
                _targetGuid = targetGuid;

                if (_udpConnection == null)
                    _udpConnection = CreateLocalUDPConnection();

                if (_udpConnection != null)
                {
                    UploadUDPInfo();
                }
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

        private void UploadUDPInfo()
        {
            ServerMessageReceivedAction("Upload UDP Info to P2P server");

            SendToIPEndPoint(PacketType.REQ_UDPInfo, LocalClientInfo.Client.Guid, IPAddress.Parse(_serverConfig.IP), _serverConfig.P2P_Port);
        }

        private UDPConnection CreateLocalUDPConnection()
        {
            try
            {
                var ip = ConnectionHelper.LocalIP();
                var port = ConnectionHelper.AvailablePort(2000);

                ServerMessageReceivedAction(string.Format("Create local UDP connection, IP = {0}, Port = {1}", ip, port));

                var connectionInfo = new ConnectionInfo(ConnectionType.UDP, ShortGuid.NewGuid(), new IPEndPoint(ip, port), true);
                var connection = UDPConnection.GetConnection(connectionInfo, UDPOptions.None);
                //connection.AppendIncomingPacketHandler<string>(PacketType.REQ_P2PEstablished, HandleP2PEstablished);

                return connection;
            }
            catch (Exception ex)
            {
                ServerMessageReceivedAction(ex.Message + ex.StackTrace);
            }

            return null;

        }

        private bool SendToIPEndPoint<T>(string packetType, T message, IPAddress ip, int port)
        {
            try
            {
                _udpConnection.SendObject<T>(packetType, message, new IPEndPoint(ip, port));
            }
            catch (Exception ex)
            {
                ServerMessageReceivedAction(ex.Message + ex.StackTrace);
                return false;
            }

            return true;
        }

        private bool MultiholePunching(IPAddress targetIP, int targetPort, int startPort, int tryTimes)
        {
            var ports = Enumerable.Range(startPort, 65535 - startPort + 1).ToArray();
            CommonHelper.Shuffle(ports);

            int i = 0;
            while (i < Math.Min(tryTimes, ports.Length))
            {
                var port = ports[i];
                if (port != targetPort)
                {
                    if (!SendToIPEndPoint(PacketType.REQ_P2PEstablished, LocalClientInfo.Client.Guid, targetIP, port))
                        return false;

                    System.Threading.Thread.Sleep(50);
                }

                i++;
            }

            ServerMessageReceivedAction("Send P2P over");
            return true;
        }
    }
}
