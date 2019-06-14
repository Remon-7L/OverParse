using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Ov3rD4r53
{
    //DLLImportはNativeMethods内に記述する(CA1060)。publicレベルは使わない(CA1401)。

    /// <summary>
    /// DLLImport用クラス
    /// </summary>
    internal static class NativeMethods
    {
        //HotKey.cs
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int RegisterHotKey(IntPtr hWnd, int id, int MOD_KEY, int VK);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int UnregisterHotKey(IntPtr hWnd, int id);

        //OverParse
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int GWL_EXSTYLE = (-20);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetWindowLong(IntPtr hwnd, int index);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int count);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool SetForegroundWindow(IntPtr hwnd);

        public static string GetActiveWindowTitle()
        {
            int chars = 256;
            StringBuilder buff = new StringBuilder(chars);
            IntPtr handle = GetForegroundWindow();
            if (handle == IntPtr.Zero) { return null; }
            return GetWindowText(handle, buff, chars) > 0 ? buff.ToString() : null;
        }

        /*
        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
        */


    }
}
