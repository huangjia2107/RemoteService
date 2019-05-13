using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Models;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections.TCP;
using System.Net;

namespace ClientCore
{
    public partial class OneServerCore
    {
        private const string ServerIP = "127.0.0.1";
        private const int ServerPort = 6666;
        private const int ServerP2PPort = 8888;

        public Action<IEnumerable<ClientInfo>> ClientInfoListChangedAction { get; set; }
        public Action<string> ServerMessageReceivedAction { get; set; }
        public Action<string> P2PMessageReceivedAction { get; set; }

        private readonly List<ClientInfo> _clientInfoList = null;

        public ClientInfo LocalClientInfo { get; private set; }

        //long connection
        private Connection _mainConnection = null;

        //final P2P connection
        private Connection _p2pConnection = null;
        //local listening for P2P connection
        private ConnectionListenerBase _p2pListener = null;

        //if current client is the originator of the P2P connection 
        private bool _isP2PSource = false;

        public OneServerCore()
        {
            _clientInfoList = new List<ClientInfo>();
            LocalClientInfo = new ClientInfo { Guid = Guid.NewGuid().ToString(), Name = "Client" + DateTime.Now.ToString("fff"), CanAccess = true };
        }

        public void Start()
        {
            ServerMessageReceivedAction("Start connection to Main server");

            NetworkComms.AppendGlobalConnectionCloseHandler(HandleConnectionShutdown);

            _mainConnection = TCPConnection.GetConnection(new ConnectionInfo(ServerIP, ServerPort));
            if (_mainConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
            {
                _mainConnection.AppendIncomingPacketHandler<ClientInfo[]>(PacketType.REQ_OnlineClientInfos, HandleOnlineClientInfos);
                _mainConnection.AppendIncomingPacketHandler<P2PClient>(PacketType.REQ_P2PSpecifiedClient, HandleP2PSpecifiedClient);
                _mainConnection.AppendIncomingPacketHandler<string>(PacketType.REQ_P2PFailed, HandleP2PFailed);

                SendLocalClientInfo();
            }
            else
            {
                NetworkComms.RemoveGlobalConnectionCloseHandler(HandleConnectionShutdown);
                _mainConnection = null;
            }
        }

        private void StopP2PListening()
        {
            //stop local listening
            if (_p2pListener != null && _p2pListener.IsListening)
            {
                var ipEndPoint = (IPEndPoint)_p2pListener.LocalListenEndPoint;
                ServerMessageReceivedAction(string.Format("Stop local P2P connection listening on {0}:{1}", ipEndPoint.Address, ipEndPoint.Port));

                _p2pListener.RemoveIncomingPacketHandler();
                Connection.StopListening(_p2pListener);
                _p2pListener = null;
            }
        } 
    }
}
