using System;
using System.Collections;
using System.ComponentModel;
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

namespace Mustard.UI.Sunflower.ExControls;

public class WaitControl : IDisposable
{
    private DispatcherFrame lockedFrame;
    private int displayCenterX;
    private int displayCenterY;
    private int displayWidth = 1200;
    private int displayHeight = 1200;
    private IntPtr _dialogOwnerHandle;
    private bool isDisposed;
    private ArrayList _threadWindowHandles;
    private Dispatcher dispatcher;
    private ManualResetEventSlim slim;
    private ManualResetEventSlim rslim;
    private Popup popup;

    public void Wait(string message = null)
    {
        rslim = new ManualResetEventSlim();
        _threadWindowHandles = new ArrayList();
        Application.Current.Dispatcher.Invoke(() =>
        {
            _dialogOwnerHandle = GetActiveWindow();
        });
        if (_dialogOwnerHandle != IntPtr.Zero && _dialogOwnerHandle == GetDesktopWindow())
        {
            _dialogOwnerHandle = IntPtr.Zero;
        }
        if (_dialogOwnerHandle != IntPtr.Zero)
        {
            int num = 0;
            while (_dialogOwnerHandle != IntPtr.Zero)
            {
                num = WGetWindowLong(new HandleRef(this, _dialogOwnerHandle), -16);
                if ((num & 0x40000000) != 1073741824)
                {
                    break;
                }
                _dialogOwnerHandle = WGetParent(new HandleRef(null, _dialogOwnerHandle));
            }
        }
        Application.Current.Dispatcher.Invoke(() =>
        {
            EnumThreadWindows(GetCurrentThreadId(), ThreadWindowsCallback, new HandleRef(null, IntPtr.Zero));
        });
        var capture = GetCapture();
        if (capture != IntPtr.Zero)
        {
            IntReleaseCapture();
        }
        EnableThreadWindows(false);
        var staThread = new Thread(() =>
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            lockedFrame = new DispatcherFrame();
            ComponentDispatcher.PushModal();
            LocalWait(message);
            Dispatcher.Run();
            ComponentDispatcher.PopModal();
            popup = null;
            if (_dialogOwnerHandle != IntPtr.Zero)
            {
                SetFocus(_dialogOwnerHandle);
                SetForegroundWindow(_dialogOwnerHandle);
            }
        })
        { IsBackground = true };
        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();
    }

    private void EnableThreadWindows(bool state)
    {
        if (_threadWindowHandles == null) return;
        lock (_threadWindowHandles)
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
    }

    private int WGetWindowLong(HandleRef hWnd, int nIndex)
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
            num = (int)(zero);
        }

        if (zero == IntPtr.Zero)
        {
        }
        return num;
    }

    private bool ThreadWindowsCallback(IntPtr hWnd, IntPtr lparam)
    {
        if (IsWindowVisible(new HandleRef(null, hWnd)) && IsWindowEnabled(new HandleRef(null, hWnd)))
        {
            _threadWindowHandles.Add(hWnd);
        }

        return true;
    }

    private IntPtr WGetParent(HandleRef hWnd)
    {
        IntPtr parent = GetParent(hWnd);
        int lastWin32Error = Marshal.GetLastWin32Error();
        if (parent == IntPtr.Zero && lastWin32Error != 0)
        {
            throw new Win32Exception(lastWin32Error);
        }
        return parent;
    }

    private void LocalWait(string displayText)
    {
        displayCenterX = (int)((SystemParameters.FullPrimaryScreenWidth - displayWidth) / 2);
        displayCenterY = (int)((SystemParameters.FullPrimaryScreenHeight - displayHeight) / 2);
        if (_dialogOwnerHandle != IntPtr.Zero && _dialogOwnerHandle == GetDesktopWindow())
        {
            _dialogOwnerHandle = IntPtr.Zero;
        }
        if (_dialogOwnerHandle != IntPtr.Zero)
        {
            RECT r = new RECT();
            GetWindowRect(_dialogOwnerHandle, ref r);
            var width = r.Right - Math.Abs(r.Left);
            var height = r.Bottom - Math.Abs(r.Top);
            displayWidth = width;
            displayHeight = height;
            displayCenterX = Math.Max(0, r.Left) + (width - displayWidth) / 2;
            displayCenterY = Math.Max(r.Top, 0);
            MakeDisplay(displayText);
        }
        rslim.Set();
    }

    public void Dispose()
    {
        rslim.Wait();
        isDisposed = true;
        if (lockedFrame != null)
        {
            lockedFrame.Continue = false;
        }
        if (slim != null)
        {
            slim.Wait();
        }
        EnableThreadWindows(true);
    }

    private Popup MakeDisplay(string displayText)
    {
        popup = new Popup();
        popup.Placement = PlacementMode.Absolute;
        popup.PlacementRectangle = new System.Windows.Rect(
            new Point(displayCenterX, displayCenterY),
            new Size(displayWidth, displayHeight));
        popup.Width = displayWidth;
        popup.Height = displayHeight;

        popup.Focusable = false;
        popup.AllowsTransparency = true;

        var borderBackground = Brushes.Black;
        borderBackground = new SolidColorBrush(Color.FromRgb(0x1C, 0x4F, 0x81)) { Opacity = .8 };

        Color majorBackground = default;
        Color sub1Background = default;
        Color sub3Background = default;

        Application.Current.Dispatcher.Invoke(() =>
        {
            majorBackground = ((SolidColorBrush)(Application.Current.FindResource("Major_Background") ?? new SolidColorBrush(Color.FromArgb(0xDF, 0x1C, 0x4F, 0x81)))).Color;
            sub1Background = ((SolidColorBrush)(Application.Current.FindResource("Sub1_Background") ?? new SolidColorBrush(Color.FromArgb(0xFF, 0x0C, 0x1F, 0x41)))).Color;
            sub3Background = ((SolidColorBrush)(Application.Current.FindResource("Sub3_Background") ?? new SolidColorBrush(Color.FromArgb(0xFF, 0x0C, 0x1F, 0x41)))).Color;
        });

        LinearGradientBrush gradientBrush = new LinearGradientBrush();
        gradientBrush.GradientStops.Add(new GradientStop { Color = majorBackground, Offset = 0 });
        gradientBrush.GradientStops.Add(new GradientStop { Color = sub1Background, Offset = 0.5 });
        gradientBrush.GradientStops.Add(new GradientStop { Color = majorBackground, Offset = 1 });

        var hostBorder = new Border { };

        hostBorder.Effect = new DropShadowEffect
        {
            BlurRadius = 30,
            ShadowDepth = 10,
            Color = sub1Background,
            Opacity = 0.8,
            Direction = -90
        };

        var hostGrid = new Grid
        {
            Margin = new Thickness(1),
            Background = gradientBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var hostStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Path shape = new Path();
        shape.Width = 50;
        shape.Height = 50;
        shape.Margin = new Thickness(30, 20, 30, 20);
        shape.VerticalAlignment = VerticalAlignment.Center;
        shape.HorizontalAlignment = HorizontalAlignment.Center;
        shape.RenderTransformOrigin = new Point(0.45, 0.5);
        shape.Opacity = 0.7;
        shape.Stretch = Stretch.Uniform;
        shape.Fill = new SolidColorBrush(sub3Background);
        shape.Data = Geometry.Parse("M377 249 377 323.323 371.392 324.173C349.395 328.645 330.124 340.548 316.351 357.128L314.942 359 250 321.751 258.861 309.978C286.664 276.508 327.154 253.827 373.017 249.2L377 249Z M543.246 345 546.572 351.865C555.219 372.192 560 394.541 560 418 560 441.459 555.219 463.808 546.572 484.135L543.246 491 479.239 454.255 481.911 446.997C484.737 437.962 486.26 428.355 486.26 418.394 486.26 408.434 484.737 398.827 481.911 389.791L479 381.883 543.246 345Z M314.529 477 316.351 479.427C330.124 496.042 349.395 507.971 371.392 512.452L377 513.305 377 587 373.017 586.8C327.154 582.163 286.664 559.432 258.861 525.891L250 514.092 314.529 477Z M358.16 405.828 357.598 406.973C356.137 410.362 355.329 414.089 355.329 418 355.329 421.911 356.137 425.638 357.598 429.027L358.16 430.172 369.083 423.982 368.655 422.835C368.178 421.328 367.921 419.727 367.921 418.066 367.921 416.405 368.178 414.803 368.655 413.297L369.124 412.041ZM357.488 403 371 410.656 370.423 412.204C369.834 414.06 369.517 416.034 369.517 418.081 369.517 420.128 369.834 422.102 370.423 423.958L370.95 425.372 357.488 433 356.796 431.589C354.996 427.413 354 422.82 354 418 354 413.18 354.996 408.587 356.796 404.411Z M395.063 451.399 396.339 451.303C400.016 450.847 403.665 449.662 407.076 447.693 410.487 445.723 413.338 443.156 415.572 440.199L416.292 439.142 405.497 432.909 404.708 433.853C403.63 435.02 402.36 436.047 400.911 436.883 399.463 437.719 397.939 438.305 396.389 438.655L395.063 438.887ZM392.928 453.398 392.928 437.978 394.563 437.693C396.473 437.262 398.351 436.539 400.136 435.509 401.921 434.478 403.486 433.213 404.814 431.775L405.787 430.611 419.091 438.292 418.203 439.595C415.45 443.239 411.937 446.403 407.733 448.83 403.529 451.257 399.032 452.718 394.5 453.28Z M416.292 396.509 415.572 395.452C413.338 392.495 410.487 389.928 407.076 387.958 403.665 385.989 400.016 384.804 396.339 384.348L395.063 384.252 395.063 396.718 396.275 396.929C397.824 397.279 399.348 397.866 400.797 398.702 402.245 399.538 403.515 400.565 404.593 401.732L405.457 402.765ZM419.091 397.359 405.737 405.068 404.673 403.795C403.344 402.356 401.779 401.091 399.995 400.061 398.21 399.03 396.331 398.308 394.422 397.876L392.928 397.616 392.928 382.253 394.5 382.371C399.032 382.933 403.529 384.394 407.733 386.821 411.937 389.248 415.45 392.412 418.203 396.056Z M390 412 390 414.703 389.779 414.734C388.913 414.896 388.155 415.329 387.612 415.932L387.557 416 385 414.645 385.349 414.217C386.444 413 388.038 412.176 389.843 412.007L390 412Z M390 412 390.157 412.007C391.962 412.176 393.557 413.003 394.651 414.224L395 414.653 392.465 416 392.419 415.943C391.877 415.339 391.118 414.905 390.252 414.741L390 414.706 390 412Z M384.616 415 387 416.276 386.898 416.534C386.794 416.843 386.738 417.172 386.738 417.513 386.738 417.855 386.794 418.184 386.898 418.493L386.991 418.729 384.616 420 384.493 419.765C384.176 419.069 384 418.303 384 417.5 384 416.697 384.176 415.931 384.493 415.235L384.616 415Z M395.379 415 395.503 415.235C395.823 415.931 396 416.697 396 417.5 396 418.303 395.823 419.069 395.503 419.765L395.379 420 393.009 418.742 393.108 418.493C393.212 418.184 393.269 417.855 393.269 417.513 393.269 417.172 393.212 416.843 393.108 416.534L393 416.263 395.379 415Z M387.541 420 387.612 420.088C388.155 420.692 388.913 421.126 389.779 421.289L390 421.32 390 424 389.843 423.993C388.038 423.824 386.444 422.997 385.349 421.778L385 421.349 387.541 420Z M392.481 420 395 421.341 394.651 421.771C393.557 422.995 391.962 423.823 390.157 423.993L390 424 390 421.317 390.252 421.281C391.118 421.118 391.877 420.683 392.419 420.077L392.481 420Z");
        RotateTransform rotateTransform = new RotateTransform();
        shape.RenderTransform = rotateTransform;
        DropShadowEffect dropShadowEffect = new DropShadowEffect();
        dropShadowEffect.BlurRadius = 20;
        dropShadowEffect.Opacity = 0.5;
        dropShadowEffect.ShadowDepth = 15;
        dropShadowEffect.Color = Colors.Aqua;
        shape.Effect = dropShadowEffect;
        hostStackPanel.Children.Add(shape);
        if (displayText != null)
        {
            hostStackPanel.Children.Add(new TextBlock
            {
                Text = displayText,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 20, 30, 20),
                FontSize = 20,
                FontFamily = new FontFamily("Microsoft YaHei"),
                Foreground = new SolidColorBrush(sub3Background),
            });
        }

        hostGrid.Children.Add(hostStackPanel);
        hostBorder.Child = hostGrid;

        popup.Child = hostBorder;
        popup.IsOpen = true;
        var popupUpdateThread = new Thread(() =>
        {
            int rotateAngle = 0;
            slim = new ManualResetEventSlim();
            while (true)
            {
                rotateAngle = (rotateAngle + 1) % 360;
                var exit = false;
                dispatcher.Invoke(() =>
                {
                    if (isDisposed)
                    {
                        popup.IsOpen = false;
                        exit = true;
                        return;
                    }
                    rotateTransform.Angle = rotateAngle;
                    dropShadowEffect.Direction = rotateAngle - 20;
                });
                if (exit)
                {
                    break;
                }
                Thread.Sleep(30);
            }
            slim.Set();
            dispatcher.InvokeShutdown();
        })
        { IsBackground = true };
        popupUpdateThread.Start();
        return popup;
    }
}