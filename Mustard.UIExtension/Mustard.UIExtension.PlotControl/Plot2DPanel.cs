using Mustard.UIExtension.PlotControl.GLBase;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using static Mustard.Base.Toolset.WinAPI;

namespace Mustard.UIExtension.PlotControl;

public class Plot2DPanel : GLControlBase
{
    private static int markSgIndex = 0;

    private int frameworkGridVBO;
    private int frameworkGridVAO;
    private Shader panelShader;
    private Matrix4 model;
    private Matrix4 scaleMat4;
    private Matrix4 projection;
    private ManualResetEventSlim eventSlim;
    private List<PanelDrawingElement> gridEntities;
    private List<PanelDrawingElement> scaleEntities;
    private bool isUnloaded;
    private TextRender scaleTextRender;
    private TextRender markRender;
    private Thread mouseMonitorThread;
    private float xScale = 1;
    private float yScale = 1;
    private uint scaleFontSize = 20;
    private float horiShift;
    private float vertShift;
    private bool mouseDown;
    private float dsScale;
    private int leftSide = 60;
    private int bottomSide = 25;
    private int maxYScaleTextLength;
    private int randSeed = 1325512;
    private ConcurrentDictionary<int, ConcurrentQueue<Point>> dataDict;
    private ConcurrentDictionary<int, Color> colorDict;
    private List<int> visibleLineIndexes;
    private List<MarkTag> markTags;

    public static DependencyProperty BackBoardColorProperty
        = DependencyProperty.Register(
            "BackBoardColor",
            typeof(Color),
            typeof(Plot2DPanel),
            new FrameworkPropertyMetadata(DisplayChanged));

    public static DependencyProperty XScaleLenProperty
        = DependencyProperty.Register(
            "XScaleLen",
            typeof(double),
            typeof(Plot2DPanel),
            new FrameworkPropertyMetadata(0.2, DisplayChanged));

    public static DependencyProperty XAxisStartProperty
        = DependencyProperty.Register(
            "XAxisStart",
            typeof(double),
            typeof(Plot2DPanel),
            new FrameworkPropertyMetadata(0d, DisplayChanged));

    public static DependencyProperty XAxisStopProperty
        = DependencyProperty.Register(
            "XAxisStop",
            typeof(double),
            typeof(Plot2DPanel),
            new FrameworkPropertyMetadata(10d, DisplayChanged));

    public static DependencyProperty YScaleLenProperty
        = DependencyProperty.Register(
            "YScaleLen",
            typeof(double),
            typeof(Plot2DPanel),
            new FrameworkPropertyMetadata(0.2, DisplayChanged));

    public static DependencyProperty YAxisStartProperty
        = DependencyProperty.Register(
            "YAxisStart",
            typeof(double),
            typeof(Plot2DPanel),
            new FrameworkPropertyMetadata(0d, DisplayChanged));

    public static DependencyProperty YAxisStopProperty
        = DependencyProperty.Register(
            "YAxisStop",
            typeof(double),
            typeof(Plot2DPanel),
            new FrameworkPropertyMetadata(10d, DisplayChanged));
    private object lockObj;
    public static readonly DependencyProperty XAxisLineStrokeProperty
        = DependencyProperty.Register(
            "XAxisLineStroke",
            typeof(Color),
            typeof(Plot2DPanel),
            new FrameworkPropertyMetadata(Colors.Black, DisplayChanged));

    public static readonly DependencyProperty YAxisLineStrokeProperty
        = DependencyProperty.Register(
            "YAxisLineStroke",
            typeof(Color),
            typeof(Plot2DPanel),
            new FrameworkPropertyMetadata(Colors.Black, DisplayChanged));

    public static readonly DependencyProperty XAxisScaleForegroundProperty
        = DependencyProperty.Register(
            "XAxisScaleForeground",
            typeof(Color),
            typeof(Plot2DPanel),
            new FrameworkPropertyMetadata(Colors.Black, DisplayChanged));

    public static readonly DependencyProperty YAxisScaleForegroundProperty
        = DependencyProperty.Register(
            "YAxisScaleForeground",
            typeof(Color),
            typeof(Plot2DPanel),
            new FrameworkPropertyMetadata(Colors.Black, DisplayChanged));

    public static readonly DependencyProperty ShowMarksProperty
        = DependencyProperty.Register(
            "ShowMarks",
            typeof(bool),
            typeof(Plot2DPanel),
            new FrameworkPropertyMetadata(true, DisplayChanged));

    public static readonly DependencyProperty ShowXAxisTitleProperty
        = DependencyProperty.Register(
            "ShowXAxisTitle",
            typeof(bool),
            typeof(Plot2DPanel),
            new PropertyMetadata(true, DisplayChanged));

    public static readonly DependencyProperty ShowYAxisTitleProperty
        = DependencyProperty.Register(
            "ShowYAxisTitle",
            typeof(bool),
            typeof(Plot2DPanel),
            new PropertyMetadata(true, DisplayChanged));

    public static readonly DependencyProperty XAxisTitleProperty
        = DependencyProperty.Register(
            "XAxisTitle",
            typeof(string),
            typeof(Plot2DPanel),
            new PropertyMetadata("X轴", DisplayChanged));

    public static readonly DependencyProperty YAxisTitleProperty
        = DependencyProperty.Register(
            "YAxisTitle",
            typeof(string),
            typeof(Plot2DPanel),
            new PropertyMetadata("Y轴", DisplayChanged));

    public static readonly DependencyProperty SizeToDataAreaProperty
         = DependencyProperty.Register(
             "SizeToDataArea",
             typeof(bool),
             typeof(Plot2DPanel),
             new PropertyMetadata(true, DisplayChanged));

    public bool SizeToDataArea
    {
        get { return (bool)GetValue(SizeToDataAreaProperty); }
        set { SetValue(SizeToDataAreaProperty, value); }
    }

    public bool ShowXAxisTitle
    {
        get { return (bool)GetValue(ShowXAxisTitleProperty); }
        set { SetValue(ShowXAxisTitleProperty, value); }
    }

    public bool ShowYAxisTitle
    {
        get { return (bool)GetValue(ShowYAxisTitleProperty); }
        set { SetValue(ShowYAxisTitleProperty, value); }
    }

    public string XAxisTitle
    {
        get { return (string)GetValue(XAxisTitleProperty); }
        set { SetValue(XAxisTitleProperty, value); }
    }

    public string YAxisTitle
    {
        get { return (string)GetValue(YAxisTitleProperty); }
        set { SetValue(YAxisTitleProperty, value); }
    }

    public bool ShowMarks
    {
        get => (bool)GetValue(ShowMarksProperty);
        set => SetValue(ShowMarksProperty, value);
    }

    public Color XAxisLineStroke
    {
        get { return (Color)GetValue(XAxisLineStrokeProperty); }
        set { SetValue(XAxisLineStrokeProperty, value); }
    }

    public Color YAxisLineStroke
    {
        get { return (Color)GetValue(YAxisLineStrokeProperty); }
        set { SetValue(YAxisLineStrokeProperty, value); }
    }

    public Color XAxisScaleForeground
    {
        get { return (Color)GetValue(XAxisScaleForegroundProperty); }
        set { SetValue(XAxisScaleForegroundProperty, value); }
    }

    public Color YAxisScaleForeground
    {
        get { return (Color)GetValue(YAxisScaleForegroundProperty); }
        set { SetValue(YAxisScaleForegroundProperty, value); }
    }

    public double XAxisStop
    {
        get { return (double)GetValue(XAxisStopProperty); }
        set { SetValue(XAxisStopProperty, value); }
    }

    public double XAxisStart
    {
        get { return (double)GetValue(XAxisStartProperty); }
        set { SetValue(XAxisStartProperty, value); }
    }

    public double XScaleLen
    {
        get { return (double)GetValue(XScaleLenProperty); }
        set { SetValue(XScaleLenProperty, value); }
    }

    public double YAxisStop
    {
        get { return (double)GetValue(YAxisStopProperty); }
        set { SetValue(YAxisStopProperty, value); }
    }

    public double YAxisStart
    {
        get { return (double)GetValue(YAxisStartProperty); }
        set { SetValue(YAxisStartProperty, value); }
    }

    public double YScaleLen
    {
        get { return (double)GetValue(YScaleLenProperty); }
        set { SetValue(YScaleLenProperty, value); }
    }

    public Color BackBoardColor
    {
        get => (Color)GetValue(BackBoardColorProperty);
        set => SetValue(BackBoardColorProperty, value);
    }

    public Plot2DPanel()
    {
        dataDict = new();
        colorDict = new();
        markTags = new();
        lockObj = new object();
        visibleLineIndexes = new();
        Start(new GLControlSettings
        {
            MajorVersion = 3,
            MinorVersion = 6,
            UseDeviceDpi = false,
        });
        Loaded += Plot2DPanelLoaded;
        Unloaded += Plot2DPanelUnloaded;
        scaleMat4 = Matrix4.Identity;
        eventSlim = new ManualResetEventSlim();
    }

    public void AddData(Point data, int lineIndex = 0)
    {
        if (!dataDict.ContainsKey(lineIndex))
        {
            dataDict.TryAdd(lineIndex, new ConcurrentQueue<Point>());
            colorDict.AddOrUpdate(lineIndex, GetRandColor(lineIndex), (_, originColor) => originColor);
            visibleLineIndexes.Add(lineIndex);
        }
        dataDict[lineIndex].Enqueue(data);
        DoRender();
    }

    public void AddData(IEnumerable<Point> datas, int lineIndex = 0)
    {
        if (!dataDict.ContainsKey(lineIndex))
        {
            dataDict.TryAdd(lineIndex, new ConcurrentQueue<Point>());
            colorDict.AddOrUpdate(lineIndex, GetRandColor(lineIndex), (_, originColor) => originColor);
            visibleLineIndexes.Add(lineIndex);
        }
        foreach (var item in datas)
        {
            dataDict[lineIndex].Enqueue(item);
        }
        DoRender();
    }

    public void SetLineColor(int lineIndex, Color color)
    {
        colorDict.AddOrUpdate(lineIndex, color, (_, _) => color);
    }

    public MarkTag MakeTag(string tagName, string tagContent, Point tagPosition, int bindLineIndex = -1)
    {
        var newTag = new MarkTag
        {
            TagID = (int)DateTime.Now.Ticks + markSgIndex++,
            TagContennt = tagContent,
            TagName = tagName,
            TagPositionX = tagPosition.X,
            TagPositionY = tagPosition.Y,
            MarkBindLineIndex = bindLineIndex
        };
        markTags.Add(newTag);
        DoRender();
        return newTag;
    }

    public void RemoveMark(MarkTag tag)
    {
        if (tag == null) return;
        var m = markTags.FirstOrDefault(e => e.TagID == tag.TagID);
        if (m != null)
        {
            markTags.Remove(m);
            DoRender();
        }
    }

    public void Clear()
    {
        dataDict.Clear();
        markTags.Clear();
        visibleLineIndexes = new();
        colorDict.Clear();
        markTags.Clear();
        DoRender();
    }

    public void RemoveLine(int lineIndex)
    {
        if (dataDict.ContainsKey(lineIndex))
        {
            dataDict.TryRemove(lineIndex, out _);
            visibleLineIndexes.Remove(lineIndex);
            var toRemoveMarks = new List<MarkTag>();
            markTags.ForEach(e =>
            {
                if (e.MarkBindLineIndex == lineIndex)
                {
                    toRemoveMarks.Add(e);
                }
            });
            toRemoveMarks.ForEach(e => markTags.Remove(e));
            DoRender();
        }
    }

    public void HideLine(int lineIndex)
    {
        if (visibleLineIndexes.Contains(lineIndex))
        {
            visibleLineIndexes.Remove(lineIndex);
            DoRender();
        }
    }

    public void ShowLine(int lineIndex)
    {
        if (dataDict.ContainsKey(lineIndex) && !visibleLineIndexes.Contains(lineIndex))
        {
            visibleLineIndexes.Add(lineIndex);
            DoRender();
        }
    }

    public void SetLineData(IEnumerable<Point> datas, int lineIndex)
    {
        if (!dataDict.ContainsKey(lineIndex))
        {
            dataDict.TryAdd(lineIndex, new ConcurrentQueue<Point>());
            colorDict.AddOrUpdate(lineIndex, GetRandColor(lineIndex), (_, originColor) => originColor);
            visibleLineIndexes.Add(lineIndex);
        }
        dataDict[lineIndex] = new ConcurrentQueue<Point>(datas);
        DoRender();
    }

    private Color GetRandColor(int index)
    {
        var rand = new Random(randSeed + index);
        for (int i = 0; i <= index; i++) rand.NextDouble();
        var randVal = rand.NextDouble();
        return Color.FromRgb((byte)(rand.NextDouble() * 255), 80, 80);
    }

    private static void DisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Plot2DPanel alp) alp.DoRender();
    }

    private void SetSizeToDataArea()
    {
        if (dataDict != null && dataDict.Count != 0 && visibleLineIndexes != null)
        {
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            foreach (var data in dataDict)
            {
                if (visibleLineIndexes.Contains(data.Key) || data.Key < 0)
                {
                    maxX = Math.Max(maxX, data.Value.Max(e => e.X));
                    maxY = Math.Max(maxY, data.Value.Max(e => e.Y));
                    minX = Math.Min(minX, data.Value.Min(e => e.X));
                    minY = Math.Min(minX, data.Value.Min(e => e.Y));
                }
            }
            XAxisStart = minX;
            YAxisStart = minY;
            XAxisStop = maxX;
            YAxisStop = maxY;
            DoRender();
        }
    }

    private void Plot2DPanelLoaded(object sender, RoutedEventArgs e)
    {
        isUnloaded = false;
        Render += Plot2DPanelRender;
        scaleTextRender = new TextRender { CharactorSpanPixel = 1 };
        markRender = new TextRender() { CharactorSpanPixel = 1 };
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        panelShader = new Shader();
        panelShader.SetShaderSource(
            // Vertex
            "#version 330 core\r\n" +
            "layout(location = 0) in vec3 aPosition;\r\n" +
            "layout(location = 1) in vec4 color;\r\n" +
            "uniform mat4 model;\r\n" +
            "uniform mat4 projection;\r\n" +
            "out vec4 vertexColor;\r\n" +
            "void main(void)\r\n" +
            "{\r\n" +
            "    gl_Position = vec4(aPosition, 1.0) * model * projection;\r\n" +
            "    vertexColor = color;\r\n" +
            "}\r\n"
            ,
            // Frag
            "#version 330\r\n" +
            "out vec4 outputColor;\r\n" +
            "in vec4 vertexColor;\r\n" +
            "void main()\r\n" +
            "{\r\n" +
            "    outputColor = vertexColor;\r\n" +
            "}\r\n"
            );

        frameworkGridVBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, frameworkGridVBO);

        frameworkGridVAO = GL.GenVertexArray();
        GL.BindVertexArray(frameworkGridVAO);

        var vertexLocation = panelShader.GetAttribLocation("aPosition");
        var colorLocation = panelShader.GetAttribLocation("color");

        GL.EnableVertexAttribArray(vertexLocation);
        GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<PanelDrawingElement>(), 0);

        GL.EnableVertexAttribArray(colorLocation);
        GL.VertexAttribPointer(colorLocation, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<PanelDrawingElement>(), 3 * sizeof(float));
        panelShader.Use();

        //GL.Enable(EnableCap.Blend);
        //GL.BlendFunc(0, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        //GL.Enable(EnableCap.DepthTest);
        //GL.DepthFunc(DepthFunction.Less);
        //GL.DepthMask(true);
        DoRender();
        mouseMonitorThread = new Thread(() =>
        {
            int clickCnt = 0;
            int timeCounter = 0;
            POINT startPos = new POINT();
            var oldHoriShift = 0.0f;
            var oldVertShift = 0.0f;
            var doubleClickProtect = false;
            var firstClickMade = false;
            var clickDown = false;
            var clickUp = false;
            var dropStartPos = new Point();
            while (!isUnloaded)
            {
                Thread.Sleep(5);
                // 若控件不可见，则不执行后续动作
                if (!IsVisible)
                {
                    continue;
                }
                var leftButtonState = GetAsyncKeyState(VirtualKeyStates.VK_LBUTTON);

                if (leftButtonState != 0 && IsMouseOver)
                {
                    if ((leftButtonState & 0x8000) != 0)
                    {
                        firstClickMade = true;
                        if (!clickDown)
                        {
                            oldHoriShift = horiShift;
                            oldVertShift = vertShift;
                            GetCursorPos(out startPos);
                        }
                        clickDown = true;
                    }
                    if (!doubleClickProtect && firstClickMade)
                    {
                        GetCursorPos(out var curPos);
                        if (curPos.X != startPos.X && curPos.Y != startPos.Y)
                        {
                            timeCounter = 0;
                            clickCnt = 0;
                            clickUp = false;
                            Dispatcher.Invoke(() =>
                            {
                                Cursor = Cursors.ScrollAll;
                            });
                        }
                        if (Keyboard.Modifiers == ModifierKeys.Shift)
                        {
                            vertShift = oldVertShift - (curPos.Y - startPos.Y);
                        }
                        else if (Keyboard.Modifiers == ModifierKeys.Control)
                        {
                            horiShift = oldHoriShift + (curPos.X - startPos.X);
                        }
                        else
                        {
                            vertShift = oldVertShift - (curPos.Y - startPos.Y);
                            horiShift = oldHoriShift + (curPos.X - startPos.X);
                        }
                        scaleMat4.Column0 = new Vector4(xScale, 0, 0, horiShift);
                        scaleMat4.Column1 = new Vector4(0, yScale, 0, vertShift);
                        scaleMat4.Column2 = new Vector4(0, 0, 1, 0);
                        scaleMat4.Column3 = new Vector4(0, 0, 0, 1);
                        DoRender();
                    }
                }
                else if (leftButtonState == 0)
                {
                    if (clickDown)
                    {
                        clickDown = false;
                        clickUp = true;
                    }
                    if (clickUp)
                    {
                        clickCnt++;
                        if (clickCnt == 2)
                        {
                            DoubleClick();
                            clickCnt = 0;
                        }
                        clickUp = false;
                    }
                    doubleClickProtect = false;
                    oldHoriShift = 0;
                    oldVertShift = 0;
                    Dispatcher.Invoke(() =>
                    {
                        if (Cursor != Cursors.Arrow && Cursor != Cursors.Hand) Cursor = Cursors.Arrow;
                    });
                    firstClickMade = false;
                }
                if (!clickDown && !clickUp && clickCnt != 0)
                {
                    timeCounter++;
                    if (timeCounter == 50)
                    {
                        clickCnt = 0;
                        timeCounter = 0;
                    }
                }

                GetCursorPos(out var immPos);
                var rmodel = model.Inverted();
                Point rPos = new Point();
                Dispatcher?.Invoke(() =>
                {
                    try
                    {
                        rPos = PointFromScreen(new(immPos.X, immPos.Y));
                        if (rPos.X > ActualWidth || rPos.Y > ActualHeight || rPos.X < 0 || rPos.Y < 0)
                        {
                            return;
                        }
                    }
                    catch
                    {
                    }
                    var immVec = new Vector4((float)rPos.X, (float)-rPos.Y, 0, 1) * rmodel;
                    var doRender = false;
                    if (leftButtonState != 0)
                    {
                        if (mouseDown)
                        {
                            // Drop
                            doRender = true;
                        }
                        else
                        {
                            dropStartPos = new Point(immVec.X, immVec.Y);
                            mouseDown = true;
                        }
                    }
                    if (leftButtonState == 0 && mouseDown)
                    {
                        mouseDown = false;
                        doRender = true;
                    }
                    if (doRender) DoRender();
                });
            }
        })
        { IsBackground = true };
        mouseMonitorThread.SetApartmentState(ApartmentState.STA);
        mouseMonitorThread.Start();
    }

    private void Plot2DPanelRender()
    {
        panelShader.Use();
        ChartRender();
    }

    private void ChartRender()
    {
    ForRedraw:
        if (panelShader == null) return;
        //GL.ClearColor(0,0,0,0);
        GL.ClearColor(BackBoardColor.R / 255.0f, BackBoardColor.G / 255.0f, BackBoardColor.B / 255.0f, BackBoardColor.A / 255.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.BindVertexArray(frameworkGridVAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, frameworkGridVBO);
        projection = Matrix4.CreateOrthographicOffCenter(0, (float)ActualWidth, 0, (float)ActualHeight, -(float)ActualWidth / 2, (float)ActualWidth / 2);
        panelShader.Use();
        model = Matrix4.Identity * scaleMat4;
        panelShader.SetMatrix4("model", Matrix4.Identity);
        panelShader.SetMatrix4("projection", projection);

        var xScaleWidth = (float)((ActualWidth - leftSide) / (XAxisStop - XAxisStart));
        var yScaleWidth = (float)((ActualHeight - bottomSide) / (YAxisStop - YAxisStart));

        while (xScaleWidth * xScale * XScaleLen > 50)
        {
            XScaleLen /= 10;
        }
        while (xScaleWidth * xScale * XScaleLen < 5)
        {
            XScaleLen *= 10;
        }
        while (yScaleWidth * yScale * YScaleLen > 20)
        {
            YScaleLen /= 10;
        }
        while (yScaleWidth * yScale * YScaleLen < 2)
        {
            YScaleLen *= 10;
        }
        var depth = 1f;
        #region Render Data Lines
        GL.LineWidth(1);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.LineSmooth);
        GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
        foreach (var line in visibleLineIndexes)
        {
            var oDataLst = new List<Point>(dataDict[line]);
            var renderDataLst = new List<PanelDrawingElement>();
            var lineColor = colorDict[line];
            foreach (var e in oDataLst)
            {
                var v = new Vector4((float)((e.X - XAxisStart) * xScaleWidth), (float)((e.Y - YAxisStart) * yScaleWidth), 0, 1) * model;
                renderDataLst.Add(new PanelDrawingElement
                {
                    X = v.X + leftSide,
                    Y = v.Y + bottomSide,
                    Z = depth,
                    ScA = lineColor.A / 255.0f,
                    ScB = (lineColor.B + BackBoardColor.B) % 255 / 255.0f,
                    ScG = (lineColor.G + BackBoardColor.G) % 255 / 255.0f,
                    ScR = (lineColor.R + BackBoardColor.R) % 255 / 255.0f,
                });
            }
            var renderDatas = renderDataLst.ToArray();
            if (renderDatas.Length > 0)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, renderDatas.Length * Marshal.SizeOf<PanelDrawingElement>(), renderDatas, BufferUsageHint.StaticDraw);
                GL.BindVertexArray(frameworkGridVAO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, frameworkGridVBO);
                GL.DrawArrays(PrimitiveType.LineStrip, 0, renderDatas.Length);
            }
        }
        GL.Disable(EnableCap.LineSmooth);
        GL.Disable(EnableCap.Blend);
        GL.LineWidth(1);
        #endregion

        #region Render Mark Tags
        markRender.ClearBuffer();
        var markTagsBindLines = from m
                                in markTags
                                where m.MarkBindLineIndex >= 0
                                select m;
        var markTagsWithoutBind = from m
                                  in markTags
                                  where m.MarkBindLineIndex == -1
                                  select m;
        var marks = new List<MarkTag>();
        if (markTagsBindLines != null)
        {
            foreach (var m in markTagsBindLines)
            {
                if (visibleLineIndexes.Contains(m.MarkBindLineIndex))
                {
                    marks.Add(m);
                    if (colorDict.TryGetValue(m.MarkBindLineIndex, out var color))
                    {
                        m.TagColor = color;
                    }
                    else
                    {
                        m.TagColor = XAxisScaleForeground;
                    }
                }
            }
        }
        if (markTagsWithoutBind != null)
        {
            foreach (var m in markTagsWithoutBind)
            {
                marks.Add(m);
                m.TagColor = XAxisScaleForeground;
            }
        }
        var marksElem = new PanelDrawingElement[marks.Count];
        for (int i = 0; i < marks.Count; i++)
        {
            var v = new Vector4((float)(((float)marks[i].TagPositionX - XAxisStart) * xScaleWidth), (float)(((float)marks[i].TagPositionY - YAxisStart) * yScaleWidth), 0, 1) * model;
            if (v.X + leftSide > ActualWidth || v.X < 0 || v.Y + bottomSide > ActualHeight || v.Y < 0) continue;
            marksElem[i] = new PanelDrawingElement
            {
                X = v.X + leftSide,
                Y = v.Y + bottomSide,
                Z = depth,
                ScA = marks[i].TagColor.A / 255.0f,
                ScR = marks[i].TagColor.R / 255.0f,
                ScG = marks[i].TagColor.G / 255.0f,
                ScB = marks[i].TagColor.B / 255.0f,
            };
            markRender.AddCharactor(
                marks[i].TagContennt,
                12,
                (int)(v.X + leftSide),
                (int)(-bottomSide - v.Y),
                marks[i].TagColor,
                AlignmentX.Left,
                AlignmentY.Top);
        }
        GL.BufferData(BufferTarget.ArrayBuffer, marksElem.Length * Marshal.SizeOf<PanelDrawingElement>(), marksElem, BufferUsageHint.StaticDraw);
        GL.BindVertexArray(frameworkGridVAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, frameworkGridVBO);
        GL.PointSize(5);
        GL.DrawArrays(PrimitiveType.Points, 0, marksElem.Length);
        #endregion

        #region Render Mask
        if (ShowMarks)
        {
            var maskEntities = new List<PanelDrawingElement>
            {
                // X Axis Mask.
                new PanelDrawingElement { X = 0,                    Y = bottomSide, Z = depth, ScR = BackBoardColor.R / 255.0f, ScG = BackBoardColor.G / 255.0f, ScB = BackBoardColor.B / 255.0f, ScA = BackBoardColor.A / 255.0f},
                new PanelDrawingElement { X = (float)ActualWidth, Y = bottomSide, Z = depth, ScR = BackBoardColor.R / 255.0f, ScG = BackBoardColor.G / 255.0f, ScB = BackBoardColor.B / 255.0f, ScA = BackBoardColor.A / 255.0f},
                new PanelDrawingElement { X = (float)ActualWidth, Y = 0,          Z = depth, ScR = BackBoardColor.R / 255.0f, ScG = BackBoardColor.G / 255.0f, ScB = BackBoardColor.B / 255.0f, ScA = BackBoardColor.A / 255.0f},
                new PanelDrawingElement { X = 0,                    Y = 0,          Z = depth, ScR = BackBoardColor.R / 255.0f, ScG = BackBoardColor.G / 255.0f, ScB = BackBoardColor.B / 255.0f, ScA = BackBoardColor.A / 255.0f},

                // Y Axis Mask.
                new PanelDrawingElement { X = 0,        Y = bottomSide,        Z = depth, ScR = BackBoardColor.R / 255.0f, ScG = BackBoardColor.G / 255.0f, ScB = BackBoardColor.B / 255.0f, ScA = BackBoardColor.A / 255.0f},
                new PanelDrawingElement { X = 0,        Y = (int)ActualHeight, Z = depth, ScR = BackBoardColor.R / 255.0f, ScG = BackBoardColor.G / 255.0f, ScB = BackBoardColor.B / 255.0f, ScA = BackBoardColor.A / 255.0f},
                new PanelDrawingElement { X = leftSide, Y = (int)ActualHeight, Z = depth, ScR = BackBoardColor.R / 255.0f, ScG = BackBoardColor.G / 255.0f, ScB = BackBoardColor.B / 255.0f, ScA = BackBoardColor.A / 255.0f},
                new PanelDrawingElement { X = leftSide, Y = bottomSide,        Z = depth, ScR = BackBoardColor.R / 255.0f, ScG = BackBoardColor.G / 255.0f, ScB = BackBoardColor.B / 255.0f, ScA = BackBoardColor.A / 255.0f},
            };

            panelShader.SetMatrix4("model", Matrix4.Identity);
            panelShader.SetMatrix4("projection", projection);
            GL.BufferData(BufferTarget.ArrayBuffer, maskEntities.Count * Marshal.SizeOf<PanelDrawingElement>(), maskEntities.ToArray(), BufferUsageHint.StaticDraw);
            GL.BindVertexArray(frameworkGridVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, frameworkGridVBO);
            GL.DrawArrays(PrimitiveType.Quads, 0, maskEntities.Count);
        }
        #endregion

        #region Render Axis Lines
        gridEntities = new List<PanelDrawingElement>
        {
            new PanelDrawingElement { X = leftSide - 1,         Y = bottomSide,        Z = depth, ScR = XAxisLineStroke.R / 255.0f, ScG = XAxisLineStroke.G / 255.0f, ScB = XAxisLineStroke.B / 255.0f, ScA = XAxisLineStroke.A / 255.0f},
            new PanelDrawingElement { X = (float)ActualWidth, Y = bottomSide,        Z = depth, ScR = XAxisLineStroke.R / 255.0f, ScG = XAxisLineStroke.G / 255.0f, ScB = XAxisLineStroke.B / 255.0f, ScA = XAxisLineStroke.A / 255.0f},
            new PanelDrawingElement { X = leftSide,             Y = bottomSide,        Z = depth, ScR = YAxisLineStroke.R / 255.0f, ScG = YAxisLineStroke.G / 255.0f, ScB = YAxisLineStroke.B / 255.0f, ScA = YAxisLineStroke.A / 255.0f},
            new PanelDrawingElement { X = leftSide,             Y = (int)ActualHeight, Z = depth, ScR = YAxisLineStroke.R / 255.0f, ScG = YAxisLineStroke.G / 255.0f, ScB = YAxisLineStroke.B / 255.0f, ScA = YAxisLineStroke.A / 255.0f},
        };

        panelShader.SetMatrix4("model", Matrix4.Identity);
        panelShader.SetMatrix4("projection", projection);
        GL.BufferData(BufferTarget.ArrayBuffer, gridEntities.Count * Marshal.SizeOf<PanelDrawingElement>(), gridEntities.ToArray(), BufferUsageHint.StaticDraw);
        GL.BindVertexArray(frameworkGridVAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, frameworkGridVBO);
        GL.DrawArrays(PrimitiveType.Lines, 0, gridEntities.Count);
        #endregion

        #region Render Scale
        var hfac = 0.0;
        var vfac = 0.0;

        var realWidth = ActualWidth / (100 * xScaleWidth * xScale);
        var realHeight = ActualHeight / (100 * yScaleWidth * yScale);

        if (xScale > 1) hfac = Math.Pow(10, GetDecimalExponent(realWidth / 20));
        else hfac = Math.Pow(10, GetDecimalExponent(realWidth / 5));
        if (yScale > 1) vfac = Math.Pow(10, GetDecimalExponent(realHeight / 20));
        else vfac = Math.Pow(10, GetDecimalExponent(realHeight / 5));

        var rmodel = model.Inverted();
        scaleTextRender.ClearBuffer();
        var origin = new Vector4(0, 0, 0, 1) * rmodel;
        var end = new Vector4((float)ActualWidth, 0, 0, 1) * rmodel;
        scaleFontSize = 8;
        var xStart = origin.X / xScaleWidth / XScaleLen;
        var xScaleCnt = ActualWidth / xScale / xScaleWidth / XScaleLen;

        var start = origin.X / xScaleWidth;
        var stop = end.X / xScaleWidth;

        var sp = ActualWidth / (stop - start);
        var n = Math.Ceiling(Math.Log10(sp / 2) - 2);
        var count = (stop - start) / Math.Pow(10, -n);

        scaleEntities = new List<PanelDrawingElement>();
#if true
        for (int i = 0; i < xScaleCnt; i++)
        {
            var xVec = new Vector4((float)((i + (int)xStart - XAxisStart % XScaleLen) * XScaleLen * xScaleWidth), 0, 0, 1) * model;
            var xsc = i + (int)xStart + (int)(XAxisStart / XScaleLen);
            if (xVec.X < 0) continue;
            scaleEntities.Add(new PanelDrawingElement
            {
                X = leftSide + xVec.X,
                Y = bottomSide,
                Z = depth,
                ScR = XAxisLineStroke.R / 255.0f,
                ScG = XAxisLineStroke.G / 255.0f,
                ScB = XAxisLineStroke.B / 255.0f,
                ScA = XAxisLineStroke.A / 255.0f
            });
            scaleEntities.Add(new PanelDrawingElement
            {
                X = leftSide + xVec.X,
                Y = bottomSide + (xsc % 5 == 0 ? 6 : 3),
                Z = depth,
                ScR = XAxisLineStroke.R / 255.0f,
                ScG = XAxisLineStroke.G / 255.0f,
                ScB = XAxisLineStroke.B / 255.0f,
                ScA = XAxisLineStroke.A / 255.0f
            });
            if (xsc % 10 == 0)
            {
                var xscTxt = (xsc * XScaleLen).ToString();
                var xscTxtPixelWidth = scaleTextRender.MeasureTextWidth(xscTxt, scaleFontSize);
                scaleTextRender.AddCharactor(
                    xscTxt,
                    scaleFontSize,
                    (float)(leftSide + xVec.X - xscTxtPixelWidth / 2),
                    5 - bottomSide,
                    XAxisScaleForeground, AlignmentX.Left,
                    AlignmentY.Bottom);
            }
        }
#else
        var firstScaleShift = start % Math.Pow(10, -n);
        for (int i = 0; i < count; i++)
        {
            var xVec = new Vector4((float)((i) * Math.Pow(10, -n) * sp + firstScaleShift), 0, 0, 1) * model;
            var xsc = i + (int)xStart + XAxisStart / XScaleLen;
            if (xVec.X < 0) continue;
            scaleEntities.Add(new PanelDrawingElement
            {
                X = leftSide + xVec.X,
                Y = bottomSide,
                Z = depth,
                ScR = XAxisLineStroke.R / 255.0f,
                ScG = XAxisLineStroke.G / 255.0f,
                ScB = XAxisLineStroke.B / 255.0f,
                ScA = XAxisLineStroke.A / 255.0f
            });
            scaleEntities.Add(new PanelDrawingElement
            {
                X = leftSide + xVec.X,
                Y = bottomSide + (xsc % 5 == 0 ? 6 : 3),
                Z = depth,
                ScR = XAxisLineStroke.R / 255.0f,
                ScG = XAxisLineStroke.G / 255.0f,
                ScB = XAxisLineStroke.B / 255.0f,
                ScA = XAxisLineStroke.A / 255.0f
            });
            if (xsc % 10 == 0)
            {
                var xscTxt = (xsc * Math.Pow(10, -n)).ToString();
                var xscTxtPixelWidth = scaleTextRender.MeasureTextWidth(xscTxt, scaleFontSize);
                scaleTextRender.AddCharactor(
                    xscTxt,
                    scaleFontSize,
                    (float)(leftSide + xVec.X - xscTxtPixelWidth / 2),
                    5 - bottomSide,
                    XAxisScaleForeground, AlignmentX.Left,
                    AlignmentY.Bottom);
            }
        }
#endif

        var yScaleCnt = ActualHeight / yScale / yScaleWidth / YScaleLen;
        var yStart = origin.Y / yScaleWidth / YScaleLen;
        var oldMaxYScaleLen = maxYScaleTextLength;
        for (int i = 0; i < yScaleCnt; i++)
        {
            var yVec = new Vector4(0, (float)((i + (int)yStart - YAxisStart % YScaleLen) * YScaleLen * yScaleWidth), 0, 1) * model;
            var ysc = i + (int)yStart + Math.Floor(YAxisStart / YScaleLen);
            if (yVec.Y < 0) continue;
            scaleEntities.Add(new PanelDrawingElement
            {
                X = leftSide,
                Y = bottomSide + yVec.Y,
                Z = depth,
                ScR = YAxisLineStroke.R / 255.0f,
                ScG = YAxisLineStroke.G / 255.0f,
                ScB = YAxisLineStroke.B / 255.0f,
                ScA = YAxisLineStroke.A / 255.0f
            });
            scaleEntities.Add(new PanelDrawingElement
            {
                X = leftSide + (ysc % 5 == 0 ? 6 : 3),
                Y = bottomSide + yVec.Y,
                Z = depth,
                ScR = YAxisLineStroke.R / 255.0f,
                ScG = YAxisLineStroke.G / 255.0f,
                ScB = YAxisLineStroke.B / 255.0f,
                ScA = YAxisLineStroke.A / 255.0f
            });
            if (ysc % 10 == 0)
            {
                var yscTxt = (ysc * YScaleLen).ToString();
                var yscTxtPixelWidth = scaleTextRender.MeasureTextWidth(yscTxt, scaleFontSize);
                maxYScaleTextLength = (int)Math.Max(scaleFontSize, yscTxtPixelWidth);
                scaleTextRender.AddCharactor(yscTxt, scaleFontSize, (float)(leftSide - yscTxtPixelWidth - 5), -bottomSide - yVec.Y, YAxisScaleForeground, AlignmentX.Left, AlignmentY.Center);
            }
        }
        if (maxYScaleTextLength != oldMaxYScaleLen)
        {
            leftSide = 25 + maxYScaleTextLength;
            goto ForRedraw;
        }
        panelShader.SetMatrix4("model", Matrix4.Identity);
        panelShader.SetMatrix4("projection", projection);
        GL.BufferData(BufferTarget.ArrayBuffer, scaleEntities.Count * Marshal.SizeOf<PanelDrawingElement>(), scaleEntities.ToArray(), BufferUsageHint.StaticDraw);
        GL.BindVertexArray(frameworkGridVAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, frameworkGridVBO);
        GL.DrawArrays(PrimitiveType.Lines, 0, scaleEntities.Count);
        #endregion

        scaleTextRender.DoDisplayRender((float)ActualWidth, (float)ActualHeight);
        markRender.DoDisplayRender((float)ActualWidth, (float)ActualHeight);
        GL.BindVertexArray(0);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        var oldXScale = xScale;
        var oldYScale = yScale;
        var pos = Mouse.GetPosition(this);
        if (Keyboard.Modifiers == ModifierKeys.None)
        {
            xScale *= (float)Math.Pow(2, 0.1 * e.Delta / Mouse.MouseWheelDeltaForOneLine);
            xScale = (float)Math.Max(0.1, Math.Min(xScale, 6000));
            horiShift = (float)(pos.X - leftSide - (pos.X - leftSide - horiShift) * (xScale / oldXScale));
        }
        else if (Keyboard.Modifiers == ModifierKeys.Shift)
        {
            yScale *= (float)Math.Pow(2, 0.1 * e.Delta / Mouse.MouseWheelDeltaForOneLine);
            yScale = (float)Math.Max(0.1, Math.Min(yScale, 6000));
            vertShift = (float)(ActualHeight - pos.Y - bottomSide - (ActualHeight - pos.Y - vertShift - bottomSide) * (yScale / oldYScale));
        }
        scaleMat4 = new Matrix4();

        scaleMat4.Column0 = new Vector4(xScale, 0, 0, horiShift);
        scaleMat4.Column1 = new Vector4(0, yScale, 0, vertShift);
        scaleMat4.Column2 = new Vector4(0, 0, 1, 0);
        scaleMat4.Column3 = new Vector4(0, 0, 0, 1);

        base.OnMouseWheel(e);
        DoRender();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        drawingContext.DrawRectangle(Brushes.Black, new Pen(Brushes.Black, 1), new Rect(new Size(ActualWidth, ActualHeight)));
        base.OnRender(drawingContext);
    }

    private void Plot2DPanelUnloaded(object sender, RoutedEventArgs e)
    {
        isUnloaded = true;
        Render -= Plot2DPanelRender;
    }

    private void DoubleClick()
    {
        Dispatcher?.Invoke(() =>
        {
            if (SizeToDataArea)
            {
                xScale = 1;
                yScale = 1;
                horiShift = 0;
                vertShift = 0;
                scaleMat4 = Matrix4.Identity;
                SetSizeToDataArea();
            }
            else
            {
                xScale = 1;
                yScale = 1;
                horiShift = 0;
                vertShift = 0;
                scaleMat4 = Matrix4.Identity;
                DoRender();
            }
        });
    }

    private int GetDecimalExponent(double val)
    {
        var logv = Math.Log10(Math.Abs(val));
        if (logv - (int)logv == 0) return (int)logv;
        return (int)logv;
    }
}
