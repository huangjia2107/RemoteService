using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Models
{
    [ProtoContract]
    public class ClientInfo
    {
        [ProtoMember(1)]
        public string Guid { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public bool CanAccess { get; set; }
    }

    [ProtoContract]
    public class P2PRequest
    {
        [ProtoMember(1)]
        public string SourceGuid { get; set; }
        [ProtoMember(2)]
        public string TargetGuid { get; set; }
    }

    [ProtoContract]
    public class P2PClient
    {
        [ProtoMember(1)]
        public string GUID { get; set; }
        [ProtoMember(2)]
        public string IP { get; set; }
        [ProtoMember(3)]
        public int Port { get; set; }
    }

    [ProtoContract]
    public class Screenshot
    {
        [ProtoMember(1)]
        public int Width { get; set; }
        [ProtoMember(2)]
        public int Height { get; set; }
        [ProtoMember(3)]
        public int OriginalLength { get; set; }
        [ProtoMember(4)]
        public int CompressLength { get; set; }
        [ProtoMember(5)]
        public byte[] Buffer { get; set; }
    }
}
