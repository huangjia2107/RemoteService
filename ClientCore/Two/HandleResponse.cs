using System.Linq;
using System.Net;

using ClientCore.Models;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using Server.Models;

namespace ClientCore
{
    public partial class TwoServerCore
    {
        private void HandleOnlineClientInfos(PacketHeader header, Connection connection, ClientInfo[] clientInfos)
        {
            ServerMessageReceivedAction("Received online client info list, Count = " + clientInfos.Length);

            _clientInfoList.Clear();
            _clientInfoList.AddRange(clientInfos.Select(c => new ClientInfoEx(c)));

            ClientInfoListChangedAction(_clientInfoList.Where(c => c.Client.CanAccess));
        }

        private void HandleNATInfo(PacketHeader header, Connection connection, string message)
        {
            if (string.IsNullOrEmpty(message) || connection == null)
                return;

            ServerMessageReceivedAction(string.Format("Local UDP Info = {0}, {1}", _udpConnection.ConnectionInfo.LocalEndPoint, message));
        }

        private void HandleUDPInfo(PacketHeader header, Connection connection, string message)
        {
            if (string.IsNullOrEmpty(_targetGuid) || _udpConnection == null)
                return;

            ServerMessageReceivedAction("I am ready, request P2P, " + message);
            connection.SendObject<P2PRequest>(PacketType.REQ_P2PRequest, new P2PRequest { SourceGuid = LocalClientInfo.Client.Guid, TargetGuid = _targetGuid });
            return;
        }

        //P2PClient.GUID request P2P connection by P2PClient.IP and P2PClient.Port.
        private void HandleP2PSpecifiedClient(PacketHeader header, Connection connection, P2PClient p2pSourceClient)
        {
            var sourceClient = _clientInfoList.FirstOrDefault(clientEx => clientEx.Client.Guid == p2pSourceClient.GUID);
            ServerMessageReceivedAction(string.Format("{0}:{1}({2}) request P2P", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Client.Name));

            if (!_isP2PSource)
            {
                if (_udpConnection == null)
                    _udpConnection = CreateLocalUDPConnection();

                if (_udpConnection != null)
                {
                    ServerMessageReceivedAction(string.Format("Send P2P try string to {0}:{1}({2})", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Client.Name));

                    /*
                    var ttl = _udpConnection.Ttl;
                    _udpConnection.Ttl = 3;

                    if (!MultiholePunching(IPAddress.Parse(p2pSourceClient.IP), p2pSourceClient.Port, p2pSourceClient.Port, 2000))
                    {
                        _udpConnection.Ttl = ttl;
                        return;
                    }
                    */
                    SendToIPEndPoint(PacketType.REQ_P2PEstablished, LocalClientInfo.Client.Guid, IPAddress.Parse(p2pSourceClient.IP), p2pSourceClient.Port);

                    _targetGuid = p2pSourceClient.GUID;
                    UploadUDPInfo();
                }
            }
            else
            {
                ServerMessageReceivedAction(string.Format("Try P2P to {0}:{1}({2})", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Client.Name));

                //MultiholePunching(IPAddress.Parse(p2pSourceClient.IP), p2pSourceClient.Port, 1025, 2000);
                SendToIPEndPoint(PacketType.REQ_P2PEstablished, LocalClientInfo.Client.Guid, IPAddress.Parse(p2pSourceClient.IP), p2pSourceClient.Port);

                return;
            }
        }

        private void HandleP2PFailed(PacketHeader header, Connection connection, string targetGuid)
        {
            var targetClient = _clientInfoList.FirstOrDefault(clientEx => clientEx.Client.Guid == targetGuid);
            ServerMessageReceivedAction(string.Format("Fail P2P connection with {0} and quit P2P", targetClient.Client.Name));

            _isP2PSource = false;
            _targetGuid = null;
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

            if (_mainConnection == null)
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
