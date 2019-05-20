using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkCommsDotNet.Connections;
using System.Net;
using System.Diagnostics;
using NetworkCommsDotNet;
using Server.Models;
using NetworkCommsDotNet.Connections.TCP;
using NetworkCommsDotNet.Connections.UDP;

namespace ClientCore
{
    public partial class TwoServerCore
    {
        private void HandleOnlineClientInfos(PacketHeader header, Connection connection, ClientInfo[] clientInfos)
        {
            ServerMessageReceivedAction("Received online client info list, Count = " + clientInfos.Length);

            _clientInfoList.Clear();
            _clientInfoList.AddRange(clientInfos);

            ClientInfoListChangedAction(clientInfos.Where(c => c.CanAccess));
        }

        private void HandleNATInfo(PacketHeader header, Connection connection, string message)
        {
            if (string.IsNullOrEmpty(message) || connection == null)
                return;

            ServerMessageReceivedAction(message);
        }

        private void HandleUDPInfo(PacketHeader header, Connection connection, string message)
        {
            if (string.IsNullOrEmpty(_targetGuid) || _udpConnection == null)
                return;

            ServerMessageReceivedAction("I am ready, request P2P, " + message);
            connection.SendObject<P2PRequest>(PacketType.REQ_P2PRequest, new P2PRequest { SourceGuid = LocalClientInfo.Guid, TargetGuid = _targetGuid });
            return;

            var ipEndPoint = (IPEndPoint)_udpConnection.ConnectionInfo.LocalEndPoint;
            //_tempConnection.CloseConnection(false);

            var listenings = Connection.StartListening(ConnectionType.UDP, ipEndPoint);
            if (listenings != null && listenings.Count > 0 && listenings[0].IsListening)
            {
                ServerMessageReceivedAction(string.Format("Start P2P listening on {0}:{1}", ipEndPoint.Address, ipEndPoint.Port));

                _p2pListener = listenings[0];
                _p2pListener.AppendIncomingPacketHandler<string>(PacketType.REQ_P2PEstablished, HandleP2PEstablished);

                connection.SendObject<P2PRequest>(PacketType.REQ_P2PRequest, new P2PRequest { SourceGuid = LocalClientInfo.Guid, TargetGuid = _targetGuid });
                return;
            }

            ServerMessageReceivedAction(string.Format("Fail P2P connection listening on {0}:{1}, and close temp connection with P2P server", ipEndPoint.Address, ipEndPoint.Port));
        }

        //P2PClient.GUID request P2P connection by P2PClient.IP and P2PClient.Port.
        private void HandleP2PSpecifiedClient(PacketHeader header, Connection connection, P2PClient p2pSourceClient)
        {
            var sourceClient = _clientInfoList.FirstOrDefault(client => client.Guid == p2pSourceClient.GUID);
            ServerMessageReceivedAction(string.Format("{0}:{1}({2}) request P2P", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));

            //stop local listening
            StopP2PListening();

            if (!_isP2PSource)
            {
                if (_udpConnection == null)
                    _udpConnection = CreateLocalUDPConnection();

                if (_udpConnection != null)
                {
                    ServerMessageReceivedAction(string.Format("Send P2P try string to {0}:{1}({2})", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));
                    SendToIPEndPoint(PacketType.REQ_P2PEstablished, LocalClientInfo.Guid, IPAddress.Parse(p2pSourceClient.IP), p2pSourceClient.Port);

                    _targetGuid = p2pSourceClient.GUID;
                    UploadUDPInfo();
                }
            }
            else
            {
                ServerMessageReceivedAction(string.Format("Try P2P to {0}:{1}({2})", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));
                SendToIPEndPoint(PacketType.REQ_P2PEstablished, LocalClientInfo.Guid, IPAddress.Parse(p2pSourceClient.IP), p2pSourceClient.Port);

                return;

                _p2pConnection = UDPConnection.GetConnection(new ConnectionInfo(p2pSourceClient.IP, p2pSourceClient.Port), UDPOptions.None);
                _p2pConnection.AppendIncomingPacketHandler<string>(PacketType.REQ_P2PEstablished, HandleP2PEstablished);

                if (_p2pConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
                {
                    ServerMessageReceivedAction(string.Format("Established P2P connection with {0}:{1}({2})", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));

                    _p2pConnection.SendObject<string>(PacketType.REQ_P2PEstablished, LocalClientInfo.Guid);
                }
                else
                {
                    _isP2PSource = false;
                    _targetGuid = null;
                    _p2pConnection = null;

                    ServerMessageReceivedAction(string.Format("Fail P2P connection with {0}:{1}({2}) and quit P2P", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));
                    connection.SendObject<P2PRequest>(PacketType.REQ_P2PFailed, new P2PRequest { SourceGuid = LocalClientInfo.Guid, TargetGuid = p2pSourceClient.GUID });
                    return;
                }
            }
        }

        private void HandleP2PFailed(PacketHeader header, Connection connection, string targetGuid)
        {
            var targetClient = _clientInfoList.FirstOrDefault(client => client.Guid == targetGuid);
            ServerMessageReceivedAction(string.Format("Fail P2P connection with {0} and quit P2P", targetClient.Name));

            //stop local listening
            StopP2PListening();
        }

        private void HandleConnectionShutdown(Connection connection)
        {
            if (connection == null)
                return;

            var remoteEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;
            if (remoteEndPoint.Port == _serverConfig.P2P_Port)
            {
                ServerMessageReceivedAction("Disconnected with P2P server");
                DisposeConnection(connection);
            }
            else if (remoteEndPoint.Port == _serverConfig.Port)
            {
                ServerMessageReceivedAction("Disconnected with Main server");
                DisposeConnection(connection);
            }
            else
            {
                _isP2PSource = false;
                _targetGuid = null;

                P2PMessageReceivedAction("P2P connection is disconnected");
                DisposeConnection(connection);
            }

            if (_mainConnection == null && _udpConnection == null && _p2pListener == null && _p2pConnection == null)
                NetworkComms.RemoveGlobalConnectionCloseHandler(HandleConnectionShutdown);
        }

        private void DisposeConnection(Connection connection)
        {
            if (connection == null)
                return;

            connection.RemoveIncomingPacketHandler();
            connection.Dispose();
            connection = null;
        }
    }
}
