using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Mustard.UIExtension.Dx11Basae;

public class Dx11Container : HwndHost
{
    #region PInvoke declarations
    internal const uint WS_EX_WINDOWEDGE = 0x00000100,
                        WS_EX_CLIENTEDGE = 0x00000200,
                        WS_OVERLAPPED = 0x00000000,
                        WS_POPUP = 0x80000000,
                        WS_CHILD = 0x40000000,
                        WS_MINIMIZE = 0x20000000,
                        WS_VISIBLE = 0x10000000,
                        WS_DISABLED = 0x08000000,
                        WS_CLIPSIBLINGS = 0x04000000,
                        WS_CLIPCHILDREN = 0x02000000,
                        WS_MAXIMIZE = 0x01000000,
                        WS_CAPTION = 0x00C00000,
                        WS_BORDER = 0x00800000,
                        WS_DLGFRAME = 0x00400000,
                        WS_VSCROLL = 0x00200000,
                        WS_HSCROLL = 0x00100000,
                        WS_SYSMENU = 0x00080000,
                        WS_THICKFRAME = 0x00040000,
                        WS_GROUP = 0x00020000,
                        WS_TABSTOP = 0x00010000,
                        WS_MINIMIZEBOX = 0x00020000,
                        WS_MAXIMIZEBOX = 0x00010000;

    internal const uint
            LBS_NOTIFY = 0x00000001,
            HOST_ID = 0x00000002,
            LISTBOX_ID = 0x00000001,
            WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);

    internal const uint WS_OVERLAPPEDWINDOW =
        WS_OVERLAPPED |
        WS_CAPTION |
        WS_SYSMENU |
        WS_THICKFRAME |
        WS_MINIMIZEBOX |
        WS_MAXIMIZEBOX;

    [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
    internal static extern IntPtr CreateWindowEx(uint dwExStyle,
                                                 string lpszClassName,
                                                 string lpszWindowName,
                                                 uint style,
                                                 int x,
                                                 int y,
                                                 int width,
                                                 int height,
                                                 IntPtr hwndParent,
                                                 IntPtr hMenu,
                                                 IntPtr hInst,
                                                 [MarshalAs(UnmanagedType.AsAny)] object pvParam);

    [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
    internal static extern bool DestroyWindow(IntPtr hwnd);

    [DllImport("Mustard.DX11Core.dll")]
    internal static extern IntPtr CreateHandle();

    [DllImport("Mustard.DX11Core.dll")]
    internal static extern void InitD3D(IntPtr handle, IntPtr hwnd);

    [DllImport("Mustard.DX11Core.dll")]
    internal static extern void Render(IntPtr handle);
    #endregion

    private IntPtr dx11HostHandle = IntPtr.Zero;
    private IntPtr dx11CoreHandle;
    private bool needRender;
    private int cnt = 20;

    public Dx11Container()
    {
        Loaded += Dx11ContainerLoaded;
        CompositionTarget.Rendering += CompositionTargetRendering;
        IsVisibleChanged += Dx11Container_IsVisibleChanged;
    }

    private void Dx11Container_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsVisible)
        {
            cnt = 20;
        }
    }

    private void CompositionTargetRendering(object sender, EventArgs e)
    {
        if (needRender)
        {
            if (IsVisible && cnt-- > 0)
            {
                Render(dx11CoreHandle);
            }
        }
        needRender = true;
    }

    private void Dx11ContainerLoaded(object sender, RoutedEventArgs e)
    {
        needRender = true;
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        dx11HostHandle = CreateWindowEx(
            0, "static", "",
            WS_CHILD | WS_VISIBLE,
            0, 0,
            (int)ActualWidth, (int)ActualHeight,
            hwndParent.Handle,
            (IntPtr)HOST_ID,
            IntPtr.Zero,
            0);
        dx11CoreHandle = CreateHandle();
        InitD3D(dx11CoreHandle, dx11HostHandle);
        return new HandleRef(this, dx11HostHandle);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        needRender = false;
        cnt = 20;
    }
}
