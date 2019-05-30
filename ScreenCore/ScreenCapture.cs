using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScreenCore.Interface;

namespace ScreenCore.Capture
{
    public class ScreenCapture
    {
        public byte[] CurrentBuffer { get; private set; }
        public byte[] CacheBuffer { get; private set; }

        private IScreenshot _screenshot = null;

        public int Width
        {
            get { return _screenshot.Width; }
        }

        public int Height
        {
            get { return _screenshot.Height; }
        }

        public ScreenCapture(int width, int height)
        {
            //win8/win10
            if (Environment.OSVersion.Version >= new Version(6, 2))
                _screenshot = new DXGIScreenshot(width, height);
            else
                _screenshot = new GDIScreenshot(width, height);
        }

        public bool RefreshBuffer()
        {
            var buffer = _screenshot.GetBuffer();
            if (buffer == null)
                return false;

            CacheBuffer = CurrentBuffer;
            CurrentBuffer = buffer;

            return true;
        }

        public void CompressBuffer()
        {

        }

        public bool IsDiff()
        {
            if (CurrentBuffer == null)
                return false;

            if (CacheBuffer == null || CacheBuffer.Length != CurrentBuffer.Length)
                return true;

            for (int index = 0; index < CacheBuffer.Length; index += 4)
            {
                if (CacheBuffer[index] != CurrentBuffer[index]
                    || CacheBuffer[index + 1] != CurrentBuffer[index + 1]
                    || CacheBuffer[index + 2] != CurrentBuffer[index + 2]
                    || CacheBuffer[index + 3] != CurrentBuffer[index + 3])
                {
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            CurrentBuffer = null;
            CacheBuffer = null;
        }
    }
}
