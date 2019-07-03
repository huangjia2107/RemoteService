using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkCommsDotNet.Connections;
using System.Net;
using Server.Models;
using Server.Utils;
using NetworkCommsDotNet;

namespace Server.Core
{
    partial class ServerCore
    {
        private void HandleConnectionEstablish(Connection connection)
        {
            if (connection == null)
                return;

            var localEndPoint = (IPEndPoint)connection.ConnectionInfo.LocalEndPoint;
            var remoteEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;

            if (connection.ConnectionInfo.ConnectionType == ConnectionType.TCP)
                ConsoleHelper.Mark(string.Format("[ TCP ] {0}:{1} has been successfully connected", remoteEndPoint.Address, remoteEndPoint.Port));
            //             else
            //                 ConsoleHelper.Mark(string.Format("[ UDP ] {0}:{1} has been successfully connected", remoteEndPoint.Address, remoteEndPoint.Port));
        }

        private void HandleClientInfo(PacketHeader header, Connection connection, ClientInfo clientInfo)
        {
            if (connection == null || clientInfo == null)
                return;

            var ipEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;
            ConsoleHelper.Info(string.Format("[ TCP ] Received client info, IPPort = {0}:{1}, Name = {2}, CanAccess = {3}", ipEndPoint.Address, ipEndPoint.Port, clientInfo.Name, clientInfo.CanAccess));

            //Get ClientInfo
            var clientSession = ExistClient(ipEndPoint);
            if (clientSession != null)
            {
                clientSession.Guid = clientInfo.Guid;
                clientSession.Name = clientInfo.Name;
                clientSession.CanAccess = clientInfo.CanAccess;
            }
            else
            {
                clientSession = new ClientSession
                {
                    Connection = connection,

                    Guid = clientInfo.Guid,
                    Name = clientInfo.Name,
                    CanAccess = clientInfo.CanAccess,
                };

                _clientSessionList.Add(clientSession);
            }

            //Send online client list to all client
            SendOnlineClients();
        }

        private void HandleOnlineClientInfos(PacketHeader header, Connection connection, string fromGuid)
        {
            if (connection == null || string.IsNullOrEmpty(fromGuid))
                return;

            var clientSession = _clientSessionList.FirstOrDefault(cs => cs.Guid == fromGuid);
            if (clientSession == null)
            {
                ConsoleHelper.Info("[ TCP ] Request online client info list, Not found guid = " + fromGuid);
                return;
            }

            var ipEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;
            ConsoleHelper.Info(string.Format("[ TCP ] {0}:{1}({2}) request online client info list", ipEndPoint.Address, ipEndPoint.Port, clientSession.Name));

            //Send online client list to all client
            SendOnlineClients(clientSession);
        }

        private void HandleNATInfo(PacketHeader header, Connection connection, string fromGuid)
        {
            if (connection == null || string.IsNullOrEmpty(fromGuid))
                return;

            var clientSession = _clientSessionList.FirstOrDefault(cs => cs.Guid == fromGuid);
            if (clientSession == null)
            {
                ConsoleHelper.Info("[ UDP ] Report UDP is Established, Not found guid = " + fromGuid);
                return;
            }

            var ipEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;

            ConsoleHelper.Mark(string.Format("[ UDP ] {0} request test NAT, Result = {1}:{2}", clientSession.Name, ipEndPoint.Address, ipEndPoint.Port));
            clientSession.Connection.SendObject<string>(PacketType.REQ_NATInfo, string.Format("UDP NAT Info = {0}:{1}", ipEndPoint.Address, ipEndPoint.Port));
        }

        private void HandleUDPP2PRequest(PacketHeader header, Connection connection, P2PRequest p2pRequest)
        {
            if (connection == null || p2pRequest == null)
                return;

            var sourceClient = _clientSessionList.FirstOrDefault(cs => cs.Guid == p2pRequest.SourceGuid);
            if (sourceClient == null)
                return;

            var targetClient = _clientSessionList.FirstOrDefault(cs => cs.Guid == p2pRequest.TargetGuid);
            if (targetClient == null)
                return;

            //udp ip/port
            var sourceUDPIPPort = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;

            //tcp ip/port
            var sourceTCPIPPort = (IPEndPoint)sourceClient.Connection.ConnectionInfo.RemoteEndPoint;
            var targetTCPIPPort = (IPEndPoint)targetClient.Connection.ConnectionInfo.RemoteEndPoint;

            ConsoleHelper.Info(string.Format("[ {0} ] {1}({2}:{3}) request {4}({5}:{6}) P2P to {7}:{8}",
                connection.ConnectionInfo.ConnectionType,
                sourceClient.Name, sourceTCPIPPort.Address, sourceTCPIPPort.Port,
                targetClient.Name, targetTCPIPPort.Address, targetTCPIPPort.Port,
                sourceUDPIPPort.Address, sourceUDPIPPort.Port));

            targetClient.Connection.SendObject<P2PClient>("UDP_P2P_Request", new P2PClient { GUID = p2pRequest.SourceGuid, IP = sourceUDPIPPort.Address.ToString(), Port = sourceUDPIPPort.Port });
        } 

        private void HandleP2PEstablished(PacketHeader header, Connection connection, P2PRequest p2pRequest)
        {
            if (connection == null || p2pRequest == null)
                return;

            var sourceClient = _clientSessionList.FirstOrDefault(cs => cs.Guid == p2pRequest.SourceGuid);
            if (sourceClient == null)
                return;

            var targetClient = _clientSessionList.FirstOrDefault(cs => cs.Guid == p2pRequest.TargetGuid);
            if (targetClient == null)
                return;

            ConsoleHelper.Info(string.Format("[ TCP ] {0} Established P2P connection with {1}", sourceClient.Name, targetClient.Name));
        } 

        private void HandleConnectionClose(Connection connection)
        {
            if (connection == null)
                return;

            var remoteEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;

            if (connection.ConnectionInfo.ConnectionType == ConnectionType.TCP)
            {
                _clientSessionList.RemoveAll(client => client.Connection.ConnectionInfo.RemoteEndPoint == remoteEndPoint);
                SendOnlineClients();

                ConsoleHelper.Warn(string.Format("[ TCP ] {0}:{1} has been disconnected, current online {2}", remoteEndPoint.Address, remoteEndPoint.Port, _clientSessionList.Count));
            }
            else
                ConsoleHelper.Warn(string.Format("[ UDP ] {0}:{1} has been disconnected", remoteEndPoint.Address, remoteEndPoint.Port, _clientSessionList.Count));
        }
    }
}
