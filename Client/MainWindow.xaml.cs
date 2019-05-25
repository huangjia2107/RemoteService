using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClientCore;
using ClientCore.Models;
using Server.Models;
using Client.Models;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ClientModel _clientModel = null;

        public MainWindow()
        {
            InitializeComponent();

            _clientModel = new ClientModel();

            _clientModel.ClientCore.ClientInfoListChangedAction = ClientInfoListChanged;
            _clientModel.ClientCore.ServerMessageReceivedAction = ServerMessageRecieved;
            _clientModel.ClientCore.P2PMessageReceivedAction = P2PMessageRecieved;

            this.Title = _clientModel.ClientCore.LocalClientInfo.Client.Name;
            this.DataContext = _clientModel;

            Start();
        }

        private void Start()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                _clientModel.ClientCore.Start();
            });
        }

        private void ClientInfoListChanged(IEnumerable<ClientInfoEx> clientInfoList)
        {
            if(this.CheckAccess())
            {
                _clientModel.ClientInfoList = new List<ClientInfoEx>(clientInfoList);
            }
            else
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    _clientModel.ClientInfoList = new List<ClientInfoEx>(clientInfoList);
                }));
            }
        }

        private void ServerMessageRecieved(string message)
        {
            if (this.CheckAccess())
            {
                _clientModel.ServerCommunities.Add(string.Format("{0} {1}",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), message));
                StatusControl.ServerScrollViewer.ScrollToBottom();
            }
            else
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    _clientModel.ServerCommunities.Add(string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), message));
                    StatusControl.ServerScrollViewer.ScrollToBottom();
                }));
            }
        }

        private void P2PMessageRecieved(string message)
        {
            if (this.CheckAccess())
            {
                _clientModel.P2PCommunities.Add(string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), message));
                StatusControl.P2PScrollViewer.ScrollToBottom();
            }
            else
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    _clientModel.P2PCommunities.Add(string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), message));
                    StatusControl.P2PScrollViewer.ScrollToBottom();
                }));
            }
        } 
    }
}
