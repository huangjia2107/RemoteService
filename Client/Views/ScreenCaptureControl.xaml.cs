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
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using Client.Models;
using Client.Utils;
using Server.Models;
using System.IO.Compression;
using System.IO;
//using ScreenCore.Capture; 

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for ScreenCaptureControl.xaml
    /// </summary>
    public partial class ScreenCaptureControl : UserControl
    {
        ClientModel _clientModel = null;
        List<byte> _screenshotCache = null;


        public ScreenCaptureControl()
        {
            InitializeComponent();

            _screenshotCache = new List<byte>();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _clientModel = this.DataContext as ClientModel;
            _clientModel.ClientCore.ScreenshotReceivedAction = ScreenshotReceived;
        }

        private void ScreenshotReceived(Screenshot screenshot)
        {
            if (screenshot == null)
                return;

            try
            {
                _screenshotCache.AddRange(screenshot.Buffer);

                if (_screenshotCache.Count == screenshot.CompressLength)
                {
                    var buffer = _screenshotCache.ToArray();
                    _screenshotCache.Clear();

                    var width = screenshot.Width;
                    var height = screenshot.Height;
                    var origincalLength = screenshot.OriginalLength;

                    ThreadPool.QueueUserWorkItem(s =>
                    {
                        var b = (byte[])s;
                        var originalBuffers = new byte[origincalLength];

                        using (var ms = new MemoryStream(b))
                        {
                            using (var gs = new GZipStream(ms, CompressionMode.Decompress))
                            {
                                gs.Read(originalBuffers, 0, origincalLength);
                            }
                        }

                        using (var bitmap = Argb32BytesToBitmap(originalBuffers, width, height))
                        {
                            ddd.Dispatcher.Invoke((Action)(() =>
                            {
                                ddd.Source = BitmapToBitmapSource(bitmap);
                            }));
                        }
                    }, buffer);


                }
            }
            catch (Exception ex)
            {

            }
        }

        public Bitmap Argb32BytesToBitmap(byte[] buffer, int width, int height)
        {
            if (buffer.Length != width * height * 4)
                return null;

            var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            Marshal.Copy(buffer, 0, bmpData.Scan0, buffer.Length);
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }

        public BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();
            BitmapSource result = null;

            try
            {
                result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception ex)
            {

            }
            finally
            {
                //Release resource
                Win32.DeleteObject(hBitmap);
            }

            return result;
        }
    }
}
