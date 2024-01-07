using OpenTK.Platform.Windows;
using System.Diagnostics;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows;
using System;
using OpenTK.Graphics.OpenGL;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections.Generic;

namespace Mustard.UIExtension.PlotControl.GLBase;

public class GLControlRenderer
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly DxGlContext _context;
    private bool _isDisposed;
    private TimeSpan _lastFrameStamp;

    internal DxGLFramebuffer _framebuffer;

    public int FrameBufferHandle => _framebuffer?.GLFramebufferHandle ?? 0;

    public int Width => _framebuffer?.FramebufferWidth ?? 0;

    public int Height => _framebuffer?.FramebufferHeight ?? 0;

    public event Action GLRender;

    public event Action GLAsyncRender;

    public GLControlRenderer(GLControlSettings settings)
    {
        _context = new DxGlContext(settings);
    }

    public void SetSize(int width, int height, double dpiScaleX, double dpiScaleY)
    {
        if (_framebuffer == null || _framebuffer.Width != width || _framebuffer.Height != height)
        {
            _framebuffer?.Dispose();
            if (width > 0 && height > 0)
            {
                _framebuffer = new DxGLFramebuffer(_context, width, height, dpiScaleX, dpiScaleY);
                _isDisposed = false;
            }
        }
    }

    public void Render(DrawingContext drawingContext)
    {
        try
        {
            if (_isDisposed) return;
            if (_framebuffer != null)
            {
                PreRender();
                GLRender?.Invoke();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Flush();
                GLAsyncRender?.Invoke();
                PostRender();
                drawingContext.PushTransform(_framebuffer.TranslateTransform);
                drawingContext.PushTransform(_framebuffer.FlipYTransform);
                drawingContext.DrawImage(rectangle: new Rect(0.0, 0.0, (int)_framebuffer.RenderD3dImage.Width, (int)_framebuffer.RenderD3dImage.Height), imageSource: _framebuffer.RenderD3dImage);
                drawingContext.Pop();
                drawingContext.Pop();
            }
        }
        catch
        {
        }
    }

    private void PreRender()
    {
        if (_isDisposed) return;
        _framebuffer.RenderD3dImage.Lock();
        Wgl.DXLockObjectsNV(_context.GlDeviceHandle, 1, new IntPtr[1] { _framebuffer.DxInteropRegisteredHandle });
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer.GLFramebufferHandle);
        GL.Viewport(0, 0, _framebuffer.FramebufferWidth, _framebuffer.FramebufferHeight);
    }

    private void PostRender()
    {
        if (_isDisposed) return;
        Wgl.DXUnlockObjectsNV(_context.GlDeviceHandle, 1, new IntPtr[1] { _framebuffer.DxInteropRegisteredHandle });
        _framebuffer.RenderD3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, _framebuffer.DxRenderTargetHandle);
        _framebuffer.RenderD3dImage.AddDirtyRect(new Int32Rect(0, 0, _framebuffer.FramebufferWidth, _framebuffer.FramebufferHeight));
        _framebuffer.RenderD3dImage.Unlock();
    }

    public void Dispose()
    {
        if (_framebuffer != null)
        {
            _framebuffer.Dispose();
            _framebuffer = null;
            _isDisposed = true;
        }
    }
}