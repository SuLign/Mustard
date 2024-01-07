using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;

namespace Mustard.UIExtension.PlotControl.GLBase;

public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParams);

public static class User32
{
    public const int GWL_STYLE = -16;
    public const int WS_SYSTEMENU = 0x80000;
    public const int WM_SYSCOMMAND = 0x0112;
    public const int SC_MOVE = 0xF010;
    public const int HTCAPTION = 0x0002;
    public const int WM_HOTKEY = 0x312;

    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

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

    [DllImport("user32", SetLastError = true)]
    public static extern bool GetCursorPos(out POINT p);

    [DllImport("user32", SetLastError = true)]
    public static extern IntPtr GetCapture();

    [DllImport("user32", SetLastError = true)]
    public static extern short GetAsyncKeyState(Keys vKey);

    [DllImport("user32", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr handle, ref RECT rect);

    public delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "ReleaseCapture", ExactSpelling = true, SetLastError = true)]
    public static extern bool IntReleaseCapture();

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern int GetCurrentThreadId();

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern bool IsWindowEnabled(HandleRef hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern bool IsWindowVisible(HandleRef hWnd);

    [DllImport("PresentationNative_v0400.dll", EntryPoint = "SetFocusWrapper", SetLastError = true)]
    public static extern IntPtr SetFocus(HandleRef hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    [SecurityCritical]
    [SuppressUnmanagedCodeSecurity]
    public static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    [SecurityCritical]
    [SuppressUnmanagedCodeSecurity]
    public static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern bool EnumThreadWindows(int dwThreadId, EnumThreadWindowsCallback lpfn, HandleRef lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    [SecurityCritical]
    [SuppressUnmanagedCodeSecurity]
    public static extern bool IsWindow(HandleRef hWnd);

    [DllImport("PresentationNative_v0400.dll", CharSet = CharSet.Auto, EntryPoint = "EnableWindowWrapper", ExactSpelling = true, SetLastError = true)]
    public static extern bool EnableWindow(HandleRef hWnd, bool enable);

    [DllImport("PresentationNative_v0400.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLongWrapper", SetLastError = true)]
    public static extern int GetWindowLong(HandleRef hWnd, int nIndex);

    [DllImport("PresentationNative_v0400.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLongPtrWrapper", SetLastError = true)]
    public static extern IntPtr GetWindowLongPtr(HandleRef hWnd, int nIndex);

    [DllImport("PresentationNative_v0400.dll", EntryPoint = "GetParentWrapper", SetLastError = true)]
    public static extern IntPtr GetParent(HandleRef hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern void SetFocus(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetDC", ExactSpelling = true, SetLastError = true)]
    [SecurityCritical]
    [SuppressUnmanagedCodeSecurity]
    public static extern IntPtr GetDC(HandleRef hWnd);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
    [SecurityCritical]
    [SuppressUnmanagedCodeSecurity]
    public static extern int GetDeviceCaps(HandleRef hDC, int nIndex);

    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Ctrl = 2,
        Shift = 4,
        WindowsKey = 8,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct POINT
    {
        public int X;
        public int Y;

        public Point GetPoint() => new(X, Y);
    }

    public enum Keys : int
    {
        KeyCode = 0xFFFF,
        Modifiers = -65536,
        None = 0x0,
        LButton = 0x1,
        RButton = 0x2,
        Cancel = 0x3,
        MButton = 0x4,
        XButton1 = 0x5,
        XButton2 = 0x6,
        Back = 0x8,
        Tab = 0x9,
        LineFeed = 0xA,
        Clear = 0xC,
        Return = 0xD,
        Enter = 0xD,
        ShiftKey = 0x10,
        ControlKey = 0x11,
        Menu = 0x12,
        Pause = 0x13,
        Capital = 0x14,
        CapsLock = 0x14,
        KanaMode = 0x15,
        HanguelMode = 0x15,
        HangulMode = 0x15,
        JunjaMode = 0x17,
        FinalMode = 0x18,
        HanjaMode = 0x19,
        KanjiMode = 0x19,
        Escape = 0x1B,
        IMEConvert = 0x1C,
        IMENonconvert = 0x1D,
        IMEAccept = 0x1E,
        IMEAceept = 0x1E,
        IMEModeChange = 0x1F,
        Space = 0x20,
        Prior = 0x21,
        PageUp = 0x21,
        Next = 0x22,
        PageDown = 0x22,
        End = 0x23,
        Home = 0x24,
        Left = 0x25,
        Up = 0x26,
        Right = 0x27,
        Down = 0x28,
        Select = 0x29,
        Print = 0x2A,
        Execute = 0x2B,
        Snapshot = 0x2C,
        PrintScreen = 0x2C,
        Insert = 0x2D,
        Delete = 0x2E,
        Help = 0x2F,
        D0 = 0x30,
        D1 = 0x31,
        D2 = 0x32,
        D3 = 0x33,
        D4 = 0x34,
        D5 = 0x35,
        D6 = 0x36,
        D7 = 0x37,
        D8 = 0x38,
        D9 = 0x39,
        A = 0x41,
        B = 0x42,
        C = 0x43,
        D = 0x44,
        E = 0x45,
        F = 0x46,
        G = 0x47,
        H = 0x48,
        I = 0x49,
        J = 0x4A,
        K = 0x4B,
        L = 0x4C,
        M = 0x4D,
        N = 0x4E,
        O = 0x4F,
        P = 0x50,
        Q = 0x51,
        R = 0x52,
        S = 0x53,
        T = 0x54,
        U = 0x55,
        V = 0x56,
        W = 0x57,
        X = 0x58,
        Y = 0x59,
        Z = 0x5A,
        LWin = 0x5B,
        RWin = 0x5C,
        Apps = 0x5D,
        Sleep = 0x5F,
        NumPad0 = 0x60,
        NumPad1 = 0x61,
        NumPad2 = 0x62,
        NumPad3 = 0x63,
        NumPad4 = 0x64,
        NumPad5 = 0x65,
        NumPad6 = 0x66,
        NumPad7 = 0x67,
        NumPad8 = 0x68,
        NumPad9 = 0x69,
        Multiply = 0x6A,
        Add = 0x6B,
        Separator = 0x6C,
        Subtract = 0x6D,
        Decimal = 0x6E,
        Divide = 0x6F,
        F1 = 0x70,
        F2 = 0x71,
        F3 = 0x72,
        F4 = 0x73,
        F5 = 0x74,
        F6 = 0x75,
        F7 = 0x76,
        F8 = 0x77,
        F9 = 0x78,
        F10 = 0x79,
        F11 = 0x7A,
        F12 = 0x7B,
        F13 = 0x7C,
        F14 = 0x7D,
        F15 = 0x7E,
        F16 = 0x7F,
        F17 = 0x80,
        F18 = 0x81,
        F19 = 0x82,
        F20 = 0x83,
        F21 = 0x84,
        F22 = 0x85,
        F23 = 0x86,
        F24 = 0x87,
        NumLock = 0x90,
        Scroll = 0x91,
        LShiftKey = 0xA0,
        RShiftKey = 0xA1,
        LControlKey = 0xA2,
        RControlKey = 0xA3,
        LMenu = 0xA4,
        RMenu = 0xA5,
        BrowserBack = 0xA6,
        BrowserForward = 0xA7,
        BrowserRefresh = 0xA8,
        BrowserStop = 0xA9,
        BrowserSearch = 0xAA,
        BrowserFavorites = 0xAB,
        BrowserHome = 0xAC,
        VolumeMute = 0xAD,
        VolumeDown = 0xAE,
        VolumeUp = 0xAF,
        MediaNextTrack = 0xB0,
        MediaPreviousTrack = 0xB1,
        MediaStop = 0xB2,
        MediaPlayPause = 0xB3,
        LaunchMail = 0xB4,
        SelectMedia = 0xB5,
        LaunchApplication1 = 0xB6,
        LaunchApplication2 = 0xB7,
        OemSemicolon = 0xBA,
        Oem1 = 0xBA,
        Oemplus = 0xBB,
        Oemcomma = 0xBC,
        OemMinus = 0xBD,
        OemPeriod = 0xBE,
        OemQuestion = 0xBF,
        Oem2 = 0xBF,
        Oemtilde = 0xC0,
        Oem3 = 0xC0,
        OemOpenBrackets = 0xDB,
        Oem4 = 0xDB,
        OemPipe = 0xDC,
        Oem5 = 0xDC,
        OemCloseBrackets = 0xDD,
        Oem6 = 0xDD,
        OemQuotes = 0xDE,
        Oem7 = 0xDE,
        Oem8 = 0xDF,
        OemBackslash = 0xE2,
        Oem102 = 0xE2,
        ProcessKey = 0xE5,
        Packet = 0xE7,
        Attn = 0xF6,
        Crsel = 0xF7,
        Exsel = 0xF8,
        EraseEof = 0xF9,
        Play = 0xFA,
        Zoom = 0xFB,
        NoName = 0xFC,
        Pa1 = 0xFD,
        OemClear = 0xFE,
        Shift = 0x10000,
        Control = 0x20000,
        Alt = 0x40000
    }
}