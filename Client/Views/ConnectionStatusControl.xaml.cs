using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Client.Models;
using System.Threading;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for ConnectionStatusControl.xaml
    /// </summary>
    public partial class ConnectionStatusControl : UserControl
    {
        ClientModel _clientModel = null;

        public ConnectionStatusControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _clientModel = this.DataContext as ClientModel;
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
                    _clientModel.ClientCore.RequestP2PConnection(_clientModel.SelectedClient.Client.Guid);
                });
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            _clientModel.ClientCore.RefreshOnlieClients();
        }

        private void TestNAT_Click(object sender, RoutedEventArgs e)
        {
            _clientModel.ClientCore.TestNAT();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (_clientModel.SelectedClient == null || !_clientModel.SelectedClient.Established)
                return;

            ThreadPool.QueueUserWorkItem(state =>
            {
                _clientModel.ClientCore.Send(_clientModel.SelectedClient.Client.Guid, (string)state);
            }, MessageTextBox.Text);

            MessageTextBox.Text = string.Empty;
        }
    }
}
