using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EJThreads
{
    class Win32
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr MessageBox(int hWnd, String text,
                     String caption, uint type);


        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool GetUserName(System.Text.StringBuilder sb, ref Int32 length);
    }
}
