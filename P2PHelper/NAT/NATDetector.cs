using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace P2PHelper.NAT
{
    internal class NATDetector
    {
        private List<IPEndPoint> _stunAddrList = null;

        public NATDetector()
        {
            _stunAddrList = new List<IPEndPoint>
            {
                new IPEndPoint(long.Parse("stun.pppan.net"), 3478),
                new IPEndPoint(long.Parse("stun.ideasip.com"), 3478),
                new IPEndPoint(long.Parse("stun.voipbuster.com"), 3478),
                new IPEndPoint(long.Parse("stun.ekiga.net"), 3478),
                new IPEndPoint(long.Parse("217.116.122.138"), 3478)
            };
        }
    }
}
