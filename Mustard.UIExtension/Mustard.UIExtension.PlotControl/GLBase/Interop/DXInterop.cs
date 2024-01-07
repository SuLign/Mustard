using System;
using System.Runtime.InteropServices;

namespace Mustard.UIExtension.PlotControl.GLBase.Interop;

internal static class DXInterop
{
    private delegate int NativeCreateDeviceEx(IntPtr contextHandle, int adapter, DeviceType deviceType, IntPtr focusWindowHandle, CreateFlags behaviorFlags, ref PresentationParameters presentationParameters, IntPtr fullscreenDisplayMode, out IntPtr deviceHandle);

    private delegate int NativeCreateRenderTarget(IntPtr deviceHandle, int width, int height, Format format, MultisampleType multisample, int multisampleQuality, bool lockable, out IntPtr surfaceHandle, ref IntPtr sharedHandle);

    private delegate uint NativeRelease(IntPtr resourceHandle);

    public const uint DefaultSdkVersion = 32u;

    private const int CreateDeviceEx_Offset = 20;

    private const int CreateRenderTarget_Offset = 28;

    private const int Release_Offset = 2;

    [DllImport("d3d9.dll")]
    public static extern int Direct3DCreate9Ex(uint SdkVersion, out IntPtr ctx);

    public static int CreateDeviceEx(IntPtr contextHandle, int adapter, DeviceType deviceType, IntPtr focusWindowHandle, CreateFlags behaviorFlags, ref PresentationParameters presentationParameters, IntPtr fullscreenDisplayMode, out IntPtr deviceHandle)
    {
        return Marshal.GetDelegateForFunctionPointer<NativeCreateDeviceEx>(Marshal.ReadIntPtr(Marshal.ReadIntPtr(contextHandle, 0), 20 * IntPtr.Size))(contextHandle, adapter, deviceType, focusWindowHandle, behaviorFlags, ref presentationParameters, fullscreenDisplayMode, out deviceHandle);
    }

    public static int CreateRenderTarget(IntPtr deviceHandle, int width, int height, Format format, MultisampleType multisample, int multisampleQuality, bool lockable, out IntPtr surfaceHandle, ref IntPtr sharedHandle)
    {
        return Marshal.GetDelegateForFunctionPointer<NativeCreateRenderTarget>(Marshal.ReadIntPtr(Marshal.ReadIntPtr(deviceHandle, 0), 28 * IntPtr.Size))(deviceHandle, width, height, format, multisample, multisampleQuality, lockable, out surfaceHandle, ref sharedHandle);
    }

    public static uint Release(IntPtr resourceHandle)
    {
        return Marshal.GetDelegateForFunctionPointer<NativeRelease>(Marshal.ReadIntPtr(Marshal.ReadIntPtr(resourceHandle, 0), 2 * IntPtr.Size))(resourceHandle);
    }
}
