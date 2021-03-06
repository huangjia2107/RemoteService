﻿using System;
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
using ClientCore.Models;
using System.Net;
using Server.Models;
using System.IO; 
using System.IO.Compression;
using System.Diagnostics;

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

            _clientModel.Client.ClientInfoListChangedAction = ClientInfoListChanged;
            _clientModel.Client.ServerMessageReceivedAction = ServerMessageRecieved;
            _clientModel.Client.P2PMessageReceivedAction = P2PMessageRecieved;
        }

        private void ClientInfoListChanged(IEnumerable<ClientInfoEx> clientInfoList)
        {
            if (this.CheckAccess())
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
                _clientModel.ServerCommunities.Add(string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), message));
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
                    _clientModel.Client.RequestP2PConnection(_clientModel.SelectedClient.Client.Guid);
                });
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            _clientModel.Client.RefreshOnlieClients();
        }

        private void TestUDP1_Click(object sender, RoutedEventArgs e)
        {
            _clientModel.Client.TestUDP1();
        }

        private void TestUDP2_Click(object sender, RoutedEventArgs e)
        {
            _clientModel.Client.TestUDP2();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (_clientModel.SelectedClient == null || !_clientModel.SelectedClient.Established)
                return;

            ThreadPool.QueueUserWorkItem(state =>
            {
                _clientModel.Client.Send(_clientModel.SelectedClient.Client.Guid, (string)state);
            }, MessageTextBox.Text);

            MessageTextBox.Text = string.Empty;
        }

        private bool SendScreenshotBuffer(IPAddress ip, int port, byte[] buffer, int originLength, long compressLength)
        {
            if (!_clientModel.Client.ShareScreenshot(ip, port,
                                        new Screenshot
                                        {
                                            Buffer = buffer,
                                            OriginalLength = originLength,
                                            CompressLength = (int)compressLength,
                                            Width = _clientModel.Capture.Width,
                                            Height = _clientModel.Capture.Height
                                        }))
            {
                _clientModel.IsSharingScreen = false;
            }

            return _clientModel.IsSharingScreen;
        }

        private void Screen_Click(object sender, RoutedEventArgs e)
        {
            if (_clientModel.SelectedClient == null || !_clientModel.SelectedClient.Established)
                return;

            var targetClient = _clientModel.ClientInfoList.FirstOrDefault(c => c.Client.Guid == _clientModel.SelectedClient.Client.Guid && c.Established);
            if (targetClient == null)
                return; 

            var ip = IPAddress.Parse(targetClient.IP);
            var port = targetClient.Port; 

            ThreadPool.QueueUserWorkItem(state =>
            { 
                while (_clientModel.IsSharingScreen)
                { 
                    if (_clientModel.Capture.RefreshBuffer())// && _clientModel.Capture.IsDiff())
                    {  
                        using (var ms = new MemoryStream())
                        {
                            using (var gs = new GZipStream(ms, CompressionMode.Compress, true))
                                gs.Write(_clientModel.Capture.CurrentBuffer, 0, _clientModel.Capture.CurrentBuffer.Length);
                              
                            ms.Seek(0, SeekOrigin.Begin);

                            var totalLength = ms.Length;
                            var sendSize = 50 * 1024L;

                            while (ms.Position < totalLength)
                            {
                                if (totalLength - ms.Position < sendSize)
                                    sendSize = totalLength - ms.Position;

                                var readBuffer = new byte[sendSize];
                                ms.Read(readBuffer, 0, (int)sendSize);

                                SendScreenshotBuffer(ip, port, readBuffer, _clientModel.Capture.CurrentBuffer.Length, totalLength);

                                if (ms.Position >= totalLength)
                                    break;

                                Thread.Sleep(5);
                            }
                        } 

                        Thread.Sleep(30);
                    }
                }
            });
        }
    }
}
