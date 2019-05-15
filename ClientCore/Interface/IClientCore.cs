using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientCore.Config;
using Server.Models;

namespace ClientCore.Interface
{
    public interface IClientCore
    {
        Action<IEnumerable<ClientInfo>> ClientInfoListChangedAction { get; set; }
        Action<string> ServerMessageReceivedAction { get; set; }
        Action<string> P2PMessageReceivedAction { get; set; }

        ClientInfo LocalClientInfo { get;}

        void Start();

        void RequestP2PConnection(string targetGuid);

        void RefreshOnlieClients();
    }
}
