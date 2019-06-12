using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NATUPNPLib;
using System.Net;
using System.Net.Sockets;
using UPNPLib;

namespace ClientCore.Utils
{
    public class UPnPHelper
    {
        //NATUPNPLib
        private UPnPNATClass _upnpNAT = null;
        private IStaticPortMappingCollection _staticMap = null;

        //UPNPLib
        UPnPDeviceFinderClass _deviceFinder = null;

        private static UPnPHelper _upnpHelper = new UPnPHelper();

        private UPnPHelper()
        {
            _upnpNAT = new UPnPNATClass();
            _staticMap = _upnpNAT.StaticPortMappingCollection;

            _deviceFinder = new UPnPDeviceFinderClass();
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

        public void Test()
        {
            var bstrTypeURI = "upnp:rootdevice";
            var bstrTypeURI1 = "urn:schemas-upnp-org:device:InternetGatewayDevice:1";
            var bstrTypeURI2 = "urn:schemas-upnp-org:device:WANDevice:1";
            var bstrTypeURI3 = "urn:schemas-upnp-org:device:WANConnectionDevice:1";

            var callnack = new UPnPDeviceFinderCallback(_deviceFinder);

            var id = _deviceFinder.CreateAsyncFind(bstrTypeURI, 0, callnack);
            _deviceFinder.StartAsyncFind(id);
        }
    }

    public class UPnPDeviceFinderCallback : IUPnPDeviceFinderCallback
    {
        UPnPDeviceFinderClass _deviceFinder = null;

        public UPnPDeviceFinderCallback(UPnPDeviceFinderClass deviceFinder)
        {
            _deviceFinder = deviceFinder;
        }

        #region IUPnPDeviceFinderCallback Members

        public void DeviceAdded(int lFindData, UPnPDevice pDevice)
        {

        }

        public void DeviceRemoved(int lFindData, string bstrUDN)
        {

        }

        public void SearchComplete(int lFindData)
        {

        }

        #endregion
    }
}
