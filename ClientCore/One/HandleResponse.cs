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

namespace ClientCore
{
    public partial class OneServerCore
    {
        private void HandleOnlineClientInfos(PacketHeader header, Connection connection, ClientInfo[] clientInfos)
        {
            ServerMessageReceivedAction("Received online client info list, Count = " + clientInfos.Length);

            _clientInfoList.Clear();
            _clientInfoList.AddRange(clientInfos);

            ClientInfoListChangedAction(clientInfos.Where(c => c.CanAccess));
        }

        //P2PClient.GUID request P2P connection by P2PClient.IP and P2PClient.Port.
        private void HandleP2PSpecifiedClient(PacketHeader header, Connection connection, P2PClient p2pSourceClient)
        {
            var sourceClient = _clientInfoList.FirstOrDefault(client => client.Guid == p2pSourceClient.GUID);
            ServerMessageReceivedAction(string.Format("{0}:{1}({2}) request P2P connection with him", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));

            //stop local listening
            StopP2PListening();

            ServerMessageReceivedAction(string.Format("Start P2P connection with {0}:{1}({2})", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));

            //P2P to specified client
            _p2pConnection = TCPConnection.GetConnection(new ConnectionInfo(p2pSourceClient.IP, p2pSourceClient.Port));
            _p2pConnection.AppendIncomingPacketHandler<string>(PacketType.REQ_P2PEstablished, HandleP2PEstablished);

            if (_p2pConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
            {
                ServerMessageReceivedAction(string.Format("Established P2P connection with {0}:{1}({2})", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));

                //for show in server
                connection.SendObject<P2PRequest>(PacketType.REQ_P2PEstablished, new P2PRequest { SourceGuid = LocalClientInfo.Guid, TargetGuid = p2pSourceClient.GUID });

                //test P2P connection
                _p2pConnection.SendObject<string>(PacketType.REQ_P2PEstablished, LocalClientInfo.Guid);
            }
            else
            {
                //reset the failed P2P connection between A and B
                _p2pConnection = null;

                if (_isP2PSource)
                {
                    ServerMessageReceivedAction(string.Format("Fail P2P connection with {0}:{1}({2}) and quit P2P", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));
                    _isP2PSource = false;

                    connection.SendObject<P2PRequest>(PacketType.REQ_P2PFailed, new P2PRequest { SourceGuid = LocalClientInfo.Guid, TargetGuid = p2pSourceClient.GUID });
                    return;
                }
                else
                {
                    ServerMessageReceivedAction(string.Format("Fail P2P connection with {0}:{1}({2}), let him try", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));
                }

                InnerRequestP2PConnection(p2pSourceClient.GUID);
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
            if (remoteEndPoint.Port == ServerP2PPort)
                ServerMessageReceivedAction("Disconnected with P2P server");
            else if (remoteEndPoint.Port == ServerPort)
                ServerMessageReceivedAction("Disconnected with Main server");
            else
                P2PMessageReceivedAction("P2P connection is disconnected");

            connection.RemoveIncomingPacketHandler();
            connection.Dispose();
            connection = null;

            if (_mainConnection == null && _p2pListener == null && _p2pListener == null)
                NetworkComms.RemoveGlobalConnectionCloseHandler(HandleConnectionShutdown);
        }
    }
}
