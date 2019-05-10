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

namespace Client.Core
{
    partial class ClientCore
    {
        private void HandleMainConnectionEstablished()
        {
            SendLocalClientInfo();
        }

        private void HandleClientInfoList(ClientInfo[] clientInfos)
        {
            _clientInfoList.Clear();
            _clientInfoList.AddRange(clientInfos);
        }

        private void HandleTempConnectionEstablished(Connection connection, string guid)
        {
            var listenings = Connection.StartListening(ConnectionType.TCP, connection.ConnectionInfo.LocalEndPoint);
            if (listenings != null && listenings.Count > 0 && listenings[0].IsListening)
            {
                Trace.TraceInformation(string.Format("Start listening on {0}", connection.ConnectionInfo.LocalEndPoint));

                _p2pListener = listenings[0];
                _p2pListener.AppendIncomingPacketHandler<string>(PacketType.REQ_P2PConnectionEstablished,  HandleP2PConnectionEstablished);

                connection.SendObject<P2PRequest>(PacketType.REQ_P2PRequest, new P2PRequest { FromGuid = _localClientInfo.Guid, ToGuid = guid });
                return;
            }

            Trace.TraceError(string.Format("Fail listening on {0}", _p2pConnection.ConnectionInfo.LocalEndPoint));
        }

        //P2PClient.GUID want to p2p by P2PClient.IP and P2PClient.Port.
        private void HandleP2PSpecifiedClient(PacketHeader header, Connection connection, P2PClient p2pClient)
        {
            //stop local listening
            if (_p2pListener != null && _p2pListener.IsListening)
            {
                _p2pListener.RemoveIncomingPacketHandler();
                Connection.StopListening(_p2pListener);
                _p2pListener = null;
            }

            //P2P to specified client
            _p2pConnection = TCPConnection.GetConnection(new ConnectionInfo(p2pClient.IP, p2pClient.Port));
            if (_p2pConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
            {
                _p2pConnection.SendObject<string>(PacketType.REQ_P2PConnectionEstablished, "Hello P2P");
            }
            else
            {
                //close the failed P2P connection between A and B
                _p2pConnection.CloseConnection(false);
                _p2pConnection.Dispose();
                _p2pConnection = null;

                RequestTempConnectionToServer(p2pClient.GUID);
            }
        }

        private void HandleConnectionShutdown(Connection connection)
        {
            if(connection != null)
            {
                connection.RemoveIncomingPacketHandler();
                connection.RemoveShutdownHandler(HandleConnectionShutdown);

                connection.Dispose();
                connection = null;
            }
        } 
    }
}
