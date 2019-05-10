using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Models;
using System.Net;
using Server.Utils;
using NetworkCommsDotNet.Connections;

namespace Server.Core
{
    partial class ServerCore
    {
        private void SyncSharedClients(ClientSession clientSession)
        {
            //Send ClientShare list to client
            var clientShares = _clientSessionList.Where(cs => cs.Guid != clientSession.Guid)
                                                 .Select(cs => new ClientInfo { Guid = cs.Guid, Name = cs.Name, CanAccess = cs.CanAccess })
                                                 .ToArray();

            if (clientShares != null && clientShares.Length > 0)
            {
                var ipPort = (IPEndPoint)clientSession.Connection.ConnectionInfo.RemoteEndPoint;
                ConsoleHelper.Info(string.Format("Sync shared client info to {0}:{1}({2})", ipPort.Address, ipPort.Port, clientSession.Name));

                clientSession.Connection.SendObject<ClientInfo[]>(PacketType.REQ_SyncClientInfoList, clientShares);
            }
        }

        private void SyncSharedClients()
        {
            if (_clientSessionList.Count < 2)
                return;

            _clientSessionList.ForEach(cs => SyncSharedClients(cs));
        } 
    }
}
