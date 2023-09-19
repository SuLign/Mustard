using SharpGL.Enumerations;
using SharpGL.WPF;
using SharpGL;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using Mustard.UIExtension.PlotControl.EventArgs;
using System.Windows.Data;

namespace Mustard.UIExtension.PlotControl;


public delegate void PlotRoutedEventHandler(object sender, PlotRoutedEventArgs e);

public class Spectrum : UserControl
{
    private const float DefaultHoriSideDis = 20;

    #region Private Field
    private double widthScale = 1;
    private double heightScale = 1;
    private double horiShift = 0;
    private double vertShift = 0;

    private double o_widthScale = -1;
    private double o_heightScale = -1;
    private double o_horiShift = -1;
    private double o_vertShift = -1;

    private double unitWidth = 1;
    private float horiSideDis = 20;
    private ConcurrentDictionary<int, List<Point>> points;
    private ConcurrentDictionary<int, Color> lineColors;
    private ConcurrentDictionary<int, bool> lineVisiables;
    private ConcurrentDictionary<string, Tuple<Point, string, Color>> marks;
    private long timeStamp;
    private Label mouselb;
    private bool traceStart = false;
    private Point tracePoint;
    private int clickedCount;
    private DateTime lastClickTime;
    private OpenGLControl glControl;
    #endregion

    #region Routed Event
    private static readonly RoutedEvent MouseClickEvent = EventManager.RegisterRoutedEvent("MouseClickEvent", RoutingStrategy.Direct, typeof(PlotRoutedEventHandler), typeof(Spectrum));
    #endregion

    #region Dependency Property
    public static DependencyProperty BoardBackgroundProperty;
    public static DependencyProperty LineColorProperty;
    public static DependencyProperty XAxisLineColorProperty;
    public static DependencyProperty YAxisLineColorProperty;
    public static DependencyProperty XAxisScaleColorProperty;
    public static DependencyProperty YAxisScaleColorProperty;
    public static DependencyProperty HoriGridLineVisiableProperty;
    public static DependencyProperty VertGridLineVisiableProperty;
    public static DependencyProperty HoriGridLineColorProperty;
    public static DependencyProperty VertGridLineColorProperty;
    public static DependencyProperty AutoAdjustGraphicsAfterUpdateProperty;
    #endregion

    #region Properties
    public Color BoardBackground
    {
        get => (Color)GetValue(BoardBackgroundProperty);
        set => SetValue(BoardBackgroundProperty, value);
    }
    public Color LineColor
    {
        get => (Color)GetValue(LineColorProperty);
        set => SetValue(LineColorProperty, value);
    }
    public Color XAxisLineColor
    {
        get => (Color)GetValue(XAxisLineColorProperty);
        set => SetValue(XAxisLineColorProperty, value);
    }
    public Color YAxisLineColor
    {
        get => (Color)GetValue(YAxisLineColorProperty);
        set => SetValue(YAxisLineColorProperty, value);
    }
    public Color XAxisScaleColor
    {
        get => (Color)GetValue(XAxisScaleColorProperty);
        set => SetValue(XAxisScaleColorProperty, value);
    }
    public Color YAxisScaleColor
    {
        get => (Color)GetValue(YAxisScaleColorProperty);
        set => SetValue(YAxisScaleColorProperty, value);
    }
    public bool HoriGridLineVisiable
    {
        get => (bool)GetValue(HoriGridLineVisiableProperty);
        set => SetValue(HoriGridLineVisiableProperty, value);
    }
    public bool VertGridLineVisiable
    {
        get => (bool)GetValue(VertGridLineVisiableProperty);
        set => SetValue(VertGridLineVisiableProperty, value);
    }
    public Color HoriGridLineColor
    {
        get => (Color)GetValue(HoriGridLineColorProperty);
        set => SetValue(HoriGridLineColorProperty, value);
    }
    public Color VertGridLineColor
    {
        get => (Color)GetValue(VertGridLineColorProperty);
        set => SetValue(VertGridLineColorProperty, value);
    }
    public bool AutoAdjustGraphicsAfterUpdate
    {
        get => (bool)GetValue(AutoAdjustGraphicsAfterUpdateProperty);
        set => SetValue(AutoAdjustGraphicsAfterUpdateProperty, value);
    }
    #endregion

    #region Event
    public event PlotRoutedEventHandler MouseClick
    {
        add { AddHandler(MouseClickEvent, value); }
        remove { RemoveHandler(MouseClickEvent, value); }
    }
    #endregion

    static Spectrum()
    {
        var type = typeof(Spectrum);
        var staticFields = type.GetFields();
        var properties = type.GetProperties();
        var methods = type.GetMethods();

        foreach (var property in properties)
        {
            var met = methods.FirstOrDefault(e => e.IsStatic && e.Name == $"Set{property.Name}");
            PropertyChangedCallback call = (met == null ? null : (PropertyChangedCallback)met.CreateDelegate(typeof(PropertyChangedCallback)));
            RegistProperty(property, null, call);
        }

        void RegistProperty(PropertyInfo proInfo, object defaultVal = null, PropertyChangedCallback callback = null)
        {
            var depField = staticFields.FirstOrDefault(e => e.IsStatic && (e.Name == $"{proInfo.Name}Property"));
            if (depField == null) return;
            var frameworkProMeta = new FrameworkPropertyMetadata();
            if (defaultVal != null)
            {
                frameworkProMeta.DefaultValue = defaultVal;
            }
            if (callback != null)
            {
                frameworkProMeta.PropertyChangedCallback = callback;
            }
            var dep = DependencyProperty.Register(
                proInfo.Name,
                proInfo.PropertyType,
                typeof(Spectrum), frameworkProMeta
                );
            depField.SetValue(null, dep);
        }
    }

    public Spectrum()
    {
        marks = new ConcurrentDictionary<string, Tuple<Point, string, Color>>();
        lineColors = new ConcurrentDictionary<int, Color>();
        lineVisiables = new ConcurrentDictionary<int, bool>();
        BoardBackground = Color.FromArgb(255, 21, 21, 21);
        LineColor = Color.FromArgb(255, 240, 240, 160);
        XAxisLineColor = Color.FromArgb(255, 0, 192, 192);
        YAxisLineColor = Color.FromArgb(255, 0, 192, 192);
        HoriGridLineVisiable = true;
        VertGridLineVisiable = true;
        XAxisScaleColor = Color.FromArgb(255, 255, 255, 255);
        YAxisScaleColor = Color.FromArgb(255, 255, 255, 255);
        HoriGridLineColor = Color.FromArgb(100, 224, 224, 224);
        VertGridLineColor = Color.FromArgb(100, 224, 224, 224);
        AutoAdjustGraphicsAfterUpdate = true;
        mouselb = new Label() { Background = Brushes.Transparent, Visibility = Visibility.Collapsed };
        points = new ConcurrentDictionary<int, List<Point>>();
        points.TryAdd(0, new List<Point>());
        glControl = new OpenGLControl();
        glControl.FrameRate = 60;
        Canvas canvs = new Canvas();
        canvs.Children.Add(glControl);
        Binding canvsWidthBinding = new Binding("ActualWidth");
        canvsWidthBinding.Source = canvs;
        glControl.SetBinding(OpenGLControl.WidthProperty, canvsWidthBinding);
        Binding canvsHeightBinding = new Binding("ActualHeight");
        canvsHeightBinding.Source = canvs;
        glControl.SetBinding(OpenGLControl.HeightProperty, canvsHeightBinding);
        this.Content = canvs;
        glControl.RenderContextType = RenderContextType.FBO;
        glControl.OpenGLDraw += GlControl_OpenGLDraw;
        glControl.Resized += GlControl_Resized;
        SizeChanged += GlControl_SizeChanged;
        MouseWheel += GlControl_MouseWheel;
        MouseMove += GlControl_MouseMove;
        SizeChanged += GlControl_SizeChanged;
        MouseDoubleClick += GlControl_MouseDoubleClick;
        MouseDown += GlControl_MouseDown;
        MouseUp += GlControl_MouseUp;
    }

    private void GlControl_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Right)
        {
            RaiseEvent(new PlotRoutedEventArgs(MouseClickEvent, tracePoint));
        }
    }

    private void GlControl_MouseDown(object sender, MouseButtonEventArgs e)
    {
    }

    private void GlControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        OnMouseDoubleClick(e);
    }

    private void GlControl_MouseMove(object sender, MouseEventArgs e)
    {
        mouselb.Background = new SolidColorBrush(Color.FromArgb(
            180,
            (byte)((BoardBackground.R + 85) % 256),
            (byte)((BoardBackground.G + 85) % 256),
            (byte)((BoardBackground.B + 85) % 256)
            ));
        var pos = Mouse.GetPosition(glControl);
        var x = (pos.X - (horiSideDis + horiShift)) / (100 * unitWidth * widthScale);
        var y = -(pos.Y - (-10 + vertShift + glControl.ActualHeight / 2)) / (100 * unitWidth * heightScale);
        Canvas.SetLeft(mouselb, pos.X + 10);
        Canvas.SetTop(mouselb, pos.Y + 10);
        if (mouselb.Visibility == Visibility.Collapsed) mouselb.Visibility = Visibility.Visible;
        mouselb.Content = $"({x:0.###}, {y:0.###})";
    }

    private void GlControl_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        var vspeed = (double)e.Delta / (DateTime.Now.Ticks - timeStamp);
        var mousePos = Mouse.GetPosition(glControl);
        mousePos.X = (int)(mousePos.X - horiSideDis);
        timeStamp = DateTime.Now.Ticks;
        if (Keyboard.Modifiers == ModifierKeys.Shift)
        {
            var s1 = vertShift;
            var scl = heightScale;
            var y = mousePos.Y - glControl.ActualHeight / 2;
            if (e.Delta > 0)
            {
                heightScale = heightScale * 1.1;
                if (heightScale > 100) heightScale = 100;
            }
            else if (e.Delta < 0)
            {
                heightScale = heightScale / 1.1;
                if (heightScale < 0.05) heightScale = 0.005;
            }
            vertShift = y - (y - s1) * (heightScale / scl);
        }
        else if (Keyboard.Modifiers == (ModifierKeys.Control))
        {
            horiShift = horiShift + 4000 * Math.Max(vspeed, 1) / e.Delta;
        }
        else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
        {
            vertShift = vertShift + 4000 * Math.Max(vspeed, 1) / e.Delta;
        }
        else
        {
            var s1 = horiShift;
            var scl = widthScale;
            if (e.Delta > 0)
            {
                widthScale = widthScale * 1.1;
                if (widthScale > 100) widthScale = 100;
            }
            else if (e.Delta < 0)
            {
                widthScale = widthScale / 1.1;
                if (widthScale < 0.005) widthScale = 0.005;
            }
            horiShift = mousePos.X - (mousePos.X - s1) * (widthScale / scl);
        }
        //needRender = true;
        glControl.DoRender();
    }

    private void GlControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        OpenGL gl = glControl.OpenGL;
        gl.MatrixMode(MatrixMode.Projection);
        gl.LoadIdentity();
        gl.Ortho(0, ActualWidth, ActualHeight, 0, -10, 10);
        gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
        gl.Enable(OpenGL.GL_BLEND);
        gl.Enable(OpenGL.GL_LINE_SMOOTH);
        gl.Hint(OpenGL.GL_LINE_SMOOTH_HINT, OpenGL.GL_NICEST);
        gl.MatrixMode(MatrixMode.Modelview);
        glControl.FrameRate = 120;
    }

    private void GlControl_OpenGLDraw(object sender, OpenGLRoutedEventArgs args)
    {
        var gl = glControl.OpenGL;
        //gl.Disable(OpenGL.GL_LINE_SMOOTH);
        gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
        gl.ClearColor(
            (float)BoardBackground.R / 255.0f,
            (float)BoardBackground.G / 255.0f,
            (float)BoardBackground.B / 255.0f,
            1f);
        gl.Color(
            (float)XAxisLineColor.R / 255.0f,
            (float)XAxisLineColor.G / 255.0f,
            (float)XAxisLineColor.B / 255.0f,
            1f);
        gl.LineWidth(1);
        gl.Begin(OpenGL.GL_LINES);
        gl.Vertex4f((int)horiSideDis, 0, 0, 1);
        gl.Vertex4f((int)horiSideDis, (int)(ActualHeight), 0, 1);
        gl.End();

        gl.Color(
            (float)YAxisLineColor.R / 255.0f,
            (float)YAxisLineColor.G / 255.0f,
            (float)YAxisLineColor.B / 255.0f,
            1f);
        gl.LineWidth(1);
        gl.Begin(OpenGL.GL_LINES);
        gl.Vertex4f((int)horiSideDis, (int)(ActualHeight - 20), 0, 1);
        gl.Vertex4f((int)(ActualWidth), (int)(ActualHeight - 20), 0, 1);
        gl.End();
        var horiScalesLst = new List<Tuple<double, string>>();
        var vertScalesLst = new List<Tuple<double, string>>();
        var hspanCount = (int)(ActualWidth - horiSideDis) / (100 * unitWidth * widthScale);
        var vspanCount = (int)(ActualHeight - 20) / (100 * unitWidth * heightScale);
        var hSpan = 1.0;
        var vSpan = 1.0;
        if (widthScale < 1)
        {
            hSpan = Math.Pow(10, GetDecimalExponent(0.3 / (widthScale * widthScale)));
        }
        else
        {
            hSpan = Math.Pow(10, GetDecimalExponent(0.3 / widthScale));
        }
        if (heightScale < 1)
        {
            vSpan = Math.Pow(10, GetDecimalExponent(0.3 / (heightScale * heightScale)));
        }
        else
        {
            vSpan = Math.Pow(10, GetDecimalExponent(0.3 / heightScale));
        }
        var hnegetiveSpanCount = horiShift / (100 * unitWidth * widthScale);
        var vnegetiveSpanCount = (-vertShift + glControl.ActualHeight / 2) / (100 * unitWidth * heightScale);
        for (double i = 0; i < hnegetiveSpanCount; i += hSpan)
        {
            var horiCoor = horiSideDis + horiShift + 100 * (-i - hSpan) * unitWidth * widthScale;
            if (horiCoor >= horiSideDis && horiCoor <= glControl.ActualWidth)
            {
                gl.Color(
                    (float)HoriGridLineColor.R / 255.0f,
                    (float)HoriGridLineColor.G / 255.0f,
                    (float)HoriGridLineColor.B / 255.0f,
                    (float)HoriGridLineColor.A / 255.0f);
                gl.LineWidth(1f);
                gl.Begin(OpenGL.GL_LINES);
                if (VertGridLineVisiable)
                {
                    gl.Vertex4f((float)(horiSideDis + horiShift + 100 * (-i - hSpan) * unitWidth * widthScale), (float)glControl.ActualHeight - 20, 0, 1);
                    gl.Vertex4f((float)(horiSideDis + horiShift + 100 * (-i - hSpan) * unitWidth * widthScale), 0, 0, 1);
                }
                gl.Vertex4f((float)(horiSideDis + horiShift + 100 * (-i - hSpan) * unitWidth * widthScale), (float)(glControl.ActualHeight - 20), 0, 1);
                gl.Vertex4f((float)(horiSideDis + horiShift + 100 * (-i - hSpan) * unitWidth * widthScale), (float)(glControl.ActualHeight - 25), 0, 1);
                gl.End();
                horiScalesLst.Add(new Tuple<double, string>((int)horiCoor, (-hSpan - i).ToString("0.####")));
            }
        }
        for (double i = 0; i < hspanCount - hnegetiveSpanCount; i += hSpan)
        {
            var horiCoor = horiSideDis + horiShift + 100 * i * unitWidth * widthScale;
            if (horiCoor >= horiSideDis && horiCoor <= glControl.ActualWidth)
            {
                gl.Color(
                    (float)HoriGridLineColor.R / 255.0f,
                    (float)HoriGridLineColor.G / 255.0f,
                    (float)HoriGridLineColor.B / 255.0f,
                    (float)HoriGridLineColor.A / 255.0f);
                gl.LineWidth(1f);
                gl.Begin(OpenGL.GL_LINES);
                if (VertGridLineVisiable)
                {
                    gl.Vertex4f((float)(horiSideDis + horiShift + 100 * i * unitWidth * widthScale), (float)(glControl.ActualHeight - 20), 0, 1);
                    gl.Vertex4f((float)(horiSideDis + horiShift + 100 * i * unitWidth * widthScale), 0, 0, 1);
                }
                gl.Vertex4f((float)(horiSideDis + horiShift + 100 * i * unitWidth * widthScale), (float)(glControl.ActualHeight - 20), 0, 1);
                gl.Vertex4f((float)(horiSideDis + horiShift + 100 * i * unitWidth * widthScale), (float)(glControl.ActualHeight - 25), 0, 1);
                gl.End();
                horiScalesLst.Add(new Tuple<double, string>((int)horiCoor, i.ToString("0.####")));
            }
        }
        for (double i = 0; i < vnegetiveSpanCount; i += vSpan)
        {
            gl.Color(
                (float)VertGridLineColor.R / 255.0f,
                (float)VertGridLineColor.G / 255.0f,
                (float)VertGridLineColor.B / 255.0f,
                (float)VertGridLineColor.A / 255.0f);
            var vertCoor = -10 + vertShift - 100 * (-i - vSpan) * unitWidth * heightScale + glControl.ActualHeight / 2;
            if (vertCoor >= 0 && vertCoor <= glControl.ActualHeight - 20)
            {
                gl.LineWidth(1f);
                gl.Begin(OpenGL.GL_LINES);
                if (HoriGridLineVisiable)
                {
                    gl.Vertex4f((float)glControl.ActualWidth, (float)vertCoor, 0, 1);
                    gl.Vertex4f(horiSideDis, (float)vertCoor, 0, 1);
                }
                gl.Vertex4f(horiSideDis, (float)(-10 + vertShift - 100 * (-i - vSpan) * unitWidth * heightScale + glControl.ActualHeight / 2), 0, 1);
                gl.Vertex4f(horiSideDis + 5, (float)(-10 + vertShift - 100 * (-i - vSpan) * unitWidth * heightScale + glControl.ActualHeight / 2), 0, 1);
                gl.End();
                vertScalesLst.Add(new Tuple<double, string>((int)(10 - vertShift + 100 * (-i - vSpan) * unitWidth * heightScale + glControl.ActualHeight / 2), (-i - vSpan).ToString("0.####")));
            }
        }
        for (double i = 0; i < vspanCount - vnegetiveSpanCount; i += vSpan)
        {
            if (HoriGridLineVisiable)
            {
                var vertCoor = -10 + vertShift + glControl.ActualHeight / 2 - 100 * i * unitWidth * heightScale;
                if (vertCoor >= 0 && vertCoor <= glControl.ActualHeight - 20)
                {
                    gl.Color(
                        (float)VertGridLineColor.R / 255.0f,
                        (float)VertGridLineColor.G / 255.0f,
                        (float)VertGridLineColor.B / 255.0f,
                        (float)VertGridLineColor.A / 255.0f);
                    gl.LineWidth(1f);
                    gl.Begin(OpenGL.GL_LINES);
                    gl.Vertex4f((float)glControl.ActualWidth, (float)(-10 + vertShift + glControl.ActualHeight / 2 - 100 * i * unitWidth * heightScale), 0, 1);
                    gl.Vertex4f(horiSideDis, (float)(-10 + vertShift + glControl.ActualHeight / 2 - 100 * i * unitWidth * heightScale), 0, 1);
                    gl.Vertex4f((float)horiSideDis, (float)(-10 + vertShift + glControl.ActualHeight / 2 - 100 * i * unitWidth * heightScale), 0, 1);
                    gl.Vertex4f((float)(horiSideDis + 5), (float)(-10 + vertShift + glControl.ActualHeight / 2 - 100 * i * unitWidth * heightScale), 0, 1);
                    gl.End();
                    vertScalesLst.Add(new Tuple<double, string>((int)(10 - vertShift + 100 * i * unitWidth * heightScale + glControl.ActualHeight / 2), i.ToString("0.####")));
                }
            }
        }
        gl.Color(
            (float)LineColor.R / 255.0f,
            (float)LineColor.G / 255.0f,
            (float)LineColor.B / 255.0f,
            (float)LineColor.A / 255.0f);
        gl.LineWidth(1f);
        gl.Enable(OpenGL.GL_BLEND);
        gl.Enable(OpenGL.GL_LINE_SMOOTH);
        gl.Hint(OpenGL.GL_LINE_SMOOTH_HINT, OpenGL.GL_NICEST);
        foreach (var item in points)
        {
            if (lineVisiables.ContainsKey(item.Key))
            {
                if (!lineVisiables[item.Key]) continue;
            }
            gl.Begin(OpenGL.GL_LINE_STRIP);
            if (lineColors.ContainsKey(item.Key))
            {
                gl.Color(
                (float)lineColors[item.Key].R / 255.0f,
                (float)lineColors[item.Key].G / 255.0f,
                (float)lineColors[item.Key].B / 255.0f,
                (float)lineColors[item.Key].A / 255.0f);
            }
            for (int i = 0; i < item.Value.Count; i++)
            {
                gl.Vertex4f(
                    horiSideDis + (float)(item.Value[i].X * 100 * unitWidth * widthScale + horiShift),
                    -10 + (float)(vertShift + glControl.ActualHeight / 2 - heightScale * 100 * (float)(item.Value[i].Y * unitWidth)),
                    0,
                    1);
            }
            gl.End();
        }
        if (
            o_heightScale == heightScale &&
            o_widthScale == widthScale &&
            o_horiShift == horiShift &&
            o_vertShift == vertShift
            )
        {
            foreach (var item in marks)
            {
                var (pos, txt, color) = item.Value;
                gl.DrawText(
                    (int)(horiSideDis + (float)(pos.X * 100 * unitWidth * widthScale + horiShift)),
                    (int)(10 - (float)(vertShift - glControl.ActualHeight / 2 - heightScale * 100 * (float)(pos.Y * unitWidth))),
                    color.R / 255.0f,
                    color.G / 255.0f,
                    color.B / 255.0f,
                    "Microsoft Yahei",
                    12,
                    txt);
                gl.Color(
                    color.R / 255.0f,
                    color.G / 255.0f,
                    color.B / 255.0f);
                gl.PointSize(3);
                gl.Begin(OpenGL.GL_POINTS);
                gl.Vertex4f(
                    horiSideDis + (float)(pos.X * 100 * unitWidth * widthScale + horiShift),
                    -10 + (float)(vertShift + glControl.ActualHeight / 2 - heightScale * 100 * (float)(pos.Y * unitWidth)),
                    0,
                    1);
                gl.End();
            }
        }
        o_heightScale = heightScale;
        o_widthScale = widthScale;
        o_horiShift = horiShift;
        o_vertShift = vertShift;
        gl.Color(
            (float)BoardBackground.R / 255.0f,
            (float)BoardBackground.G / 255.0f,
            (float)BoardBackground.B / 255.0f,
            1f);
        gl.Begin(OpenGL.GL_QUADS);
        gl.Vertex4f(0, 0, 0, 1);
        gl.Vertex4f(horiSideDis, 0, 0, 1);
        gl.Vertex4f(horiSideDis, (float)glControl.ActualHeight, 0, 1);
        gl.Vertex4f(0, (float)glControl.ActualHeight, 0, 1);
        gl.Vertex4f(0, (float)glControl.ActualHeight, 0, 1);
        gl.Vertex4f(0, (float)(glControl.ActualHeight - 20), 0, 1);
        gl.Vertex4f((float)glControl.ActualWidth, (float)(glControl.ActualHeight - 20), 0, 1);
        gl.Vertex4f((float)glControl.ActualWidth, (float)glControl.ActualHeight, 0, 1);
        gl.End();
        foreach (var item in horiScalesLst)
        {
            var (x, txt) = item;
            gl.DrawText(
                (int)x,
                (int)(10),
                (float)XAxisScaleColor.R / 255.0f,
                (float)XAxisScaleColor.G / 255.0f,
                (float)XAxisScaleColor.B / 255.0f,
                "Microsoft Yahei",
                12,
                txt);
        }
        float maxTxtWide = 0;
        foreach (var item in vertScalesLst)
        {
            var (y, txt) = item;
            gl.DrawText(
                (int)0,
                (int)y,
                (float)YAxisScaleColor.R / 255.0f,
                (float)YAxisScaleColor.G / 255.0f,
                (float)YAxisScaleColor.B / 255.0f,
                "Microsoft Yahei",
                12,
                txt);
            var txtLength = txt.ToString().Length * 0.5 * 12;
            maxTxtWide = (float)Math.Max(txtLength, maxTxtWide);
        }
        if (maxTxtWide >= horiSideDis)
        {
            horiSideDis = (float)maxTxtWide;
        }
        else
        {
            horiSideDis = DefaultHoriSideDis;
        }
        gl.Color(
          0.8f,
          0.8f,
          0.8f,
          0.53f);
        if (!traceStart && (Mouse.RightButton == MouseButtonState.Pressed))
        {
            var pos = Mouse.GetPosition(glControl);
            var x = (pos.X - (horiSideDis + horiShift)) / (100 * unitWidth * widthScale);
            var y = -(pos.Y - (-10 + vertShift + glControl.ActualHeight / 2)) / (100 * unitWidth * heightScale);
            tracePoint = new Point((float)x, (float)y);
            if (!traceStart) traceStart = true;
        }
        else if (traceStart)
        {
            var p = Mouse.GetPosition(glControl);
            var x = (p.X - (horiSideDis + horiShift)) / (100 * unitWidth * widthScale);
            var y = -(p.Y - (-10 + vertShift + glControl.ActualHeight / 2)) / (100 * unitWidth * heightScale);
            var pos = new Point((float)x, (float)y);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Vertex4f(
                horiSideDis + (float)(pos.X * 100 * unitWidth * widthScale + horiShift),
                -10 + (float)(vertShift + glControl.ActualHeight / 2 - heightScale * 100 * (float)(pos.Y * unitWidth)),
                0,
                1);

            gl.Vertex4f(
                horiSideDis + (float)(pos.X * 100 * unitWidth * widthScale + horiShift),
                -10 + (float)(vertShift + glControl.ActualHeight / 2 - heightScale * 100 * (float)(tracePoint.Y * unitWidth)),
                0,
                1);
            gl.Vertex4f(
                horiSideDis + (float)(tracePoint.X * 100 * unitWidth * widthScale + horiShift),
                -10 + (float)(vertShift + glControl.ActualHeight / 2 - heightScale * 100 * (float)(tracePoint.Y * unitWidth)),
                0,
                1);
            gl.Vertex4f(
                horiSideDis + (float)(tracePoint.X * 100 * unitWidth * widthScale + horiShift),
                -10 + (float)(vertShift + glControl.ActualHeight / 2 - heightScale * 100 * (float)(pos.Y * unitWidth)),
                0,
                1);
            gl.End();
            if (traceStart && (Mouse.RightButton == MouseButtonState.Released))
            {
                traceStart = false;
                var pminx = Math.Min(tracePoint.X, pos.X);
                var pmaxx = Math.Max(tracePoint.X, pos.X);
                var pminy = Math.Min(tracePoint.Y, pos.Y);
                var pmaxy = Math.Max(tracePoint.Y, pos.Y);

                if ((pmaxx - pminx) * 100 * unitWidth * widthScale > 10 &&
                    (pmaxy - pminy) * heightScale * unitWidth * 100 > 10)
                {
                    AddjustGraphicPosAndScale(pminx, pmaxx, pminy, pmaxy);
                }
                else
                {
                    if ((pmaxx - pminx) * 100 * unitWidth * widthScale == 0 &&
                    (pmaxy - pminy) * heightScale * unitWidth * 100 == 0)
                        if ((DateTime.Now - lastClickTime).TotalMilliseconds < 400)
                        {
                            clickedCount++;
                        }
                        else
                        {
                            clickedCount = 1;
                        }
                    lastClickTime = DateTime.Now;
                    if (clickedCount == 2)
                    {
                        AddjustGraphicPosAndScale();
                        clickedCount = 0;
                    }
                }
            }
        }
        gl.Flush();
    }

    private void GlControl_Resized(object sender, OpenGLRoutedEventArgs args)
    {
        OpenGL gl = glControl.OpenGL;
        gl.MatrixMode(MatrixMode.Projection);
        gl.LoadIdentity();
        gl.Ortho(0, glControl.ActualWidth, glControl.ActualHeight, 0, -10, 10);
        gl.MatrixMode(MatrixMode.Modelview);
    }

    #region Display
    public void AddPoint(float x, float y, int lineIndex = 0)
    {
        if (!points.ContainsKey(lineIndex))
        {
            points.TryAdd(lineIndex, new List<Point>());
            if (!lineColors.ContainsKey(lineIndex))
            {
                Random random = new Random((int)DateTime.Now.Ticks);
                lineColors.TryAdd(lineIndex, Color.FromArgb(
                    255,
                    (byte)(255.0 * random.NextDouble()),
                    (byte)(255.0 * random.NextDouble()),
                    (byte)(255.0 * random.NextDouble())));
            }
        }
        points[lineIndex].Add(new Point(x, y));
        if (AutoAdjustGraphicsAfterUpdate) AddjustGraphicPosAndScale();
    }

    public void AddPoints(Point[] iPoints, int lineIndex = 0)
    {
        if (!points.ContainsKey(lineIndex)) points.TryAdd(lineIndex, new List<Point>());
        for (int i = 0; i < iPoints.Length; i++)
        {
            points[lineIndex].Add(iPoints[i]);
            if (AutoAdjustGraphicsAfterUpdate) AddjustGraphicPosAndScale();
        }
    }

    public void Clear()
    {
        points.Clear();
        marks.Clear();
    }

    /// <summary>
    /// 自动调整图像显示大小和尺寸
    /// </summary>
    public void AddjustGraphicPosAndScale(
        double pminx = double.MaxValue,
        double pmaxx = double.MinValue,
        double pminy = double.MaxValue,
        double pmaxy = double.MinValue
        )
    {
        var width = 0.0;
        var height = 0.0;
        if (pminx == double.MaxValue &&
           pmaxx == double.MinValue &&
           pminy == double.MaxValue &&
           pmaxy == double.MinValue)
        {
            if (points == null || points.Count == 0) return;
            pminx = double.MaxValue;
            pmaxx = double.MinValue;
            pminy = double.MaxValue;
            pmaxy = double.MinValue;
            foreach (var item in points)
            {
                if (item.Value.Count == 0) continue;
                pminx = Math.Min(pminx, item.Value.Min(e => e.X));
                pmaxx = Math.Max(pmaxx, item.Value.Max(e => e.X));
                pminy = Math.Min(pminy, item.Value.Min(e => e.Y));
                pmaxy = Math.Max(pmaxy, item.Value.Max(e => e.Y));
            }
        }
        width = pmaxx - pminx;
        height = pmaxy - pminy;

        widthScale = (glControl.ActualWidth - horiSideDis) / (width * 100 * unitWidth);
        heightScale = (glControl.ActualHeight - 20) / (height * 100 * unitWidth);

        horiShift = -pminx * 100 * unitWidth * widthScale;
        vertShift = pminy * 100 * unitWidth * heightScale + glControl.ActualHeight / 2 - 10;
    }

    public void FindPeekAndFloor(out List<int> peeks, out List<int> floors, int lineIndex = 0)
    {
        var kps = new List<double>();
        for (int i = 0; i < points[lineIndex].Count - 1; i++)
        {
            kps.Add((points[lineIndex][i + 1].Y - points[lineIndex][i].Y) / (points[lineIndex][i + 1].X - points[lineIndex][i].X));
        }
        peeks = new List<int>();
        floors = new List<int>();
        for (int i = 1; i < kps.Count - 1; i++)
        {
            if (kps[i - 1] * kps[i] <= 0 && !(kps[i - 1] == 0 && kps[i] == 0))
            {
                bool? dnPositive = null;
                bool? dpPositive = null;
                int jn = 1;
                while (i - jn >= 0)
                {
                    if (kps[i - jn] > 0)
                    {
                        dnPositive = true;
                        break;
                    }
                    else if (kps[i - jn] < 0)
                    {
                        dnPositive = false;
                        break;
                    }
                    else
                    {
                        jn++;
                        if (i - jn < 0) break;
                    }
                }
                if (dnPositive == null) continue;
                while (i < kps.Count)
                {
                    if (kps[i] > 0)
                    {
                        dpPositive = true;
                        break;
                    }
                    else if (kps[i] < 0)
                    {
                        dpPositive = false;
                        break;
                    }
                    else
                    {
                        i++;
                        if (i == kps.Count) break;
                    }
                }
                if (dpPositive == null) continue;
                if (!(bool)dnPositive && (bool)dpPositive)
                {
                    floors.Add(i);
                }
                if ((bool)dnPositive && !(bool)dpPositive)
                {
                    peeks.Add(i);
                }
                while (kps[i] == kps[i - 1])
                {
                    i++;
                    if (i == kps.Count) break;
                }
            }
        }
        peeks = peeks.OrderByDescending(e => points[lineIndex][e].Y).ToList();
        floors = floors.OrderBy(e => points[lineIndex][e].Y).ToList();
        for (int i = 0; i < peeks.Count; i++)
        {
            marks.TryAdd(Guid.NewGuid().ToString(), new Tuple<Point, string, Color>(points[lineIndex][peeks[i]], $"P {i}: {points[lineIndex][peeks[i]].X:0.##}, {points[lineIndex][peeks[i]].Y:0.##}", LineColor));
        }
        for (int i = 0; i < floors.Count; i++)
        {
            marks.TryAdd(Guid.NewGuid().ToString(), new Tuple<Point, string, Color>(points[lineIndex][floors[i]], $"F {i}: {points[lineIndex][floors[i]].X:0.##}, {points[lineIndex][floors[i]].Y:0.##}", LineColor));
        }
    }

    public string AddMarkToMousePos()
    {
        var pos = Mouse.GetPosition(glControl);
        var x = (pos.X - (horiSideDis + horiShift)) / (100 * unitWidth * widthScale);
        var y = -(pos.Y - (-10 + vertShift + glControl.ActualHeight / 2)) / (100 * unitWidth * heightScale);
        return AddMark(new Point((float)x, (float)y));
    }

    public string AddMarkToMousePos(string text)
    {
        var pos = Mouse.GetPosition(glControl);
        var x = (pos.X - (horiSideDis + horiShift)) / (100 * unitWidth * widthScale);
        var y = -(pos.Y - (-10 + vertShift + glControl.ActualHeight / 2)) / (100 * unitWidth * heightScale);
        return AddMark(new Point((float)x, (float)y), text);
    }

    public string AddMark(Point markPos)
    {
        var id = Guid.NewGuid().ToString();
        AddMark(id, markPos);
        return id;
    }

    public string AddMark(Point markPos, string text)
    {
        var id = Guid.NewGuid().ToString();
        AddMark(id, text, markPos);
        return id;
    }

    public void AddMark(string markID, Point markPos) => AddMark(markID, $"{markPos.X:0.##},{markPos.Y:0.##}", markPos);

    public void AddMark(string markID, string markText, Point markPos) => AddMark(markID, markText, markPos, LineColor);

    public void AddMark(string markID, string markText, Point markPos, Color markColor)
    {
        if (marks.ContainsKey(markID))
        {
            marks.TryUpdate(markID, new Tuple<Point, string, Color>(markPos, markText, markColor), marks[markID]);
        }
        else
        {
            marks.TryAdd(markID, new Tuple<Point, string, Color>(markPos, markText, markColor));
        }
    }

    public void RemoveMark(string markID)
    {
        if (marks.ContainsKey(markID))
        {
            marks.TryRemove(markID, out _);
        }
    }

    public void ClearMarks()
    {
        marks.Clear();
    }

    public void SetLineColor(int line, Color tlineColor)
    {
        if (lineColors.ContainsKey(line))
        {
            lineColors[line] = tlineColor;
        }
        else
        {
            lineColors.TryAdd(line, tlineColor);
        }
    }
    #endregion

    #region Base Functions
    private int GetDecimalExponent(double val)
    {
        var logv = Math.Log10(Math.Abs(val));
        if (logv - ((int)logv) == 0) return (int)logv;
        return (int)(logv);
    }
    #endregion
}
