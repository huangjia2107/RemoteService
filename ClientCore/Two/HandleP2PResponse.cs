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
using ClientCore.Interface;

namespace ClientCore
{
    public partial class TwoServerCore
    {
        private void HandleP2PEstablished(PacketHeader header, Connection connection, string guid)
        {
            var clientInfo = _clientInfoList.FirstOrDefault(clientEx => clientEx.Client.Guid == guid);
            var ipEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;

            clientInfo.IP = ipEndPoint.Address.ToString();
            clientInfo.Port = ipEndPoint.Port;

            clientInfo.Established = true;

            P2PMessageReceivedAction(string.Format("[ {0}({1}:{2}) ]: Test P2P connection", clientInfo.Client.Name, ipEndPoint.Address, ipEndPoint.Port));

            if (!_isP2PSource)
            {
                //test P2P connection
                connection.SendObject<string>(PacketType.REQ_P2PEstablished, LocalClientInfo.Client.Guid);
            }
            else
            {
                //for show in server
                _mainConnection.SendObject<P2PRequest>(PacketType.REQ_P2PEstablished, new P2PRequest { SourceGuid = LocalClientInfo.Client.Guid, TargetGuid = guid });
            }
        }

        private void HandleP2PMessage(PacketHeader header, Connection connection, string message)
        {
            var ipEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;
            var clientInfo = _clientInfoList.FirstOrDefault(clientEx => clientEx.IP == ipEndPoint.Address.ToString() && clientEx.Port == ipEndPoint.Port);

            if (clientInfo != null)
                P2PMessageReceivedAction(string.Format("[ {0}({1}:{2}) ]: {3}", clientInfo.Client.Name, ipEndPoint.Address, ipEndPoint.Port, message));
            else
                P2PMessageReceivedAction(string.Format("[ {0}({1}:{2}) ]: {3}", "UnKnown", ipEndPoint.Address, ipEndPoint.Port, message));
        }

        private void HandleP2PScreenshot(PacketHeader header, Connection connection, Screenshot screenshot)
        {
            if (ScreenshotReceivedAction != null && screenshot != null)
                ScreenshotReceivedAction(screenshot);
        }
    }
}
