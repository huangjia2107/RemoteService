using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScreenCore.Interface;

namespace ScreenCore.Capture
{
    class DXGIScreenshot : IScreenshot
    {
        public DXGIScreenshot(int width, int height)
        {
            Width = width;
            Height = height;
        }

        #region IScreenshot Members

        public int Width { get; private set; }
        public int Height { get; private set; }

        public byte[] GetBuffer()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
