using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Models;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.TCP;
using NetworkCommsDotNet;

namespace Client.Core
{
    partial class ClientCore
    {
        private const string ServerIP = "127.0.0.1";
        private const int ServerPort = 6666;


        private readonly List<ClientInfo> _clientInfoList = null;

        private ClientInfo _localClientInfo = null;

        private Connection _mainConnection = null;
        private Connection _p2pConnection = null;
        private Connection _tempConnection = null;

        private ConnectionListenerBase _p2pListener = null;

        public ClientCore()
        {
            _clientInfoList = new List<ClientInfo>();
            _localClientInfo = new ClientInfo { Guid = Guid.NewGuid().ToString(), Name = "Client" + DateTime.Now.ToString("fff"), CanAccess = true };
        }

        public void Start()
        {
            _mainConnection = TCPConnection.GetConnection(new ConnectionInfo(ServerIP, ServerPort));

            _mainConnection.AppendIncomingPacketHandler<string>(PacketType.REQ_ConnectionEstablished, (header, conn, p2pClient) => HandleMainConnectionEstablished());
            _mainConnection.AppendIncomingPacketHandler<ClientInfo[]>(PacketType.REQ_SyncClientInfoList, (header, conn, clientInfos) => HandleClientInfoList(clientInfos));
            _mainConnection.AppendIncomingPacketHandler<P2PClient>(PacketType.REQ_P2PSpecifiedClient, (header, conn, p2pClient) => HandleP2PSpecifiedClient(header, conn, p2pClient));
            _mainConnection.AppendShutdownHandler(HandleConnectionShutdown);
        } 
    }
}
