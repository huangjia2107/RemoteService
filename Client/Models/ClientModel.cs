using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ClientCore;
using ClientCore.Interface;
using ClientCore.Config;
using Server.Config;
using ClientCore.Models;
using Server.Models;
using ClientCore.ViewModels;

namespace Client.Models
{
    class ClientModel : ViewModelBase
    {
        public IClientCore ClientCore { get; private set; }

        private ServerConfig _serverConfig = null;

        public ClientModel()
        {
            _serverConfig = ConfigHelper<ServerConfig>.Instance().GetServerConfig();
            _clientInfoList = new List<ClientInfoEx>();
             
            ClientCore = new TwoServerCore();

            ServerCommunities = new ObservableCollection<string>();
            P2PCommunities = new ObservableCollection<string>();
        }

        private ClientInfoEx _selectedClient;
        public ClientInfoEx SelectedClient
        {
            get { return _selectedClient; }
            set { _selectedClient = value; InvokePropertyChanged("SelectedClient"); }
        }

        private List<ClientInfoEx> _clientInfoList;
        public List<ClientInfoEx> ClientInfoList
        {
            get { return _clientInfoList; }
            set { _clientInfoList = value; InvokePropertyChanged("ClientInfoList"); }
        }

        public ObservableCollection<string> ServerCommunities { get; set; }
        public ObservableCollection<string> P2PCommunities { get; set; }
    }
}
