using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkCommsDotNet.Connections;
using System.Net;
using System.Diagnostics;
using Server.Models;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections.TCP;

namespace Client.Core
{
    partial class ClientCore
    {
        private void HandleP2PConnectionEstablished(Connection connection, string guid)
        {
            var listenings = Connection.StartListening(ConnectionType.TCP, connection.ConnectionInfo.LocalEndPoint);
            if (listenings != null && listenings.Count > 0 && listenings[0].IsListening)
            {
                Trace.TraceInformation(string.Format("Start listening on {0}", _p2pConnection.ConnectionInfo.LocalEndPoint));
                listenings[0].AppendIncomingPacketHandler<string>(PacketType.REQ_P2PConnection, (h, c, m) =>
                    {
                        Trace.TraceInformation(m);
                    });


                connection.SendObject<P2PRequest>(PacketType.REQ_P2PRequest, new P2PRequest { FromGuid = _localClientInfo.Guid, ToGuid = guid });

                return;
            }

            Trace.TraceError(string.Format("Fail listening on {0}", _p2pConnection.ConnectionInfo.LocalEndPoint));

            // _p2pConnection.SendObject<P2PRequest>(PacketType.REQ_P2PRequest, new P2PRequest { FromGuid = _localClientInfo.Guid, ToGuid = _clientInfoList[0].Guid });
        }


    }
}
