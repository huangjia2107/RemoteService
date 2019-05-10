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

            var ipEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;
            ConsoleHelper.Mark(string.Format("{0}:{1} has been successfully connected", ipEndPoint.Address, ipEndPoint.Port));

            connection.SendObject(PacketType.REQ_ConnectionEstablished);
        }

        private void HandleClientInfo(PacketHeader header, Connection connection, ClientInfo clientInfo)
        {
            if (connection == null || clientInfo == null)
                return;

            var ipEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;
            ConsoleHelper.Info(string.Format("{0}:{1} send client info, Name = {2}, CanAccess = {3}", ipEndPoint.Address, ipEndPoint.Port, clientInfo.Name, clientInfo.CanAccess));

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

            //Sync shared client list to all client
            SyncSharedClients();
        }

        private void HandleP2PRequest(PacketHeader header, Connection connection, P2PRequest p2pRequest)
        {
            if (connection == null || p2pRequest == null)
                return;

            var sourceClient = _clientSessionList.FirstOrDefault(cs => cs.Guid == p2pRequest.FromGuid);
            if (sourceClient == null)
                return;

            var targetClient = _clientSessionList.FirstOrDefault(cs => cs.Guid == p2pRequest.ToGuid);
            if (targetClient == null)
                return;

            var sourceIPPort = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;
            var targetIPPort = (IPEndPoint)targetClient.Connection.ConnectionInfo.RemoteEndPoint;

            ConsoleHelper.Info(string.Format("{0}:{1}({2}) request to p2p {3}{4}({5})",
                sourceIPPort.Address, sourceIPPort.Port, sourceClient.Name,
                targetIPPort.Address, targetIPPort.Port, targetClient.Name));

            //close the temp connection between Server and A/B
            connection.CloseConnection(false);  

            targetClient.Connection.SendObject<P2PClient>(PacketType.REQ_P2PSpecifiedClient, new P2PClient { GUID = p2pRequest.FromGuid, IP = sourceIPPort.Address.ToString(), Port = sourceIPPort.Port });
        }

        private void HandleConnectionClose(Connection connection)
        {
            if (connection == null)
                return;

            var ipEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;
            ConsoleHelper.Warn(string.Format("{0}:{1} has been disconnected", ipEndPoint.Address, ipEndPoint.Port));

            var clientInfo = ExistClient(ipEndPoint);
            if (clientInfo != null)
                _clientSessionList.Remove(clientInfo);

            //Sync shared client list to all client
            SyncSharedClients();
        }
    }
}
