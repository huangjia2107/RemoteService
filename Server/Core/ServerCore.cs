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

            //finish client info
            NetworkComms.AppendGlobalIncomingPacketHandler<ClientInfo>(PacketType.REQ_ClientInfo, HandleClientInfo);

            //request p2p connection with specified client
            NetworkComms.AppendGlobalIncomingPacketHandler<P2PRequest>(PacketType.REQ_P2PRequest, HandleP2PRequest);

            NetworkComms.AppendGlobalConnectionCloseHandler(HandleConnectionClose);
        }

        private void StartListening()
        {
            var listenings = new List<ConnectionListenerBase>
            {
                new TCPConnectionListener(NetworkComms.DefaultSendReceiveOptions, ApplicationLayerProtocolStatus.Enabled),
                new TCPConnectionListener(NetworkComms.DefaultSendReceiveOptions, ApplicationLayerProtocolStatus.Enabled)
            };

            var ipEndPoints = new List<IPEndPoint>
            {
                new IPEndPoint(IPAddress.Parse(_serverConfig.IP), _serverConfig.Port),
                new IPEndPoint(IPAddress.Parse(_serverConfig.IP), _serverConfig.P2P_Port)
            };

            Connection.StartListening<IPEndPoint>(listenings, ipEndPoints, false);

            //var listenings = Connection.StartListening(ConnectionType.TCP, new IPEndPoint(IPAddress.Parse(_serverConfig.IP), _serverConfig.Port));

            foreach(var listening in listenings)
            {
                if(listening.IsListening)
                {
                    var ipEndPoint = (IPEndPoint)listening.LocalListenEndPoint;
                    ConsoleHelper.Info(string.Format("Start listening on {0}:{1}", ipEndPoint.Address, ipEndPoint.Port));
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
