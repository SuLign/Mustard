using System;
using System.CodeDom;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

using static Mustard.Base.Toolset.WinAPI;

using Path = System.Windows.Shapes.Path;

namespace Mustard.UI.MVVM;

public class MustardOpenFileDialog
{
    private Popup displayPopup;
    private Point cPos;
    private MessageBoxResult _dialogResult;

    private IntPtr _dialogOwnerHandle;
    private ArrayList _threadWindowHandles;
    private IntPtr popHandle;
    private DispatcherFrame _dispatcherFrame;
    private IntPtr dialogActiveHandle;
    private HwndSource sourceHandle;

    private void InitElementComponents()
    {
        displayPopup = new Popup();
        displayPopup.Width = 1200;
        displayPopup.Height = 800;
        displayPopup.AllowsTransparency = true;
        displayPopup.PlacementRectangle = new Rect(
            new Point((SystemParameters.MaximizedPrimaryScreenWidth - displayPopup.Width) / 2,
                (SystemParameters.MaximizedPrimaryScreenHeight - displayPopup.Height * 3) / 2),
            new Size(displayPopup.Width,
                displayPopup.Height));
        displayPopup.Effect = new DropShadowEffect
        {
            BlurRadius = 10,
            ShadowDepth = 10,
            Direction = 100,
            Opacity = 1,
        };
        var border = new Border
        {
            BorderThickness = new Thickness(1, 0, 1, 1),
        };
        border.SetResourceReference(Border.BackgroundProperty, "Major_Background");
        border.SetResourceReference(Border.BorderBrushProperty, "Border_BorderBrush");
        displayPopup.Child = border;


        var hostGrid = new Grid();
        hostGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40, GridUnitType.Pixel) });
        hostGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        hostGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80, GridUnitType.Pixel) });
        border.Child = hostGrid;

        //ResizeGrip resizeGrip = new ResizeGrip();
        //Grid.SetRowSpan(resizeGrip, 10);
        //hostGrid.Children.Add(resizeGrip);

        //var msgBoxImgPathData = "M19,20H4C2.89,20 2,19.1 2,18V6C2,4.89 2.89,4 4,4H10L12,6H19A2,2 0 0,1 21,8H21L4,8V18L6.14,10H23.21L20.93,18.5C20.7,19.37 19.92,20 19,20Z";

        var captionBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0))
        };
        captionBorder.MouseMove += MustardMessageBoxDropMove;

        var captionGrid = new Grid
        {
            Margin = new Thickness(0, 0, 0, 5)
        };

        captionGrid.SetResourceReference(Grid.BackgroundProperty, "Border_BorderBrush");
        Grid.SetRow(captionGrid, 0);
        captionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        captionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        captionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        captionBorder.Child = captionGrid;
        hostGrid.Children.Add(captionBorder);

        // Close Button.
        var clsBtn = new Button
        {
            Width = 45,
            Margin = new Thickness(0, 0, 1, 0),
        };
        var clsBtnImg = new Path
        {
            Stretch = Stretch.Uniform,
            Width = 12
        };
        clsBtnImg.Data = Geometry.Parse("M13.46,12L19,17.54V19H17.54L12,13.46L6.46,19H5V17.54L10.54,12L5,6.46V5H6.46L12,10.54L17.54,5H19V6.46L13.46,12Z");
        clsBtnImg.Fill = Brushes.LightGray;
        clsBtn.Content = clsBtnImg;
        Grid.SetColumn(clsBtn, 2);
        clsBtn.Click += (_, _) =>
        {
            _dispatcherFrame.Continue = false;
            displayPopup.IsOpen = false;
            if (_threadWindowHandles != null)
            {
                EnableThreadWindows(state: true);
            }
            if (dialogActiveHandle != IntPtr.Zero && IsWindow(new HandleRef(null, dialogActiveHandle)))
            {
                TrySetFocus(new HandleRef(null, dialogActiveHandle), ref dialogActiveHandle);
            }
        };
        captionGrid.Children.Add(clsBtn);

        // Caption.
        var captionTextBlock = new TextBlock
        {
            Margin = new Thickness(15, 0, 0, 0),
            FontSize = 15,
            Text = "打开文件",
            FontFamily = new FontFamily("Microsoft Yahei"),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Opacity = 1
        };
        captionTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "Sub1_Foreground");
        Grid.SetColumn(captionTextBlock, 1);
        captionGrid.Children.Add(captionTextBlock);

        // Caption Icon.
        var imgIcon = new Image
        {
            Height = 20,
            Width = 20,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(15, 0, 0, 0),
        };
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
        //var iconPath = new Path
        //{
        //    Data = Geometry.Parse(msgBoxImgPathData),
        //    Margin = new Thickness(15, 0, 0, 0),
        //    VerticalAlignment = VerticalAlignment.Center,
        //    Stretch = Stretch.Uniform,
        //    Opacity = 0.7,
        //    Height = 13
        //};
        //iconPath.SetResourceReference(Path.FillProperty, "Sub1_Foreground");
        //Grid.SetColumn(iconPath, 0);
        //captionGrid.Children.Add(iconPath);
        Grid.SetColumn(imgIcon, 0);
        captionGrid.Children.Add(imgIcon);

        // Buttons.
        var buttonStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 5, 0),
        };
        Grid.SetRow(buttonStackPanel, 2);
        hostGrid.Children.Add(buttonStackPanel);

        // Content.
        var contentGrid = new Grid();
        Grid.SetRow(contentGrid, 1);
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35, GridUnitType.Pixel) });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        hostGrid.Children.Add(contentGrid);

        // Navigate.
        var navigateGrid = new Grid();
        navigateGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        navigateGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        navigateGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        contentGrid.Children.Add(navigateGrid);

        // Navigate Buttons.
        var navButtonsStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
        };
        navigateGrid.Children.Add(navButtonsStackPanel);

        // Left Nav Button.
        var leftNavBtn = new Button
        {
            Width = 40
        };
        var leftNavBtnImg = new Path
        {
            Data = Geometry.Parse("M20,11V13H8L13.5,18.5L12.08,19.92L4.16,12L12.08,4.08L13.5,5.5L8,11H20Z"),
            Stretch = Stretch.Uniform,
            Height = 12,
            Opacity = 0.5
        };
        leftNavBtnImg.SetResourceReference(Path.FillProperty, "Sub1_Foreground");
        leftNavBtn.Content = leftNavBtnImg;
        navButtonsStackPanel.Children.Add(leftNavBtn);

        // Right Nav Button.
        var rightNavBtn = new Button
        {
            Width = 40
        };
        var rightNavBtnImg = new Path
        {
            Data = Geometry.Parse("M4,11V13H16L10.5,18.5L11.92,19.92L19.84,12L11.92,4.08L10.5,5.5L16,11H4Z"),
            Stretch = Stretch.Uniform,
            Height = 12,
            Opacity = 0.5
        };
        rightNavBtnImg.SetResourceReference(Path.FillProperty, "Sub1_Foreground");
        rightNavBtn.Content = rightNavBtnImg;
        navButtonsStackPanel.Children.Add(rightNavBtn);

        // Down Nav Button.
        var downNavBtn = new Button
        {
            Width = 40
        };
        var downNavBtnImg = new Path
        {
            Data = Geometry.Parse("M7.41,8.58L12,13.17L16.59,8.58L18,10L12,16L6,10L7.41,8.58Z"),
            Stretch = Stretch.Uniform,
            Height = 8,
            Opacity = 0.5
        };
        downNavBtnImg.SetResourceReference(Path.FillProperty, "Sub1_Foreground");
        downNavBtn.Content = downNavBtnImg;
        navButtonsStackPanel.Children.Add(downNavBtn);

        // Find Presious Nav Button.
        var preNavBtn = new Button
        {
            Width = 40
        };
        var preNavBtnImg = new Path
        {
            Data = Geometry.Parse("M13,20H11V8L5.5,13.5L4.08,12.08L12,4.16L19.92,12.08L18.5,13.5L13,8V20Z"),
            Stretch = Stretch.Uniform,
            Height = 12,
            Opacity = 0.5
        };
        preNavBtnImg.SetResourceReference(Path.FillProperty, "Sub1_Foreground");
        preNavBtn.Content = preNavBtnImg;
        navButtonsStackPanel.Children.Add(preNavBtn);

        // Address Fill.
        TextBox addressFillTextBox = new TextBox
        {
            FontSize = 16,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(addressFillTextBox, 1);
        navigateGrid.Children.Add(addressFillTextBox);

        // Search Fill.
        TextBox searchFillTextBox = new TextBox
        {
            Width = 240,
            Margin = new Thickness(5, 0, 5, 0),
            FontSize = 16,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(searchFillTextBox, 2);
        navigateGrid.Children.Add(searchFillTextBox);

        // Browser Grid.
        var browserGrid = new Grid
        {
            Margin = new Thickness(0, 5, 0, 0),
        };
        browserGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35, GridUnitType.Pixel) });
        browserGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        Grid.SetRow(browserGrid, 1);
        contentGrid.Children.Add(browserGrid);

        var browserEditorNavGrid = new Grid
        {
            Opacity = 0.6
        };
        browserEditorNavGrid.SetResourceReference(Grid.BackgroundProperty, "Sub1_Background");
        browserGrid.Children.Add(browserEditorNavGrid);

        var browserFileContent = new Grid
        {

        };
        Grid.SetRow(browserFileContent, 1);
        browserFileContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(350, GridUnitType.Pixel) });
        browserFileContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        browserGrid.Children.Add(browserFileContent);

        var folderTreeViewer = new TreeView
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0, 0, 1, 0),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x1F, 0x85, 0x85, 0x85))
        };
        browserFileContent.Children.Add(folderTreeViewer);

        // Bottom Grid.
        var bottomGrid = new Grid
        {
        };
        bottomGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        bottomGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        bottomGrid.SetResourceReference(Grid.BackgroundProperty, "Sub4_Background");
        Grid.SetRow(bottomGrid, 3);
        hostGrid.Children.Add(bottomGrid);

        var bottomFillGird = new Grid
        {
            Margin = new Thickness(150, 7, 10, 0),
        };
        bottomFillGird.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        bottomFillGird.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        bottomFillGird.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        bottomGrid.Children.Add(bottomFillGird);

        bottomFillGird.Children.Add(new TextBlock
        {
            Text = "文件名:",
            Margin = new Thickness(5, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 14,
            Foreground = Brushes.White,
            FontFamily = new FontFamily("Microsoft Yahei"),
        });

        var fileNameInput = new ComboBox
        {
            FontSize = 16,
            Margin = new Thickness(0, 0, 5, 0),
            VerticalContentAlignment = VerticalAlignment.Center,
            IsEditable = true,
        };
        Grid.SetColumn(fileNameInput, 1);
        bottomFillGird.Children.Add(fileNameInput);

        var fileFilterInput = new ComboBox
        {
            FontSize = 16,
            Width = 170,
            VerticalContentAlignment = VerticalAlignment.Center,
            IsEditable = true,
        };
        Grid.SetColumn(fileFilterInput, 2);
        bottomFillGird.Children.Add(fileFilterInput);

        var bottomBtnStackPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 10, 0),
        };
        Grid.SetRow(bottomBtnStackPanel, 1);
        bottomGrid.Children.Add(bottomBtnStackPanel);

        var openBtn = new Button
        {
            Width = 80,
            Content = "打开",
            FontSize = 14,
            Margin = new Thickness(0, 5, 0, 5),
            FontFamily = new FontFamily("Microsoft Yahei"),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75)),
            BorderThickness = new Thickness(1),
        };
        bottomBtnStackPanel.Children.Add(openBtn);

        var cancelBtn = new Button
        {
            Width = 80,
            Content = "取消",
            FontSize = 14,
            Margin = new Thickness(3, 5, 0, 5),
            FontFamily = new FontFamily("Microsoft Yahei"),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75)),
            BorderThickness = new Thickness(1),
        };
        bottomBtnStackPanel.Children.Add(cancelBtn);

        displayPopup.MouseDown += GotFocus;
        displayPopup.PreviewMouseDown += GotFocus;
        addressFillTextBox.GotMouseCapture += GotFocus;
        searchFillTextBox.GotMouseCapture += GotFocus;
        fileFilterInput.GotMouseCapture += GotFocus;
        fileNameInput.GotMouseCapture += GotFocus;
        displayPopup.Focusable = true;
        displayPopup.IsOpen = true;
        sourceHandle = (HwndSource)PresentationSource.FromVisual(displayPopup.Child);
        SetFocus(sourceHandle.Handle);
        SetForegroundWindow(sourceHandle.Handle);
    }

    private void GotFocus(object sender, RoutedEventArgs e)
    {
        SetFocus(sourceHandle.Handle);
        SetForegroundWindow(sourceHandle.Handle);
    }

    private void MustardMessageBoxDropMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if ((GetAsyncKeyState(VirtualKeyStates.VK_LBUTTON) & 0x8000) != 0)
        {
            GetCursorPos(out var startPos);
            var oldCPos = cPos;
            while ((GetAsyncKeyState(VirtualKeyStates.VK_LBUTTON) & 0x8000) != 0)
            {
                GetCursorPos(out var mPos);
                cPos = new Point(mPos.X - startPos.X + oldCPos.X, mPos.Y - startPos.Y + oldCPos.Y);
                displayPopup.PlacementRectangle = new Rect(
                    new Point((SystemParameters.MaximizedPrimaryScreenWidth - displayPopup.Width) / 2 + cPos.X,
                        (SystemParameters.MaximizedPrimaryScreenHeight - displayPopup.Height * 3) / 2 + cPos.Y),
                    new Size(displayPopup.Width,
                        displayPopup.Height));
            }
        }
    }

    private void SetDialogHandle()
    {
        _dialogOwnerHandle = IntPtr.Zero;
        if (!IsWindow(new HandleRef(null, _dialogOwnerHandle)))
        {
            _dialogOwnerHandle = IntPtr.Zero;
        }
        var dialogActiveHandle = GetActiveWindow();
        if (_dialogOwnerHandle == IntPtr.Zero)
        {
            _dialogOwnerHandle = dialogActiveHandle;
        }
        if (dialogActiveHandle != IntPtr.Zero && _dialogOwnerHandle == GetDesktopWindow())
        {
            _dialogOwnerHandle = IntPtr.Zero;
        }
        if (_dialogOwnerHandle != IntPtr.Zero)
        {
            int num = 0;
            while (_dialogOwnerHandle != IntPtr.Zero)
            {
                num = WGetWindowLong(new HandleRef(null, _dialogOwnerHandle), -16);
                if ((num & 0x40000000) != 1073741824)
                {
                    break;
                }
                _dialogOwnerHandle = WGetParent(new HandleRef(null, _dialogOwnerHandle));
            }
        }
        _threadWindowHandles = new ArrayList();
        EnumThreadWindows(GetCurrentThreadId(), ThreadWindowsCallback, new HandleRef(null, IntPtr.Zero));
        EnableThreadWindows(false);
        IntPtr capture = GetCapture();
        Debug.WriteLine(_threadWindowHandles.Count);
        if (capture != IntPtr.Zero)
        {
            IntReleaseCapture();
        }
    }

    private bool ThreadWindowsCallback(IntPtr hWnd, IntPtr lparam)
    {
        if ((IsWindowVisible(new HandleRef(null, hWnd)) && IsWindowEnabled(new HandleRef(null, hWnd)))
            || popHandle == hWnd)
        {
            _threadWindowHandles.Add(hWnd);
        }

        return true;
    }

    private void EnableThreadWindows(bool state)
    {
        for (int i = 0; i < _threadWindowHandles.Count; i++)
        {
            IntPtr handle = (IntPtr)_threadWindowHandles[i];
            if (IsWindow(new HandleRef(null, handle)))
            {
                EnableWindow(new HandleRef(null, handle), state);
            }
        }
        if (state)
        {
            _threadWindowHandles = null;
        }
    }

    private static bool TrySetFocus(HandleRef hWnd, ref IntPtr result)
    {
        result = SetFocus(hWnd);
        int lastWin32Error = Marshal.GetLastWin32Error();
        if (result == IntPtr.Zero && lastWin32Error != 0)
        {
            return false;
        }
        return true;
    }

    private static int WGetWindowLong(HandleRef hWnd, int nIndex)
    {
        int num = 0;
        IntPtr zero = IntPtr.Zero;
        int num2 = 0;
        if (IntPtr.Size == 4)
        {
            num = GetWindowLong(hWnd, nIndex);
            num2 = Marshal.GetLastWin32Error();
            zero = new IntPtr(num);
        }
        else
        {
            zero = GetWindowLongPtr(hWnd, nIndex);
            num2 = Marshal.GetLastWin32Error();
            num = (int)zero;
        }

        if (zero == IntPtr.Zero)
        {
        }

        return num;
    }

    private static IntPtr WGetParent(HandleRef hWnd)
    {
        IntPtr parent = GetParent(hWnd);
        int lastWin32Error = Marshal.GetLastWin32Error();
        if (parent == IntPtr.Zero && lastWin32Error != 0)
        {
            throw new Win32Exception(lastWin32Error);
        }

        return parent;
    }

    public bool? ShowDialog()
    {
        _dispatcherFrame = new DispatcherFrame
        {
            Continue = true
        };
        var thread = new Thread(() =>
        {
            InitElementComponents();
            Dispatcher.Run();
        })
        { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        SetDialogHandle();
        // 阻塞
        ComponentDispatcher.PushModal();
        Dispatcher.PushFrame(_dispatcherFrame);
        ComponentDispatcher.PopModal();
        return null;
    }
}
