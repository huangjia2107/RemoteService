using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace ClientCore.Utils
{
    static class ConnectionHelper
    {
        public static IPAddress LocalIP()
        {
            var ss = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(addr => addr.AddressFamily == AddressFamily.InterNetwork);

            var ii = ss.FirstOrDefault(addr => addr.ToString().StartsWith("192.168"));
            if (ii == null)
                return ss.First();

            return ii;
        }

        public static int AvailablePort(int startPort)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var endPoints = ipGlobalProperties.GetActiveUdpListeners();

            var port = startPort;

            while (true)
            {
                if (port > IPEndPoint.MaxPort)
                {
                    throw new ApplicationException("Not able to find a free UDP port.");
                }

                if (!endPoints.Any(p => p.Port == port))
                    break;

                port++;
            }

            return port;
        }
    }
}
