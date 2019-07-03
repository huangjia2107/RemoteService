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

            ServerMessageReceivedAction(string.Format("Local UDP Info = {0}, {1}", _udpTraversal.LocalEndPoint, message));
        }


        private void HandleUDPP2PRequest(PacketHeader header, Connection connection, P2PClient p2pClient)
        {
            var sourceClient = _clientInfoList.FirstOrDefault(clientEx => clientEx.Client.Guid == p2pClient.GUID);
            if (sourceClient == null)
                return;

            if (_udpTraversal.IsSource)
            {
                ServerMessageReceivedAction(string.Format("Connect to {0}({1}:{2})", sourceClient.Client.Name, p2pClient.IP, p2pClient.Port));
                _udpTraversal.Connect(LocalClientInfo.Client.Guid, IPAddress.Parse(p2pClient.IP), p2pClient.Port);
            }
            else
            {
                ServerMessageReceivedAction(string.Format("Punch to {0}({1}:{2})", sourceClient.Client.Name, p2pClient.IP, p2pClient.Port));
                if (_udpTraversal.TryPunch(IPAddress.Parse(p2pClient.IP), p2pClient.Port))
                {
                    ServerMessageReceivedAction(string.Format("Target request to {0}", sourceClient.Client.Name));
                    _udpTraversal.Request(new P2PRequest { SourceGuid = LocalClientInfo.Client.Guid, TargetGuid = p2pClient.GUID }, false);
                }
            }
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
                _udpTraversal.CleanUp();

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
