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

        private void HandleClientInfoList(PacketHeader header, Connection connection, ClientInfo[] clientInfos)
        {
            if (connection == null || clientInfos == null || clientInfos.Length == 0)
                return;

            _clientInfoList.Clear();
            _clientInfoList.AddRange(clientInfos);
        }

        //P2PClient.GUID want to p2p by P2PClient.IP and P2PClient.Port.
        private void HandleP2PPublicClient(PacketHeader header, Connection connection, P2PClient p2pClient)
        {
            if (connection == null || p2pClient == null)
                return;

            //NAT-traversal
            _p2pConnection = TCPConnection.GetConnection(new ConnectionInfo(p2pClient.IP, p2pClient.Port));
            if (_p2pConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
            {
                _p2pConnection.SendObject<string>(PacketType.REQ_P2PConnection, "Hello P2P");
            }
            else
            {
                _p2pConnection.CloseConnection(false);
                _p2pConnection.Dispose();

                _p2pConnection = TCPConnection.GetConnection(new ConnectionInfo(ServerIP, ServerPort));
                _p2pConnection.AppendIncomingPacketHandler<string>(PacketType.REQ_ConnectionEstablished, (d, conn, c) => HandleP2PConnectionEstablished(conn, p2pClient.GUID));
            }
        }

        private void HandleConnectionClose(Connection connection)
        {
            if (connection == null)
                return;

            var ipEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;
            Trace.TraceWarning(string.Format("{0}:{1} has been disconnected", ipEndPoint.Address, ipEndPoint.Port));
        }
    }
}
