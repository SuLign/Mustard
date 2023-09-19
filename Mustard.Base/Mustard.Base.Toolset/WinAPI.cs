using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mustard.Base.Toolset
{
    public class WinAPI
    {
        #region User32
        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParams);

        public const int GWL_STYLE = -16;
        public const int WS_SYSTEMENU = 0x80000;
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;
        public const int WM_HOTKEY = 0x312;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string className, string winName);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageTimeout(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, uint fuFlage, uint timeout, IntPtr result);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc proc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string className, string winName);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hwnd, IntPtr hwndParent);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers fsModifiers, int vk);

        [DllImport("user32", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [Flags]
        public enum KeyModifiers
        {
            None = 0,
            Alt = 1,
            Ctrl = 2,
            Shift = 4,
            WindowsKey = 8,
        }
        #endregion

        #region Shell32
        /// <summary>
        /// 获取程序Icon
        /// </summary>
        /// <param name="lpszFile">文件路径</param>
        /// <param name="niconIndex">图标索引</param>
        /// <param name="phiconLarge">指向图标句柄数据的数组，它可接收从文件获取的大图标的句柄。</param>
        /// <param name="phiconSamll">指向图标句柄数据的数组，它可接收从文件获取的小图标的句柄。</param>
        /// <param name="nIcons">执行要从文件中抽取的图标的数量</param>
        /// <returns>执行结果</returns>
        [DllImport("shell32")]
        public static extern int ExtractIconEx(
            string lpszFile,
            int niconIndex,
            IntPtr[] phiconLarge,
            IntPtr[] phiconSamll,
            int nIcons);
        #endregion

        #region Kernel32

        [DllImport("kernel32", SetLastError = true)]
        public static extern short GlobalAddAtom(string lpString);

        [DllImport("kernel32", SetLastError = true)]
        public static extern short GlobalDeleteAtom(short nAtom);
        #endregion
    }
}
