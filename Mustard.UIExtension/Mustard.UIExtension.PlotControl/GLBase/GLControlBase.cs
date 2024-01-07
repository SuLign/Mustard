using OpenTK.Graphics.OpenGL;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace Mustard.UIExtension.PlotControl.GLBase;

public class GLControlBase : FrameworkElement
{
    private GLControlSettings _settings;
    private GLControlRenderer _renderer;
    private bool _needsRedraw;

    public int Framebuffer => _renderer?.FrameBufferHandle ?? 0;

    public bool RenderContinuously
    {
        get
        {
            return _settings.RenderContinuously;
        }
        set
        {
            _settings.RenderContinuously = value;
        }
    }

    public int FrameBufferWidth => _renderer?.Width ?? 0;

    public int FrameBufferHeight => _renderer?.Height ?? 0;

    public event Action Render;

    public event Action AsyncRender;

    public event Action Ready;

    public void Start(GLControlSettings settings)
    {
        if (_settings != null)
        {
            throw new InvalidOperationException("Start must only be called once for a given GLControlBase");
        }

        _settings = settings.Copy();
        _needsRedraw = settings.RenderContinuously;
        _renderer = new GLControlRenderer(_settings);
        _renderer.GLRender += () =>
        {
            Render?.Invoke();
        };
        _renderer.GLAsyncRender += delegate
        {
            AsyncRender?.Invoke();
        };
        IsVisibleChanged += delegate (object _, DependencyPropertyChangedEventArgs args)
        {
            if ((bool)args.NewValue)
            {
                CompositionTarget.Rendering += OnCompTargetRender;
            }
            else
            {
                CompositionTarget.Rendering -= OnCompTargetRender;
            }
        };
        Loaded += delegate
        {
            _needsRedraw = true;
        };
        Unloaded += delegate
        {
            OnUnloaded();
        };
        Ready?.Invoke();
    }

    private void SetupRenderSize()
    {
        if (_renderer == null || _settings == null)
        {
            return;
        }
        double dpiScaleX = 1.0;
        double dpiScaleY = 1.0;
        if (_settings.UseDeviceDpi)
        {
            PresentationSource presentationSource = PresentationSource.FromVisual(this);
            if (presentationSource != null)
            {
                Matrix transformToDevice = presentationSource.CompositionTarget.TransformToDevice;
                dpiScaleX = transformToDevice.M11;
                dpiScaleY = transformToDevice.M22;
            }
        }
        //var ah = (int)RenderSize.Height;
        //var aw = (int)RenderSize.Width;
        _renderer?.SetSize((int)RenderSize.Width, (int)RenderSize.Height, dpiScaleX, dpiScaleY);
    }

    private void OnUnloaded()
    {
        //_renderer?.SetSize(0, 0, 1.0, 1.0); // 这行代码太坑了，很容易引起PresentationCore的访问冲突。━━(￣ー￣*|||━━
        _renderer?.Dispose();
    }

    public BitmapSource RenderToBitmap()
    {
        var fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        var texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            (int)ActualWidth,
            (int)ActualHeight,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            IntPtr.Zero);
        GL.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            texture,
            0);
        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
        {
            return null;
        }
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        Render?.Invoke();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        var pixels = new byte[(int)(ActualWidth * ActualHeight) * 4];
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        GL.ReadPixels(0, 0, (int)ActualWidth, (int)ActualHeight, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

        for (int i = 0; i < ActualHeight / 2; i++)
        {
            for (int j = 0; j < ActualWidth * 4; j++)
            {
                byte temp = pixels[(int)(i * 4 * ActualWidth + j)];
                pixels[(int)(i * 4 * ActualWidth + j)] = pixels[(int)((ActualHeight - i - 1) * ActualWidth * 4 + j)];
                pixels[(int)((ActualHeight - i - 1) * ActualWidth * 4 + j)] = temp;
            }
        }
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.DeleteFramebuffer(fbo);
        GL.DeleteTexture(texture);
        return BitmapSource.Create((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgra32, null, pixels, (int)ActualWidth * 4);
    }

    private void OnCompTargetRender(object sender, EventArgs e)
    {
        if (_needsRedraw)
        {
            InvalidateVisual();
            _needsRedraw = false;
        }
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (GetSystemMetrics(0x1000))
        {
            drawingContext.DrawRectangle(Brushes.Black, new Pen(Brushes.Black, 1), new Rect(new Size(ActualWidth, ActualHeight)));
            var bitmap = RenderToBitmap();
            drawingContext.DrawImage(bitmap, new Rect(new Size(ActualWidth, ActualHeight)));
        }
        else
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                DesignTimeHelper.DrawDesignTimeHelper(this, drawingContext);
            }
            else if (_renderer != null)
            {
                SetupRenderSize();
                _renderer?.Render(drawingContext);
            }
            else
            {
                UnstartedControlHelper.DrawUnstartedControlHelper(this, drawingContext);
            }
            base.OnRender(drawingContext);
        }
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo info)
    {
        if (!DesignerProperties.GetIsInDesignMode(this))
        {
            if ((info.WidthChanged || info.HeightChanged) && info.NewSize.Width > 0.0 && info.NewSize.Height > 0.0)
            {
                _needsRedraw = true;
            }
            base.OnRenderSizeChanged(info);
        }
    }

    protected virtual void DoRender()
    {
        _needsRedraw = true;
    }

    [DllImport("user32.dll")]
    private static extern bool GetSystemMetrics(int nIndex);
}
