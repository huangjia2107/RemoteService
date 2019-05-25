using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Server.Models;
using ClientCore.ViewModels;

namespace ClientCore.Models
{
    public class ClientInfoEx : ViewModelBase
    {
        public ClientInfo Client { get; private set; }

        public ClientInfoEx(ClientInfo client)
        {
            Client = client;
        }

        private bool _established;
        public bool Established
        {
            get{ return _established; }
            set { _established = value; InvokePropertyChanged("Established"); }
        }

        public string IP { get; set; }
        public int Port { get; set; }
    }
}
