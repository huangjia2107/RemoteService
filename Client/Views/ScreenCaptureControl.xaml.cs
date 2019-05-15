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
//using ScreenCore.Capture; 

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for ScreenCaptureControl.xaml
    /// </summary>
    public partial class ScreenCaptureControl : UserControl
    {
        //ScreenCapture screenCapture = null;

        public ScreenCaptureControl()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(ScreenCaptureControl_Loaded);

           // screenCapture = new ScreenCapture(0, 0, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
        }

        void ScreenCaptureControl_Loaded(object sender, RoutedEventArgs e)
        {
            //screenCapture = new ScreenCapture(0, 0, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            //screenCapture.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            //screenCapture.Stop();
        }
    }
}
