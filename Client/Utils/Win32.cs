using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.Utils
{
    public static class Win32
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}
