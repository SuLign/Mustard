using Mustard.UIExtension.PlotControl.GLBase.Interop;

using OpenTK.Graphics.OpenGL;
using OpenTK.Platform.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

using static Mustard.UIExtension.PlotControl.GLBase.User32;

namespace Mustard.UIExtension.PlotControl.GLBase;

internal sealed class DxGLFramebuffer : IDisposable
{
    private DxGlContext DxGlContext { get; }

    public int FramebufferWidth { get; }

    public int FramebufferHeight { get; }

    public int Width { get; }

    public int Height { get; }

    public IntPtr DxRenderTargetHandle { get; }

    public int GLFramebufferHandle { get; }

    public int GLSharedTextureHandle { get; }

    private int GLDepthRenderBufferHandle { get; }

    public IntPtr DxInteropRegisteredHandle { get; }

    public D3DImage RenderD3dImage { get; }

    public TranslateTransform TranslateTransform { get; }

    public ScaleTransform FlipYTransform { get; }

    public DxGLFramebuffer(DxGlContext context, int width, int height, double dpiScaleX, double dpiScaleY)
    {
        DxGlContext = context;
        Width = width;
        Height = height;
        FramebufferWidth = (int)Math.Ceiling(width * dpiScaleX);
        FramebufferHeight = (int)Math.Ceiling(height * dpiScaleY);
        IntPtr sharedHandle = IntPtr.Zero;
        DXInterop.CreateRenderTarget(context.DxDeviceHandle, FramebufferWidth, FramebufferHeight, Format.A8R8G8B8, MultisampleType.None, 0, lockable: false, out var surfaceHandle, ref sharedHandle);
        DxRenderTargetHandle = surfaceHandle;
        Wgl.DXSetResourceShareHandleNV(surfaceHandle, sharedHandle);
        GLFramebufferHandle = GL.GenFramebuffer();
        GLSharedTextureHandle = GL.GenTexture();
        DxInteropRegisteredHandle = Wgl.DXRegisterObjectNV(context.GlDeviceHandle, surfaceHandle, (uint)GLSharedTextureHandle, 3553u, WGL_NV_DX_interop.AccessReadWrite);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, GLFramebufferHandle);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, GLSharedTextureHandle, 0);
        GLDepthRenderBufferHandle = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, GLDepthRenderBufferHandle);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, FramebufferWidth, FramebufferHeight);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, GLDepthRenderBufferHandle);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        HandleRef hWnd = new HandleRef(null, IntPtr.Zero);
        IntPtr dC = GetDC(hWnd);
        var dpiX = GetDeviceCaps(new HandleRef(null, dC), 88);
        RenderD3dImage = new D3DImage(dpiX * dpiScaleX, dpiX * dpiScaleY);
        TranslateTransform = new TranslateTransform(0.0, height);
        FlipYTransform = new ScaleTransform(1.0, -1.0);
    }

    public void Dispose()
    {
        GL.DeleteFramebuffer(GLFramebufferHandle);
        GL.DeleteRenderbuffer(GLDepthRenderBufferHandle);
        GL.DeleteTexture(GLSharedTextureHandle);
        Wgl.DXUnregisterObjectNV(DxGlContext.GlDeviceHandle, DxInteropRegisteredHandle);
        DXInterop.Release(DxRenderTargetHandle);
    }
}
