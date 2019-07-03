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
        private void UDPP2PConnected(string fromGuid, IPEndPoint ipEndPoint)
        {
            var clientInfo = _clientInfoList.FirstOrDefault(clientEx => clientEx.Client.Guid == fromGuid);
            P2PMessageReceivedAction(string.Format("Connected with {0}({1}:{2})", clientInfo.Client.Name, ipEndPoint.Address, ipEndPoint.Port));

            clientInfo.IP = ipEndPoint.Address.ToString();
            clientInfo.Port = ipEndPoint.Port;
            clientInfo.Established = true;

            if (!_udpTraversal.IsSource)
            {
                //notify p2p source
                _udpTraversal.Connect(LocalClientInfo.Client.Guid, ipEndPoint.Address, ipEndPoint.Port);
            }
            else
            {
                //for show in server
                _mainConnection.SendObject<P2PRequest>(PacketType.REQ_P2PEstablished, new P2PRequest { SourceGuid = LocalClientInfo.Client.Guid, TargetGuid = fromGuid });
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
