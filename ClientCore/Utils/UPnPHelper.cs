using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NATUPNPLib;
using System.Net;
using System.Net.Sockets;

namespace ClientCore.Utils
{
    public class UPnPHelper
    {
        private UPnPNAT _upnpNAT = null;
        private IStaticPortMappingCollection _staticMap = null;

        private static UPnPHelper _upnpHelper = new UPnPHelper();

        private UPnPHelper()
        {
            _upnpNAT = new UPnPNAT();
            _staticMap = _upnpNAT.StaticPortMappingCollection;
        }

        public static UPnPHelper Instance()
        {
            return _upnpHelper;
        }

        public bool AddMap(int externalPort, ProtocolType protocolType, IPAddress ip, int internalPort)
        {
            if (_staticMap == null || protocolType != ProtocolType.Udp || protocolType != ProtocolType.Tcp)
                return false;

            return _staticMap.Add(externalPort, protocolType.ToString(), internalPort, ip.ToString(), true, "For P2P") != null;
        }

        public void RemoveMap(int externalPort, ProtocolType protocolType)
        {
            if (_staticMap == null || protocolType != ProtocolType.Udp || protocolType != ProtocolType.Tcp)
                return;

            _staticMap.Remove(externalPort, protocolType.ToString());
        }
    }
}
