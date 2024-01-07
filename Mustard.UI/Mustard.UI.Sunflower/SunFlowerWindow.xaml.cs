using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using static Mustard.Base.Toolset.WinAPI;
using static Mustard.UI.Sunflower.SunFlowerCore;

namespace Mustard.UI.Sunflower;

public class SunFlowerWindow : Window
{
    #region 依赖属性
    //public static DependencyProperty ColorThemeProperty;
    public static DependencyProperty MenuBackgroundProperty;
    public static DependencyProperty MenuForegroundProperty;
    public static DependencyProperty TitleForegroundProperty;
    public static DependencyProperty TitleMenuProperty;
    public static DependencyProperty WindowMaxButtonVisableProperty;
    public static DependencyProperty WindowMinButtonVisableProperty;
    public static DependencyProperty WindowClsButtonVisableProperty;
    public static DependencyProperty TitleExtensionBarContentProperty;
    public static DependencyProperty ShowStatusBarProperty;
    #endregion

    #region 属性
    //public WindowColorTheme ColorTheme { get => (WindowColorTheme)GetValue(ColorThemeProperty); set => SetValue(ColorThemeProperty, value); }
    public Brush MenuBackground { get => (Brush)GetValue(MenuBackgroundProperty); set => SetValue(MenuBackgroundProperty, value); }
    public Brush MenuForeground { get => (Brush)GetValue(MenuForegroundProperty); set => SetValue(MenuForegroundProperty, value); }
    public Brush TitleForeground { get => (Brush)GetValue(TitleForegroundProperty); set => SetValue(TitleForegroundProperty, value); }
    public Menu TitleMenu { get => (Menu)GetValue(TitleMenuProperty); set => SetValue(TitleMenuProperty, value); }
    public bool WindowMaxButtonVisable { get => (bool)GetValue(WindowMaxButtonVisableProperty); set => SetValue(WindowMaxButtonVisableProperty, value); }
    public bool WindowMinButtonVisable { get => (bool)GetValue(WindowMinButtonVisableProperty); set => SetValue(WindowMinButtonVisableProperty, value); }
    public bool WindowClsButtonVisable { get => (bool)GetValue(WindowClsButtonVisableProperty); set => SetValue(WindowClsButtonVisableProperty, value); }
    public object TitleExtensionBarContent { get => GetValue(TitleExtensionBarContentProperty); set => SetValue(TitleExtensionBarContentProperty, value); }
    public bool ShowStatusBar { get => (bool)GetValue(ShowStatusBarProperty); set => SetValue(ShowStatusBarProperty, value); }
    #endregion

    #region 依赖属性更改事件
    internal static void WindowMaxButtonVisableChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is bool visable && o is SunFlowerWindow window)
        {
            if (window.btnMaximize != null)
            {
                window.btnMaximize.Visibility = visable ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    internal static void WindowMinButtonVisableChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is bool visable && o is SunFlowerWindow window)
        {
            if (window.btnMinimize != null)
            {
                window.btnMinimize.Visibility = visable ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    internal static void WindowClsButtonVisableChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is bool visable && o is SunFlowerWindow window)
        {
            if (window.btnClose != null)
            {
                window.btnClose.Visibility = visable ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
    #endregion

    #region 控件
    private Button btnMinimize;
    private Button btnMaximize;
    private Button btnClose;
    private Button btnHelp;
    private Grid grdBtnGroup;
    private Image imgIcon;
    #endregion

    #region 构造
    /// <summary>
    /// 静态构造，初始化环境
    /// </summary>
    static SunFlowerWindow()
    {
        LoadConverters();

        // 添加Xaml到程序资源中
        FindAndAdd("pack://application:,,,/Mustard.UI.Sunflower;component/SunflowerWindow.xaml");
        InitTheme();
        // 添加依赖属性
        var type = typeof(SunFlowerWindow);
        var staticFields = type.GetFields();
        var properties = type.GetProperties();
        var methods = type.GetMethods();
        var methodsw = type.GetRuntimeMethods();
        foreach (var property in properties)
        {
            var met = methodsw.FirstOrDefault(e => e.IsStatic && e.Name == $"{property.Name}Changed");
            PropertyChangedCallback call = (met == null ? null : (PropertyChangedCallback)met.CreateDelegate(typeof(PropertyChangedCallback)));
            RegistProperty(property, null, call);
        }

        // 注册资源属性
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
                typeof(SunFlowerWindow), frameworkProMeta
                );
            depField.SetValue(null, dep);
        }
    }

    public SunFlowerWindow()
    {
        WindowClsButtonVisable = true;
        WindowMaxButtonVisable = true;
        WindowMinButtonVisable = true;
        SetResourceReference(StyleProperty, "SunFlowerWindowStyle");
        Loaded += SunFlowerWindow_Loaded;
        PreviewMouseDown += (_, _) =>
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, () =>
            {
                if (Keyboard.FocusedElement is TextBoxBase tbb && tbb.TemplatedParent is not ComboBox && Keyboard.FocusedElement != Mouse.Captured)
                {
                    Keyboard.ClearFocus();
                    FocusManager.SetFocusedElement(this, this);
                }
                else if (Keyboard.FocusedElement is TextBoxBase cmTbb && cmTbb.TemplatedParent is ComboBox && Mouse.Captured == cmTbb)
                {
                    Keyboard.ClearFocus();
                    FocusManager.SetFocusedElement(this, this);
                }
            });
        };
    }

    private void SunFlowerWindow_Loaded(object sender, RoutedEventArgs e)
    {
        btnMinimize = (Button)Template.FindName("btnMinimize", this);
        btnMaximize = (Button)Template.FindName("btnMaximize", this);
        btnClose = (Button)Template.FindName("btnClose", this);
        btnHelp = (Button)Template.FindName("btnHelp", this);
        grdBtnGroup = (Grid)Template.FindName("captionButtonsGroup", this);
        imgIcon = (Image)Template.FindName("imgIcon", this);

        if (btnClose != null)
        {
            btnClose.Click += (_, _) =>
            {
                Close();
            };
            btnClose.Visibility = WindowClsButtonVisable ? Visibility.Visible : Visibility.Collapsed;
        }
        if (btnMaximize != null)
        {
            btnMaximize.Click += (_, _) =>
            {
                if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
                else WindowState = WindowState.Maximized;
            };
            btnMaximize.Visibility = WindowMaxButtonVisable ? Visibility.Visible : Visibility.Collapsed;
        }
        if (btnMinimize != null)
        {
            btnMinimize.Click += (_, _) =>
            {
                WindowState = WindowState.Minimized;
            };
            btnMinimize.Visibility = WindowMinButtonVisable ? Visibility.Visible : Visibility.Collapsed;
        }
        if (grdBtnGroup != null)
        {
            var dt0 = DateTime.Now;
            var mousePos = new Point(double.NaN, double.NaN);
            grdBtnGroup.MouseDown += (_, _) =>
            {
                if ((DateTime.Now - dt0).TotalMilliseconds < 200 && (Mouse.GetPosition(grdBtnGroup) - mousePos).Length < 30)
                {
                    if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
                    else WindowState = WindowState.Maximized;
                    dt0 = DateTime.Now;
                    mousePos = Mouse.GetPosition(grdBtnGroup);
                    ReleaseCapture();
                }
                else
                {
                    dt0 = DateTime.Now;
                    mousePos = Mouse.GetPosition(grdBtnGroup);
                    IntPtr hwnd = new WindowInteropHelper(this).Handle;
                    ReleaseCapture();
                    SendMessage(hwnd, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
                }
            };
        }
        if (imgIcon != null)
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var iconBitmap = System.Drawing.Icon.ExtractAssociatedIcon(process.MainModule.FileName).ToBitmap();
                using var bitmapStream = new MemoryStream();
                iconBitmap.Save(bitmapStream, System.Drawing.Imaging.ImageFormat.Png);
                bitmapStream.Seek(0, SeekOrigin.Begin);
                var decoder = BitmapDecoder.Create(bitmapStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                if (decoder.Frames != null || decoder.Frames.Count != 0)
                {
                    imgIcon.Source = decoder.Frames.OrderByDescending(e => e.PixelWidth).ElementAt(0);
                }
            }
            catch { }
        }
    }
    #endregion

    #region 窗口事件


    #endregion

    #region Protected Functions & Methods
    protected static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
    {
        if (parent == null) return null;
        T foundChild = null;
        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            var childType = child as T;
            if (childType == null)
            {
                foundChild = FindChild<T>(parent, childName);
                if (foundChild != null) break;
            }
            else if (!string.IsNullOrEmpty(childName))
            {
                var frameworkElement = child as FrameworkElement;
                if (frameworkElement != null && frameworkElement.Name == childName)
                {
                    foundChild = (T)child;
                    break;
                }
                foundChild = FindChild<T>(child, childName);
            }
            else
            {
                foundChild = FindChild<T>(child, childName);
                break;
            }
        }
        return foundChild;
    }
    #endregion
}
