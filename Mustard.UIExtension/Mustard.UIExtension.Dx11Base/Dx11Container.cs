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
    internal const int
            WS_CHILD = 0x40000000,
            WS_VISIBLE = 0x10000000,
            LBS_NOTIFY = 0x00000001,
            HOST_ID = 0x00000002,
            LISTBOX_ID = 0x00000001,
            WS_VSCROLL = 0x00200000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_BORDER = 0x00800000;
    [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
    internal static extern IntPtr CreateWindowEx(int dwExStyle,
                                                  string lpszClassName,
                                                  string lpszWindowName,
                                                  int style,
                                                  int x, int y,
                                                  int width, int height,
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
    private int cnt = 1000;

    public Dx11Container()
    {
        Loaded += Dx11ContainerLoaded;
        CompositionTarget.Rendering += CompositionTargetRendering;
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
            WS_CHILD | WS_CLIPCHILDREN,
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
