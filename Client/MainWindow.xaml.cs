using System.Threading;
using System.Windows;
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

            this.Title = _clientModel.ClientCore.LocalClientInfo.Client.Name;
            this.DataContext = _clientModel; 
        }

        private void Start()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                _clientModel.ClientCore.Start();
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Start();
        } 
    }
}
