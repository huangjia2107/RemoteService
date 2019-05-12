using Client.ViewModels;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Client.Models
{    
    class ClientModel : ViewModelBase
    {
        public ClientModel()
        {
            _clientInfoList = new List<ClientInfo>();

            ServerCommunities = new ObservableCollection<string>();
            P2PCommunities = new ObservableCollection<string>();
        }

        private ClientInfo _selectedClient;
        public ClientInfo SelectedClient
        {
            get { return _selectedClient; }
            set { _selectedClient = value; InvokePropertyChanged("SelectedClient"); }
        }

        private List<ClientInfo> _clientInfoList;
        public List<ClientInfo> ClientInfoList
        {
            get { return _clientInfoList; }
            set { _clientInfoList = value; InvokePropertyChanged("ClientInfoList"); }
        } 

        public ObservableCollection<string> ServerCommunities { get; set; }
        public ObservableCollection<string> P2PCommunities { get; set; }
    }
}
