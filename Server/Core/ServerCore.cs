using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkCommsDotNet;
using Server.Utils;
using Server.Config;
using Server.Models;
using System.Net;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.TCP;
using NetworkCommsDotNet.Connections.UDP;

namespace Server.Core
{
    partial class ServerCore
    {
        private ServerConfig _serverConfig = null;
        private readonly List<ClientSession> _clientSessionList = null;

        public ServerCore(ServerConfig serverConfig)
        {
            _serverConfig = serverConfig;
            _clientSessionList = new List<ClientSession>();
        }

        public void Start()
        {
            ConfigGlobalConnectionHandler();
            StartListening();
        }

        #region Private

        private void ConfigGlobalConnectionHandler()
        {
            NetworkComms.AppendGlobalConnectionEstablishHandler(HandleConnectionEstablish);

            //send self client info
            NetworkComms.AppendGlobalIncomingPacketHandler<ClientInfo>(PacketType.REQ_ClientInfo, HandleClientInfo);

            //request online client infos
            NetworkComms.AppendGlobalIncomingPacketHandler<string>(PacketType.REQ_OnlineClientInfos, HandleOnlineClientInfos);

            //request NAT info
            NetworkComms.AppendGlobalIncomingPacketHandler<string>(PacketType.REQ_NATInfo, HandleNATInfo);

            //request NAT info
            NetworkComms.AppendGlobalIncomingPacketHandler<P2PRequest>(PacketType.REQ_UDPP2PRequest, HandleUDPP2PRequest); 

            //feedback established p2p connection with specified client
            NetworkComms.AppendGlobalIncomingPacketHandler<P2PRequest>(PacketType.REQ_P2PEstablished, HandleP2PEstablished); 

            NetworkComms.AppendGlobalConnectionCloseHandler(HandleConnectionClose);
        }

        private void StartListening()
        {
            var listenings = new List<ConnectionListenerBase>
            {
                new TCPConnectionListener(NetworkComms.DefaultSendReceiveOptions, ApplicationLayerProtocolStatus.Enabled),
                new UDPConnectionListener(NetworkComms.DefaultSendReceiveOptions, ApplicationLayerProtocolStatus.Enabled, UDPOptions.None),
                new UDPConnectionListener(NetworkComms.DefaultSendReceiveOptions, ApplicationLayerProtocolStatus.Enabled, UDPOptions.None),
            };

            var ipEndPoints = new List<IPEndPoint>
            {
                new IPEndPoint(IPAddress.Parse(_serverConfig.IP), _serverConfig.Port),
                new IPEndPoint(IPAddress.Parse(_serverConfig.IP), _serverConfig.P2P_Port),
                new IPEndPoint(IPAddress.Parse(_serverConfig.IP), _serverConfig.Test_Port),
            };

            Connection.StartListening<IPEndPoint>(listenings, ipEndPoints, false);
            foreach (var listening in listenings)
            {
                if (listening.IsListening)
                {
                    var ipEndPoint = (IPEndPoint)listening.LocalListenEndPoint;
                    if (listening.ConnectionType == ConnectionType.TCP)
                        ConsoleHelper.Info(string.Format("[ TCP ] Start Main server listening on {0}:{1}", ipEndPoint.Address, ipEndPoint.Port));
                    else
                        ConsoleHelper.Info(string.Format("[ UDP ] Start P2P server listening on {0}:{1}", ipEndPoint.Address, ipEndPoint.Port));
                }
            }
        }

        private ClientSession ExistClient(EndPoint remoteEndPoint)
        {
            return _clientSessionList.FirstOrDefault(client => client.Connection.ConnectionInfo.RemoteEndPoint == remoteEndPoint);
        }

        #endregion
    }
}
