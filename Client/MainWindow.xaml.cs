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
using Client.Models;
using Server.Models;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OneServerCore _clientCore = null;
        ClientModel _clientModel = null;

        public MainWindow()
        {
            InitializeComponent();

            _clientCore = new OneServerCore();
            this.Title = _clientCore.LocalClientInfo.Name;

            _clientCore.ClientInfoListChangedAction = ClientInfoListChanged;
            _clientCore.ServerMessageReceivedAction = ServerMessageRecieved;
            _clientCore.P2PMessageReceivedAction = P2PMessageRecieved;

            _clientModel = new ClientModel();
            this.DataContext = _clientModel;

            Start();
        }

        private void Start()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                _clientCore.Start();
            });
        }

        private void ClientInfoListChanged(IEnumerable<ClientInfo> clientInfoList)
        {
            if(this.CheckAccess())
            {
                _clientModel.ClientInfoList = new List<ClientInfo>(clientInfoList);
            }
            else
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    _clientModel.ClientInfoList = new List<ClientInfo>(clientInfoList);
                }));
            }
        }

        private void ServerMessageRecieved(string message)
        {
            if (this.CheckAccess())
            {
                _clientModel.ServerCommunities.Add(string.Format("{0} {1}",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), message));
                ServerScrollViewer.ScrollToBottom();
            }
            else
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    _clientModel.ServerCommunities.Add(string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), message));
                    ServerScrollViewer.ScrollToBottom();
                }));
            }
        }

        private void P2PMessageRecieved(string message)
        {
            if (this.CheckAccess())
            {
                _clientModel.P2PCommunities.Add(string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), message));
                P2PScrollViewer.ScrollToBottom();
            }
            else
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    _clientModel.P2PCommunities.Add(string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), message));
                    P2PScrollViewer.ScrollToBottom();
                }));
            }
        }

        private void ClearServer_Click(object sender, RoutedEventArgs e)
        {
            _clientModel.ServerCommunities.Clear();
        }

        private void ClearP2P_Click(object sender, RoutedEventArgs e)
        {
            _clientModel.P2PCommunities.Clear();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (_clientModel.SelectedClient != null)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    _clientCore.RequestP2PConnection(_clientModel.SelectedClient.Guid);
                });
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            _clientCore.RefreshOnlieClients();
        }
    }
}
