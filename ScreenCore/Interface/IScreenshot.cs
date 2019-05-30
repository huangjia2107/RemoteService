using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScreenCore.Interface
{
    public interface IScreenshot
    {
        int Width { get; }
        int Height { get; }

        /// <summary>
        /// 32 bits per pixel; 8 bits each are used for the alpha, red, green, and blue components
        /// </summary>
        byte[] GetBuffer();
    }
}
