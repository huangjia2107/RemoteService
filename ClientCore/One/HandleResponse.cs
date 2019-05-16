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
            ServerMessageReceivedAction(string.Format("3. {0}:{1}({2}) request P2P connection with him", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));

            //stop local listening
            StopP2PListening();

            ServerMessageReceivedAction(string.Format("4.Start P2P connection with {0}:{1}({2})", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));

            if (!_isP2PSource)
            {
                //UDPConnection.SendObject<string>("dddd", "TestTest", new IPEndPoint(IPAddress.Parse(p2pSourceClient.IP), p2pSourceClient.Port), NetworkComms.DefaultSendReceiveOptions, ApplicationLayerProtocolStatus.Enabled);

                UDPConnection.SendObject("dddd", "TestTest", p2pSourceClient.IP, p2pSourceClient.Port);
                InnerRequestP2PConnection(p2pSourceClient.GUID);
            }
            else
            {
                UDPConnection.SendObject<string>(PacketType.REQ_P2PEstablished, LocalClientInfo.Guid, new IPEndPoint(IPAddress.Parse(p2pSourceClient.IP), p2pSourceClient.Port), NetworkComms.DefaultSendReceiveOptions, ApplicationLayerProtocolStatus.Enabled);
            }

            //P2P to specified client
            /*
            _p2pConnection = UDPConnection.GetConnection(new ConnectionInfo(p2pSourceClient.IP, p2pSourceClient.Port), UDPOptions.None);
            _p2pConnection.AppendIncomingPacketHandler<string>(PacketType.REQ_P2PEstablished, HandleP2PEstablished);

            if (!_isP2PSource)
            {
                ServerMessageReceivedAction(string.Format("5.Send P2P connection try string to {0}:{1}({2})", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));

                //for show in server
                //connection.SendObject<P2PRequest>(PacketType.REQ_P2PEstablished, new P2PRequest { SourceGuid = LocalClientInfo.Guid, TargetGuid = p2pSourceClient.GUID });

                //test P2P connection
                _p2pConnection.SendObject<string>("TestTest", LocalClientInfo.Guid);

                InnerRequestP2PConnection(p2pSourceClient.GUID);
            }
            else
            {
                if (_p2pConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
                {
                    ServerMessageReceivedAction(string.Format("Established P2P connection with {0}:{1}({2})", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));

                    _p2pConnection.SendObject<string>(PacketType.REQ_P2PEstablished, LocalClientInfo.Guid);
                }
                else
                {
                    ServerMessageReceivedAction(string.Format("Fail P2P connection with {0}:{1}({2}) and quit P2P", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));
                    _isP2PSource = false;

                    connection.SendObject<P2PRequest>(PacketType.REQ_P2PFailed, new P2PRequest { SourceGuid = LocalClientInfo.Guid, TargetGuid = p2pSourceClient.GUID });
                }
            }*/
            //             else
            //             {
            //                 //reset the failed P2P connection between A and B
            //                 _p2pConnection = null;
            // 
            //                 if (_isP2PSource)
            //                 {
            //                     ServerMessageReceivedAction(string.Format("Fail P2P connection with {0}:{1}({2}) and quit P2P", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));
            //                     _isP2PSource = false;
            // 
            //                     connection.SendObject<P2PRequest>(PacketType.REQ_P2PFailed, new P2PRequest { SourceGuid = LocalClientInfo.Guid, TargetGuid = p2pSourceClient.GUID });
            //                     return;
            //                 }
            //                 else
            //                 {
            //                     ServerMessageReceivedAction(string.Format("Fail P2P connection with {0}:{1}({2}), let him try", p2pSourceClient.IP, p2pSourceClient.Port, sourceClient.Name));
            //                 }
            // 
            //                 InnerRequestP2PConnection(p2pSourceClient.GUID);
            //             }
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
                P2PMessageReceivedAction("P2P connection is disconnected");
                DisposeConnection(connection);
            }

            if (_mainConnection == null && _p2pListener == null && _p2pConnection == null)
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
