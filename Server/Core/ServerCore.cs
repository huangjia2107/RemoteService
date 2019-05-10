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

        #region Privare

        private void ConfigGlobalConnectionHandler()
        {
            NetworkComms.AppendGlobalConnectionEstablishHandler(conn => HandleConnectionEstablish(conn));

            //finish client info
            NetworkComms.AppendGlobalIncomingPacketHandler<ClientInfo>(PacketType.REQ_ClientInfo, (header, conn, clientInfo) => HandleClientInfo(header, conn, clientInfo));

            //request p2p connection with specified client
            NetworkComms.AppendGlobalIncomingPacketHandler<P2PRequest>(PacketType.REQ_P2PRequest, (header, conn, p2pClient) => HandleP2PRequest(header, conn, p2pClient));

            NetworkComms.AppendGlobalConnectionCloseHandler(conn => HandleConnectionClose(conn));
        }

        private void StartListening()
        {
            var listenings = Connection.StartListening(ConnectionType.TCP, new IPEndPoint(IPAddress.Parse(_serverConfig.IP), _serverConfig.Port));

            if (listenings != null && listenings.Count > 0 && listenings[0].IsListening)
                ConsoleHelper.Info(string.Format("Start listening on {0}:{1}", _serverConfig.IP, _serverConfig.Port));
            else
                ConsoleHelper.Error(string.Format("Fail listening on {0}:{1}", _serverConfig.IP, _serverConfig.Port));
        }

        private ClientSession ExistClient(EndPoint remoteEndPoint)
        {
            return _clientSessionList.FirstOrDefault(client => client.Connection.ConnectionInfo.RemoteEndPoint == remoteEndPoint);
        }

        #endregion
    }
}
