using OpenTK.Graphics;
using OpenTK.Platform.Windows;
using OpenTK.Platform;
using System;
using System.Threading;
using System.Windows.Interop;
using System.Windows;
using Mustard.UIExtension.PlotControl.GLBase.Interop;

namespace Mustard.UIExtension.PlotControl.GLBase;

internal sealed class DxGlContext : IDisposable
{
    private static IGraphicsContext _sharedContext;

    private static GLControlSettings _sharedContextSettings;

    private static IDisposable[] _sharedContextResources;

    private static int _sharedContextReferenceCount;

    public IntPtr DxContextHandle { get; }

    public IntPtr DxDeviceHandle { get; }

    public IGraphicsContext GraphicsContext { get; }

    public IntPtr GlDeviceHandle { get; }

    public DxGlContext(GLControlSettings settings)
    {
        DXInterop.Direct3DCreate9Ex(32u, out var ctx);
        DxContextHandle = ctx;
        PresentationParameters presentationParameters = new PresentationParameters
        {
            Windowed = 1,
            SwapEffect = SwapEffect.Discard,
            DeviceWindowHandle = IntPtr.Zero,
            PresentationInterval = 0,
            BackBufferFormat = Format.X8R8G8B8,
            BackBufferWidth = 1,
            BackBufferHeight = 1,
            AutoDepthStencilFormat = Format.Unknown,
            BackBufferCount = 1u,
            EnableAutoDepthStencil = 0,
            Flags = 0,
            FullScreen_RefreshRateInHz = 0,
            MultiSampleQuality = 0,
            MultiSampleType = MultisampleType.None
        };
        DXInterop.CreateDeviceEx(ctx, 0, DeviceType.HAL, IntPtr.Zero, CreateFlags.Multithreaded | CreateFlags.PureDevice | CreateFlags.HardwareVertexProcessing, ref presentationParameters, IntPtr.Zero, out var deviceHandle);
        DxDeviceHandle = deviceHandle;
        if (settings.ContextToUse != null)
        {
            GraphicsContext = settings.ContextToUse;
        }
        else
        {
            GraphicsContext = GetOrCreateSharedOpenGLContext(settings);
        }

        GlDeviceHandle = Wgl.DXOpenDeviceNV(deviceHandle);
    }

    private static IGraphicsContext GetOrCreateSharedOpenGLContext(GLControlSettings settings)
    {
        if (_sharedContext != null)
        {
            if (!GLControlSettings.WouldResultInSameContext(settings, _sharedContextSettings))
            {
                throw new ArgumentException("The provided GLWpfControlSettings would resultin a different context creation to one previously created. To fix this, either ensure all of your context settings are identical, or provide an external context via the 'ContextToUse' field.");
            }
        }
        else
        {
            Window window = Window.GetWindow(new DependencyObject());
            IntPtr parent = window == null ? IntPtr.Zero : new WindowInteropHelper(window).Handle;
            HwndSource hwndSource = new HwndSource(0, 0, 0, 0, 0, "GLWpfControl", parent);
            IWindowInfo windowInfo = Utilities.CreateWindowsWindowInfo(hwndSource.Handle);
            GraphicsContext graphicsContext = new GraphicsContext(new GraphicsMode(ColorFormat.Empty, 0, 0, 0, 0, 0, stereo: false), windowInfo, settings.MajorVersion, settings.MinorVersion, settings.GraphicsContextFlags);
            graphicsContext.LoadAll();
            graphicsContext.MakeCurrent(windowInfo);
            _sharedContext = graphicsContext;
            _sharedContextSettings = settings;
            _sharedContextResources = new IDisposable[3] { hwndSource, windowInfo, graphicsContext };
            _sharedContext.MakeCurrent(windowInfo);
        }

        Interlocked.Increment(ref _sharedContextReferenceCount);
        return _sharedContext;
    }

    public void Dispose()
    {
        if (_sharedContext == GraphicsContext && Interlocked.Decrement(ref _sharedContextReferenceCount) == 0)
        {
            IDisposable[] sharedContextResources = _sharedContextResources;
            for (int i = 0; i < sharedContextResources.Length; i++)
            {
                sharedContextResources[i].Dispose();
            }
        }
    }
}