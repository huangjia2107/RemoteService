﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Models;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections.TCP;
using System.Net;
using ClientCore.Interface;
using ClientCore.Config;
using Server.Config;

namespace ClientCore
{
    public partial class OneServerCore : IClientCore
    {
        public Action<IEnumerable<ClientInfo>> ClientInfoListChangedAction { get; set; }
        public Action<string> ServerMessageReceivedAction { get; set; }
        public Action<string> P2PMessageReceivedAction { get; set; }

        public ClientInfo LocalClientInfo { get; private set; }

        private readonly List<ClientInfo> _clientInfoList = null;
        private ServerConfig _serverConfig = null;

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
            _serverConfig = ConfigHelper<ServerConfig>.Instance().GetServerConfig();
            _clientInfoList = new List<ClientInfo>();
            LocalClientInfo = new ClientInfo { Guid = Guid.NewGuid().ToString(), Name = "Client" + DateTime.Now.ToString("fff"), CanAccess = true };
        }

        private bool ResolveDns()
        {
            try
            {
                var ipHostEntry = Dns.GetHostEntry(_serverConfig.Domain);
                if (ipHostEntry == null || ipHostEntry.AddressList.Length == 0)
                {
                    ServerMessageReceivedAction("Failed to resolve domain = " + _serverConfig.Domain);
                    return false;
                }

                ServerMessageReceivedAction("Resolved domain to IP = " + string.Join(" | ", ipHostEntry.AddressList.Select(addr => addr.ToString()).ToArray()));
                _serverConfig.IP = ipHostEntry.AddressList[ipHostEntry.AddressList.Length - 1].ToString();

                return true;
            }
            catch (Exception ex)
            {
                ServerMessageReceivedAction(string.Format("Failed to resolve domain = {0}, Error = {1}", _serverConfig.Domain, ex.Message));
                return false;
            }
        }

        public void Start()
        {
            if (!ResolveDns())
                return;

            ServerMessageReceivedAction("Start connection to Main server");

            try
            {
                _mainConnection = TCPConnection.GetConnection(new ConnectionInfo(_serverConfig.IP, _serverConfig.Port));
                if (_mainConnection.ConnectionInfo.ConnectionState == ConnectionState.Established)
                {
                    NetworkComms.AppendGlobalConnectionCloseHandler(HandleConnectionShutdown);

                    _mainConnection.AppendIncomingPacketHandler<ClientInfo[]>(PacketType.REQ_OnlineClientInfos, HandleOnlineClientInfos);
                    _mainConnection.AppendIncomingPacketHandler<P2PClient>(PacketType.REQ_P2PSpecifiedClient, HandleP2PSpecifiedClient);
                    _mainConnection.AppendIncomingPacketHandler<string>(PacketType.REQ_P2PFailed, HandleP2PFailed);

                    SendLocalClientInfo();
                }
                else
                {
                    ServerMessageReceivedAction("Failed connect to Main server");

                    NetworkComms.RemoveGlobalConnectionCloseHandler(HandleConnectionShutdown);
                    _mainConnection = null;
                }
            }
            catch (Exception ex)
            {
                ServerMessageReceivedAction("Failed connect to Main server");

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
