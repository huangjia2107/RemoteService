using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScreenCore.Interface;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Drawing;

namespace ScreenCore.Capture
{
    class GDIScreenshot : IScreenshot
    {
        public GDIScreenshot(int width, int height)
        {
            Width = width;
            Height = height;
        }

        #region IScreenshot Members

        public int Width { get; private set; }
        public int Height { get; private set; }

        public byte[] GetBuffer()
        {
            try
            {
                var buffer = new byte[Width * Height * 4];

                using (var bitmap = new Bitmap(Width, Height))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        //screen to bitmap
                        graphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(Width, Height));

                        //bitmap to buffer 
                        var bits = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        Marshal.Copy(bits.Scan0, buffer, 0, buffer.Length);
                        bitmap.UnlockBits(bits);

                        // Should also capture the mouse cursor here, but skipping for simplicity
                        // For those who are interested, look at http://www.codeproject.com/Articles/12850/Capturing-the-Desktop-Screen-with-the-Mouse-Cursor
                    }
                }

                return buffer;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
