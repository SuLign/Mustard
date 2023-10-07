using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

using static Mustard.Base.Toolset.WinAPI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Mustard.UI.MVVM;

public class MustardMessageBox
{
    private Popup displayPopup;
    private Point cPos;
    private MessageBoxResult _dialogResult;

    private static IntPtr _dialogOwnerHandle;
    private static ArrayList _threadWindowHandles;
    private static MustardMessageBox mustardMessageBox;
    private static DispatcherFrame _dispatcherFrame;
    private static IntPtr dialogActiveHandle;
    private HwndSource sourceHandle;

    private MustardMessageBox()
    {
    }

    private void InitElementComponents(string caption, string message, MessageBoxButton messageBoxButton, MessageBoxResult defaultMessageBoxResult, MessageBoxImage messageBoxImage)
    {
        displayPopup = new Popup();
        displayPopup.Width = 650;
        displayPopup.Height = 250;
        displayPopup.AllowsTransparency = true;
        displayPopup.PlacementRectangle = new Rect(
            new Point((SystemParameters.PrimaryScreenWidth - displayPopup.Width) / 2,
                (SystemParameters.PrimaryScreenHeight - displayPopup.Height * 3) / 2),
            new Size(displayPopup.Width,
                displayPopup.Height));
        displayPopup.Effect = new DropShadowEffect
        {
            BlurRadius = 10,
            ShadowDepth = 10,
            Direction = 100,
            Opacity = 1,
        };
        _dialogResult = defaultMessageBoxResult;
        var border = new Border
        {
            BorderThickness = new Thickness(1, 5, 1, 1),
        };
        border.SetResourceReference(Border.BackgroundProperty, "Major_Background");
        border.SetResourceReference(Border.BorderBrushProperty, "Border_BorderBrush");
        displayPopup.Child = border;

        var hostGrid = new Grid();
        hostGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35, GridUnitType.Pixel) });
        hostGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        hostGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(45, GridUnitType.Pixel) });
        border.Child = hostGrid;

        var msgBoxImgPathData = "M20 2H4C2.9 2 2 2.9 2 4V22L6 18H20C21.11 18 22 17.11 22 16V4C22 2.9 21.11 2 20 2M20 16H6L4 18V4H20M17 11H15V9H17M13 11H11V9H13M9 11H7V9H9";

        if (messageBoxImage == MessageBoxImage.Warning)
        {
            msgBoxImgPathData = "M13,13H11V7H13M13,17H11V15H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";
            var messageBoxImagePath = new Path
            {
                Data = Geometry.Parse(msgBoxImgPathData),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Stretch = Stretch.Uniform,
                Opacity = 0.1,
                Height = 400,
                Width = 400,
                Margin = new Thickness(0, 0, -100, 0),
            };
            messageBoxImagePath.SetResourceReference(Path.FillProperty, "MessageBoxImage_Warning");
            Grid.SetRow(messageBoxImagePath, 0);
            Grid.SetRowSpan(messageBoxImagePath, 3);
            hostGrid.Children.Add(messageBoxImagePath);
        }
        if (messageBoxImage == MessageBoxImage.Question)
        {
            msgBoxImgPathData = "M12,2C8.14,2 5,5.14 5,9C5,14.25 12,22 12,22C12,22 19,14.25 19,9C19,5.14 15.86,2 12,2M12.88,15.75H11.13V14H12.88M12.88,12.88H11.13C11.13,10.04 13.75,10.26 13.75,8.5A1.75,1.75 0 0,0 12,6.75A1.75,1.75 0 0,0 10.25,8.5H8.5A3.5,3.5 0 0,1 12,5A3.5,3.5 0 0,1 15.5,8.5C15.5,10.69 12.88,10.91 12.88,12.88Z";
            var messageBoxImagePath = new Path
            {
                Data = Geometry.Parse(msgBoxImgPathData),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Stretch = Stretch.Uniform,
                Opacity = 0.25,
                Height = 450,
                Width = 450,
                Margin = new Thickness(0, 0, -50, 0),
            };
            messageBoxImagePath.SetResourceReference(Path.FillProperty, "MessageBoxImage_Question");
            Grid.SetRow(messageBoxImagePath, 0);
            Grid.SetRowSpan(messageBoxImagePath, 3);
            hostGrid.Children.Add(messageBoxImagePath);
        }
        if (messageBoxImage == MessageBoxImage.Error)
        {
            msgBoxImgPathData = "M13,14H11V10H13M13,18H11V16H13M1,21H23L12,2L1,21Z";
            var messageBoxImagePath = new Path
            {
                Data = Geometry.Parse(msgBoxImgPathData),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Stretch = Stretch.Uniform,
                Opacity = 0.25,
                Height = 600,
                Width = 600,
                Margin = new Thickness(0, -200, -150, 0),
            };
            messageBoxImagePath.SetResourceReference(Path.FillProperty, "MessageBoxImage_Error");
            Grid.SetRow(messageBoxImagePath, 0);
            Grid.SetRowSpan(messageBoxImagePath, 3);
            hostGrid.Children.Add(messageBoxImagePath);
        }
        if (messageBoxImage == MessageBoxImage.Information)
        {
            msgBoxImgPathData = "M21 11.1V8C21 6.9 20.1 6 19 6H11L9 4H3C1.9 4 1 4.9 1 6V18C1 19.1 1.9 20 3 20H10.3C11.6 21.9 13.8 23 16 23C19.9 23 23 19.9 23 16C23 14.2 22.3 12.4 21 11.1M16 21C13.2 21 11 18.8 11 16S13.2 11 16 11 21 13.2 21 16 18.8 21 16 21M17 20H15V15H17V20M17 14H15V12H17V14Z";
            var messageBoxImagePath = new Path
            {
                Data = Geometry.Parse(msgBoxImgPathData),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Stretch = Stretch.Uniform,
                Opacity = 0.1,
                Height = 500,
                Width = 500,
                Margin = new Thickness(0, 0, -100, 0),
            };
            messageBoxImagePath.SetResourceReference(Path.FillProperty, "MessageBoxImage_Information");
            Grid.SetRow(messageBoxImagePath, 0);
            Grid.SetRowSpan(messageBoxImagePath, 3);
            hostGrid.Children.Add(messageBoxImagePath);
        }

        var captionBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
        };
        captionBorder.MouseMove += MustardMessageBoxDropMove;
        hostGrid.Children.Add(captionBorder);

        var captionGrid = new Grid();
        Grid.SetRow(captionGrid, 0);
        captionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        captionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        captionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        hostGrid.Children.Add(captionGrid);

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
            displayPopup.IsOpen = false;
            Dispatcher.ExitAllFrames();
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
            Margin = new Thickness(10, 0, 0, 0),
            FontSize = 18,
            Text = caption,
            FontFamily = new FontFamily("Microsoft Yahei Bold"),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Opacity = 0.85
        };
        captionTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "Sub1_Foreground");
        Grid.SetColumn(captionTextBlock, 1);
        captionGrid.Children.Add(captionTextBlock);

        // Caption Icon.
        var iconPath = new Path
        {
            Data = Geometry.Parse(msgBoxImgPathData),
            Margin = new Thickness(15, 2, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Stretch = Stretch.Uniform,
            Opacity = 0.5,
            Height = 20
        };
        iconPath.SetResourceReference(Path.FillProperty, "Sub1_Foreground");
        Grid.SetColumn(iconPath, 0);
        captionGrid.Children.Add(iconPath);

        // Message.
        var contentTextBlock = new TextBlock
        {
            Margin = new Thickness(90, 0, 0, 0),
            FontSize = 16,
            Text = message,
            FontFamily = new FontFamily("Microsoft Yahei"),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        contentTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "Sub1_Foreground");
        Grid.SetRow(contentTextBlock, 1);
        hostGrid.Children.Add(contentTextBlock);

        // Buttons.
        var buttonStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 5, 0),
        };
        if (messageBoxButton == MessageBoxButton.OK || messageBoxButton == MessageBoxButton.OKCancel)
        {
            var okButton = new Button
            {
                Width = 90,
                Margin = new Thickness(1, 2, 2, 2),
                BorderThickness = new Thickness(0),
            };
            okButton.Click += (_, _) =>
            {
                _dialogResult = MessageBoxResult.OK;
                displayPopup.IsOpen = false;
                Dispatcher.ExitAllFrames();
                if (_threadWindowHandles != null)
                {
                    EnableThreadWindows(state: true);
                }
                if (dialogActiveHandle != IntPtr.Zero && IsWindow(new HandleRef(null, dialogActiveHandle)))
                {
                    TrySetFocus(new HandleRef(null, dialogActiveHandle), ref dialogActiveHandle);
                }
            };
            var okImgPath = new Path
            {
                Data = Geometry.Parse("M20,12A8,8 0 0,1 12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4C12.76,4 13.5,4.11 14.2,4.31L15.77,2.74C14.61,2.26 13.34,2 12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12M7.91,10.08L6.5,11.5L11,16L21,6L19.59,4.58L11,13.17L7.91,10.08Z"),
                Margin = new Thickness(5, 2, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Stretch = Stretch.Uniform,
                Opacity = 0.5,
                Height = 16,
            };
            okImgPath.SetResourceReference(Path.FillProperty, "Sub1_Foreground");
            var okBtnText = new TextBlock
            {
                Text = "确认",
                FontSize = 16,
                FontFamily = new FontFamily("Microsoft Yahei"),
            };
            var okBtnContentStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
            };
            okBtnContentStackPanel.Children.Add(okImgPath);
            okBtnContentStackPanel.Children.Add(okBtnText);
            okButton.Content = okBtnContentStackPanel;

            okButton.IsDefault = true;

            buttonStackPanel.Children.Add(okButton);
        }
        if (messageBoxButton == MessageBoxButton.YesNoCancel || messageBoxButton == MessageBoxButton.YesNo)
        {
            var yesButton = new Button
            {
                Width = 90,
                Margin = new Thickness(1, 2, 2, 2),
                BorderThickness = new Thickness(0),
            };
            yesButton.Click += (_, _) =>
            {
                _dialogResult = MessageBoxResult.Yes;
                displayPopup.IsOpen = false;
                Dispatcher.ExitAllFrames();
                if (_threadWindowHandles != null)
                {
                    EnableThreadWindows(state: true);
                }
                if (dialogActiveHandle != IntPtr.Zero && IsWindow(new HandleRef(null, dialogActiveHandle)))
                {
                    TrySetFocus(new HandleRef(null, dialogActiveHandle), ref dialogActiveHandle);
                }
            };
            var yesImgPath = new Path
            {
                Data = Geometry.Parse("M0.41,13.41L6,19L7.41,17.58L1.83,12M22.24,5.58L11.66,16.17L7.5,12L6.07,13.41L11.66,19L23.66,7M18,7L16.59,5.58L10.24,11.93L11.66,13.34L18,7Z"),
                Margin = new Thickness(5, 2, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Stretch = Stretch.Uniform,
                Opacity = 0.5,
                Height = 16,
            };
            yesImgPath.SetResourceReference(Path.FillProperty, "Sub1_Foreground");
            var yesBtnText = new TextBlock
            {
                Text = "是",
                FontSize = 16,
                FontFamily = new FontFamily("Microsoft Yahei"),
            };
            var yesBtnContentStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
            };
            yesBtnContentStackPanel.Children.Add(yesImgPath);
            yesBtnContentStackPanel.Children.Add(yesBtnText);
            yesButton.Content = yesBtnContentStackPanel;

            yesButton.IsDefault = true;

            buttonStackPanel.Children.Add(yesButton);

            var noButton = new Button
            {
                Width = 90,
                Margin = new Thickness(1, 2, 2, 2),
                BorderThickness = new Thickness(0),
            };
            noButton.Click += (_, _) =>
            {
                _dialogResult = MessageBoxResult.No;
                displayPopup.IsOpen = false;
                Dispatcher.ExitAllFrames();
                if (_threadWindowHandles != null)
                {
                    EnableThreadWindows(state: true);
                }
                if (dialogActiveHandle != IntPtr.Zero && IsWindow(new HandleRef(null, dialogActiveHandle)))
                {
                    TrySetFocus(new HandleRef(null, dialogActiveHandle), ref dialogActiveHandle);
                }
            };
            var noImgPath = new Path
            {
                Data = Geometry.Parse("M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z"),
                Margin = new Thickness(5, 2, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Stretch = Stretch.Uniform,
                Opacity = 0.5,
                Height = 16,
            };
            noImgPath.SetResourceReference(Path.FillProperty, "Sub1_Foreground");
            var noBtnText = new TextBlock
            {
                Text = "否",
                FontSize = 16,
                FontFamily = new FontFamily("Microsoft Yahei"),
            };
            var noBtnContentStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
            };
            noBtnContentStackPanel.Children.Add(noImgPath);
            noBtnContentStackPanel.Children.Add(noBtnText);
            noButton.Content = noBtnContentStackPanel;

            buttonStackPanel.Children.Add(noButton);
        }
        if (messageBoxButton == MessageBoxButton.YesNoCancel || messageBoxButton == MessageBoxButton.OKCancel)
        {
            var cancelButton = new Button
            {
                Width = 90,
                Margin = new Thickness(1, 2, 2, 2),
                BorderThickness = new Thickness(0),
            };
            cancelButton.Click += (_, _) =>
            {
                _dialogResult = MessageBoxResult.Cancel;
                displayPopup.IsOpen = false;
                Dispatcher.ExitAllFrames();
                if (_threadWindowHandles != null)
                {
                    EnableThreadWindows(state: true);
                }
                if (dialogActiveHandle != IntPtr.Zero && IsWindow(new HandleRef(null, dialogActiveHandle)))
                {
                    TrySetFocus(new HandleRef(null, dialogActiveHandle), ref dialogActiveHandle);
                }
            };
            var cancelImgPath = new Path
            {
                Data = Geometry.Parse("M19,3H5C3.89,3 3,3.89 3,5V9H5V5H19V19H5V15H3V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5C21,3.89 20.1,3 19,3M10.08,15.58L11.5,17L16.5,12L11.5,7L10.08,8.41L12.67,11H3V13H12.67L10.08,15.58Z"),
                Margin = new Thickness(5, 2, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Stretch = Stretch.Uniform,
                Opacity = 0.5,
                Height = 16,
            };
            cancelImgPath.SetResourceReference(Path.FillProperty, "Sub1_Foreground");
            var cancelBtnText = new TextBlock
            {
                Text = "取消",
                FontSize = 16,
                FontFamily = new FontFamily("Microsoft Yahei"),
            };
            var cancelBtnContentStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
            };
            cancelBtnContentStackPanel.Children.Add(cancelImgPath);
            cancelBtnContentStackPanel.Children.Add(cancelBtnText);
            cancelButton.Content = cancelBtnContentStackPanel;

            buttonStackPanel.Children.Add(cancelButton);
        }
        Grid.SetRow(buttonStackPanel, 2);
        hostGrid.Children.Add(buttonStackPanel);
        displayPopup.IsOpen = true;

        displayPopup.PreviewMouseDown += (_, _) =>
        {
            SetFocus(sourceHandle.Handle);
            SetForegroundWindow(sourceHandle.Handle);
        };

        sourceHandle = (HwndSource)PresentationSource.FromVisual(displayPopup.Child);
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
                if (cPos.Y + displayPopup.Height / 2 + 1 > SystemParameters.PrimaryScreenHeight / 2)
                {
                    cPos.Y = SystemParameters.PrimaryScreenHeight / 2 - displayPopup.Height / 2;
                }
                displayPopup.PlacementRectangle = new Rect(
                    new Point((SystemParameters.PrimaryScreenWidth - displayPopup.Width) / 2 + cPos.X,
                        (SystemParameters.PrimaryScreenHeight - displayPopup.Height * 3) / 2 + cPos.Y),
                    new Size(displayPopup.Width,
                        displayPopup.Height));
            }
        }
    }

    private static void SetDialogHandle()
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
        if (capture != IntPtr.Zero)
        {
            IntReleaseCapture();
        }
    }

    private static bool ThreadWindowsCallback(IntPtr hWnd, IntPtr lparam)
    {
        if (IsWindowVisible(new HandleRef(null, hWnd)) && IsWindowEnabled(new HandleRef(null, hWnd)))
        {
            _threadWindowHandles.Add(hWnd);
        }
        return true;
    }

    private static void EnableThreadWindows(bool state)
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

    private static MessageBoxResult CreateMessage(string caption, string message, MessageBoxButton messageBoxButton, MessageBoxResult defaultMessageBoxResult, MessageBoxImage messageBoxImage)
    {
        SetDialogHandle();
        var thread = new Thread(() =>
        {
            mustardMessageBox = new MustardMessageBox();
            mustardMessageBox.InitElementComponents(caption, message, messageBoxButton, defaultMessageBoxResult, messageBoxImage);
            Dispatcher.Run();
            _dispatcherFrame.Continue = false;
        })
        { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        // 阻塞
        ComponentDispatcher.PushModal();
        _dispatcherFrame = new DispatcherFrame();
        Dispatcher.PushFrame(_dispatcherFrame);
        ComponentDispatcher.PopModal();
        return mustardMessageBox._dialogResult;
    }

    public static MessageBoxResult Show(string message)
        => Show("提示", message);
    public static MessageBoxResult Show(string caption, string message)
        => Show(caption, message, MessageBoxButton.OKCancel);
    public static MessageBoxResult Show(string caption, string message, MessageBoxButton messageBoxButton)
        => Show(caption, message, messageBoxButton, MessageBoxResult.Cancel);
    public static MessageBoxResult Show(string caption, string message, MessageBoxButton messageBoxButton, MessageBoxResult defaultMessageBoxResult)
        => Show(caption, message, messageBoxButton, defaultMessageBoxResult, MessageBoxImage.None);
    public static MessageBoxResult Show(string caption, string message, MessageBoxButton messageBoxButton, MessageBoxResult defaultMessageBoxResult, MessageBoxImage messageBoxImage)
        => CreateMessage(caption, message, messageBoxButton, defaultMessageBoxResult, messageBoxImage);
}
