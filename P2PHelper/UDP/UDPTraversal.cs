using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using NetworkCommsDotNet.Connections.UDP;
using NetworkCommsDotNet.Tools;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet;
using System.Diagnostics;
using Server.Models;

namespace P2PHelper.UDP
{
    public class UDPTraversal
    {
        private IPAddress _serverIP = null;
        private int _serverPort;

        private Connection _connection = null;
        private Action<string, IPEndPoint> _connected = null;

        public bool IsSource { get; private set; }

        public EndPoint LocalEndPoint
        {
            get { return _connection == null ? null : _connection.ConnectionInfo.LocalEndPoint; }
        }

        public UDPTraversal(string udpServerIP, int udpServerPort, Action<string, IPEndPoint> connected)
        {
            _serverIP = IPAddress.Parse(udpServerIP);
            _serverPort = udpServerPort;
            _connected = connected;

            _connection = CreateUDPConnection();

            NetworkComms.AppendGlobalIncomingPacketHandler<string>(PacketType.REQ_UDPP2PConnect, Connected);
        }

        private void Connected(PacketHeader header, Connection connection, string fromIdentity)
        {
            if (_connected != null)
                _connected(fromIdentity, (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint);
        }

        public bool Request<T>(T message)
        {
            return Request(message, true);
        }

        //Source -> Server -> Target
        //tell target to try punch
        public bool Request<T>(T message, bool isSource)
        {
            var result = Send(PacketType.REQ_UDPP2PRequest, message, _serverIP, _serverPort);
            if (result)
                IsSource = isSource;

            return result;
        }

        //1.Target -> Source
        //2.Target -> Server -> Source
        //try punch to source
        public bool TryPunch(IPAddress sourceIP, int sourcePort)
        {
            /*
            var ttl = _udpConnection.Ttl;
            _udpConnection.Ttl = 3;

            if (!MultiholePunching(ip, port, port, 1000))
            {
                _udpConnection.Ttl = ttl;
                return;
            }
            */

            //just punch, do not care result.
            return Send<string>("IGNORE", "Try Punch", sourceIP, sourcePort);
        }

        //Source -> Target
        //connect to target
        public bool Connect<T>(T message, IPAddress targetIP, int targetPort)
        {
            return Send(PacketType.REQ_UDPP2PConnect, message, targetIP, targetPort);
        }

        public void CleanUp()
        {
            IsSource = false;
        }

        private UDPConnection CreateUDPConnection()
        {
            try
            {
                var connectionInfo = new ConnectionInfo(ConnectionType.UDP, ShortGuid.NewGuid(), new IPEndPoint(IPAddress.Any, 0), true);
                var connection = UDPConnection.GetConnection(connectionInfo, UDPOptions.None);

                return connection;
            }
            catch (Exception ex)
            {
                Trace.TraceError("[ UDP ] CreateLocalUDPConnection, Error = {0}", ex.Message);
            }

            return null;
        }

        public bool Send<T>(string packetType, T message, IPAddress ip, int port)
        {
            if (_connection == null)
                return false;

            try
            {
                (_connection as UDPConnection).SendObject<T>(packetType, message, new IPEndPoint(ip, port));
            }
            catch (Exception ex)
            {
                Trace.TraceError("[ UDP ] SendToIPEndPoint, Error = {0}", ex.Message);
                return false;
            }

            return true;
        }

        private bool MultiholePunching(IPAddress targetIP, int targetPort, int startPort, int tryTimes)
        {
            var ports = Enumerable.Range(startPort, 65535 - startPort + 1).ToArray();
            Shuffle(ports);

            int i = 0;
            while (i < Math.Min(tryTimes, ports.Length))
            {
                var port = ports[i];
                if (port != targetPort)
                {
                    if (!Send("IGNORE", "Try Punch", targetIP, port))
                        return false;

                    System.Threading.Thread.Sleep(50);
                }

                i++;
            }

            return true;
        }

        // Fisher-Yates shuffle algorithm
        private void Shuffle(int[] numbers)
        {
            var random = new Random();
            var temp = 0;

            for (int i = numbers.Length - 1; i > 0; i--)
            {
                var r = random.Next(1, i);

                temp = numbers[i];
                numbers[i] = numbers[r];
                numbers[r] = temp;
            }
        }
    }
}
