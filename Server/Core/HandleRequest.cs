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

            if (localEndPoint.Port == _serverConfig.Port)
                ConsoleHelper.Mark(string.Format("[ Main ] {0}:{1} has been successfully connected", remoteEndPoint.Address, remoteEndPoint.Port));
            else
                ConsoleHelper.Mark(string.Format("[ P2P  ] {0}:{1} has been successfully connected", remoteEndPoint.Address, remoteEndPoint.Port));
        }

        private void HandleClientInfo(PacketHeader header, Connection connection, ClientInfo clientInfo)
        {
            if (connection == null || clientInfo == null)
                return;

            var ipEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;
            ConsoleHelper.Info(string.Format("[ Main ] Received client info, IPPort = {0}:{1}, Name = {2}, CanAccess = {3}", ipEndPoint.Address, ipEndPoint.Port, clientInfo.Name, clientInfo.CanAccess));

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
                ConsoleHelper.Info("[ Main ] Request online client info list, Not found guid = " + fromGuid);
                return;
            }

            var ipEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;
            ConsoleHelper.Info(string.Format("[ Main ] {0}:{1}({2}) request online client info list", ipEndPoint.Address, ipEndPoint.Port, clientSession.Name));

            //Send online client list to all client
            SendOnlineClients(clientSession);
        }

        private void HandleP2PRequest(PacketHeader header, Connection connection, P2PRequest p2pRequest)
        {
            if (connection == null || p2pRequest == null)
                return;

            var sourceClient = _clientSessionList.FirstOrDefault(cs => cs.Guid == p2pRequest.SourceGuid);
            if (sourceClient == null)
                return;

            var targetClient = _clientSessionList.FirstOrDefault(cs => cs.Guid == p2pRequest.TargetGuid);
            if (targetClient == null)
                return;

            var sourceIPPort = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;
            var targetIPPort = (IPEndPoint)targetClient.Connection.ConnectionInfo.RemoteEndPoint;

            ConsoleHelper.Info(string.Format("[ P2P  ] {0}:{1}({2}) request P2P connection with {3}{4}({5})",
                sourceIPPort.Address, sourceIPPort.Port, sourceClient.Name,
                targetIPPort.Address, targetIPPort.Port, targetClient.Name));

            //close the temp connection between P2P Server and A/B
            if (((IPEndPoint)connection.ConnectionInfo.LocalEndPoint).Port != _serverConfig.Port)
                connection.CloseConnection(false);

            targetClient.Connection.SendObject<P2PClient>(PacketType.REQ_P2PSpecifiedClient, new P2PClient { GUID = p2pRequest.SourceGuid, IP = sourceIPPort.Address.ToString(), Port = sourceIPPort.Port });
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

            ConsoleHelper.Info(string.Format("[ Main  ] {0} Established P2P connection with {1}", sourceClient.Name, targetClient.Name));
        }

        private void HandleP2PFailed(PacketHeader header, Connection connection, P2PRequest p2pRequest)
        {
            if (connection == null || p2pRequest == null)
                return;

            var sourceClient = _clientSessionList.FirstOrDefault(cs => cs.Guid == p2pRequest.SourceGuid);
            if (sourceClient == null)
                return;

            var targetClient = _clientSessionList.FirstOrDefault(cs => cs.Guid == p2pRequest.TargetGuid);
            if (targetClient == null)
                return;

            ConsoleHelper.Info(string.Format("[ Main  ] {0} failed P2P connection with {1}", sourceClient.Name, targetClient.Name));

            targetClient.Connection.SendObject<string>(PacketType.REQ_P2PFailed, sourceClient.Guid);
        }

        private void HandleConnectionClose(Connection connection)
        {
            if (connection == null)
                return;

            var localEndPoint = (IPEndPoint)connection.ConnectionInfo.LocalEndPoint;
            var remoteEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;

            if (localEndPoint.Port == _serverConfig.Port)
            {
                _clientSessionList.RemoveAll(client => client.Connection.ConnectionInfo.RemoteEndPoint == remoteEndPoint);
                SendOnlineClients();

                ConsoleHelper.Warn(string.Format("[ Main ] {0}:{1} has been disconnected, current online {2}", remoteEndPoint.Address, remoteEndPoint.Port, _clientSessionList.Count));
            }
            else
                ConsoleHelper.Warn(string.Format("[ P2P  ] {0}:{1} has been disconnected", remoteEndPoint.Address, remoteEndPoint.Port, _clientSessionList.Count));
        }
    }
}
