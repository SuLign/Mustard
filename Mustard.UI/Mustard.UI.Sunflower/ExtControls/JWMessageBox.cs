using Mustard.UI.Sunflower.Controls;

using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime;
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

public class JWMessageBox
{
    private MessageBoxResult result;
    private ArrayList _threadWindowHandles;
    private IntPtr _dialogOwnerHandle;
    private DispatcherFrame _dispatcherFrame;
    private double primeryScreenHeight;
    private static Dispatcher invokeDispatcher;
    private Dispatcher runningDispatcher;

    public static MessageBoxResult Show(
        string message,
        string caption = null,
        string title = null,
        MessageBoxButton buttons = MessageBoxButton.OKCancel,
        MessageBoxImage icon = MessageBoxImage.Warning,
        MessageBoxResult defaultResult = MessageBoxResult.None)
    {
        JWMessageBox messageBox = new JWMessageBox();
        messageBox.Title = title ?? "提示";
        messageBox.Caption = caption ?? "提示";
        messageBox.Content = message;
        invokeDispatcher = Application.Current.Dispatcher;
        messageBox.CreateHostVirtual(message, caption, title, buttons, icon, defaultResult);
        return messageBox.result;
    }

    internal void GetPopup(string message, string caption, string title, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defaultResult)
    {
        var popup = new Popup
        {
            AllowsTransparency = true,
        };

        popup.BeginInit();
        var hostBorder = new Border
        {
            BorderThickness = new Thickness(0),
        };
        var titleBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0x01, 0x45, 0x45, 0x45)),
        };
        var titleGrid = new Grid
        {
            Margin = new Thickness(0, 0, 10, 0),
        };
        var closeBtn = new Button
        {
            Height = 20,
            Width = 20,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        var noBtn = new Button
        {
            Width = 80,
            Height = 25,
            Margin = new Thickness(0, 0, 5, 0),
            Content = "否",
            Background = Brushes.Transparent
        };
        var okBtn = new Button
        {
            Width = 80,
            Height = 25,
            Margin = new Thickness(0, 0, 5, 0),
            Content = "确定",
            Background = Brushes.Transparent
        };
        var yesBtn = new Button
        {
            Width = 80,
            Height = 25,
            Margin = new Thickness(0, 0, 5, 0),
            Content = "是",
            Background = Brushes.Transparent
        };
        var cancelBtn = new Button
        {
            Width = 80,
            Height = 25,
            Margin = new Thickness(0, 0, 5, 0),
            Content = "取消",
            Background = Brushes.Transparent
        };
        popup.EndInit();
        primeryScreenHeight = SystemParameters.PrimaryScreenHeight;
        hostBorder.SetResourceReference(Border.BorderBrushProperty, "Default_Bottom_Background");
        popup.Width = 490;
        popup.Height = 240;
        popup.PlacementRectangle = new System.Windows.Rect(
            new Point((SystemParameters.PrimaryScreenWidth - popup.Width) / 2, SystemParameters.PrimaryScreenHeight / 2 - popup.Height * 1.5),
            new Size(popup.Width, popup.Height));
        popup.IsOpen = true;
        popup.Focusable = true;
        popup.AllowsTransparency = true;
        popup.Child = hostBorder;
        var sourceHandle = (HwndSource)PresentationSource.FromVisual(popup.Child);
        popup.PreviewMouseDown += (_, _) =>
        {
            SetFocus(sourceHandle.Handle);
            SetForegroundWindow(sourceHandle.Handle);
        };
        SetFocus(sourceHandle.Handle);
        SetForegroundWindow(sourceHandle.Handle);
        var hostGrid = new Grid
        {
            ClipToBounds = true,
            Margin = new Thickness(20)
        };
        hostGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45, GridUnitType.Pixel) });
        hostGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20, GridUnitType.Pixel) });
        hostGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
        hostGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45, GridUnitType.Pixel) });
        hostBorder.Child = hostGrid;

        hostGrid.SetResourceReference(Grid.BackgroundProperty, "Default_Major_Background");
        hostBorder.Effect = new DropShadowEffect
        {
            BlurRadius = 20,
            ShadowDepth = 2,
            Color = Colors.Black,
            RenderingBias = RenderingBias.Quality,
            Direction = -45,
            Opacity = .5,
        };

        var titlePanel = new StackPanel() { VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
        titleBorder.Child = titleGrid;
        Point oldPos = new Point(popup.PlacementRectangle.X, popup.PlacementRectangle.Y);
        Point cursorRelatedPos = new Point();
        bool isCaptured = false;
        titleBorder.MouseMove += (_, _) =>
        {
            while ((GetAsyncKeyState(VirtualKeyStates.VK_LBUTTON) & 0x8000) != 0)
            {
                GetCursorPos(out var pos);
                if (!isCaptured)
                {
                    oldPos = new Point(popup.PlacementRectangle.X, popup.PlacementRectangle.Y);
                    cursorRelatedPos = new Point(pos.X - oldPos.X, pos.Y - oldPos.Y);
                    isCaptured = true;
                }
                var pt = new Point(pos.X - cursorRelatedPos.X, pos.Y - cursorRelatedPos.Y);
                if (pt.Y + popup.Height * 2 >= primeryScreenHeight)
                {
                    pt.Y = (int)(primeryScreenHeight - popup.Height * 2);
                }
                popup.PlacementRectangle = new System.Windows.Rect(
                    pt,
                    new Size(popup.Width, popup.Height));
            }
            isCaptured = false;
            oldPos = new Point(popup.PlacementRectangle.X, popup.PlacementRectangle.Y);
        };
        titleGrid.Children.Add(new Rectangle { Fill = Brushes.Transparent });

        var closeBtnIcon = new Path
        {
            Data = Geometry.Parse("M13.46,12L19,17.54V19H17.54L12,13.46L6.46,19H5V17.54L10.54,12L5,6.46V5H6.46L12,10.54L17.54,5H19V6.46L13.46,12Z"),
            Stretch = Stretch.Uniform,
            Height = 8
        };
        closeBtnIcon.SetResourceReference(Path.FillProperty, "Default_Major_Foreground");
        closeBtn.Content = closeBtnIcon;

        closeBtn.Click += (_, _) =>
        {
            popup.IsOpen = false;
            EnableThreadWindows(state: true);
            runningDispatcher?.Invoke(() =>
            {
                Dispatcher.ExitAllFrames();
            });
        };
        titleGrid.Children.Add(closeBtn);

        titleGrid.Children.Add(titlePanel);
        Geometry pathData = null;
        if (icon == MessageBoxImage.Hand) pathData = Geometry.Parse("M7,19H15V22H7V19M16.94,1C16.4,0.91 15.87,1.25 15.76,1.8L14.75,7.57C14.53,7.54 14.29,7.5 14,7.47L13.57,7.5L12.41,1.8C12.31,1.26 11.78,0.91 11.24,1C10.7,1.13 10.35,1.66 10.45,2.2L11.65,8.11L7.85,9.8C7.35,10 7,10.53 7,11.14V15.5C7,16.3 7.73,17 8.5,17H15C15.39,17 15.74,16.84 16,16.57L16.5,16.16C16.5,16.16 17,15.78 17,15.36V13C17,13 17,11.86 16.13,11.3L17.71,2.2C17.83,1.66 17.5,1.13 16.94,1Z");
        if (icon == MessageBoxImage.Question) pathData = Geometry.Parse("M3.05 13H1V11H3.05C3.5 6.83 6.83 3.5 11 3.05V1H13V3.05C17.17 3.5 20.5 6.83 20.95 11H23V13H20.95C20.5 17.17 17.17 20.5 13 20.95V23H11V20.95C6.83 20.5 3.5 17.17 3.05 13M12 5C8.13 5 5 8.13 5 12S8.13 19 12 19 19 15.87 19 12 15.87 5 12 5M11.13 17.25H12.88V15.5H11.13V17.25M12 6.75C10.07 6.75 8.5 8.32 8.5 10.25H10.25C10.25 9.28 11.03 8.5 12 8.5S13.75 9.28 13.75 10.25C13.75 12 11.13 11.78 11.13 14.63H12.88C12.88 12.66 15.5 12.44 15.5 10.25C15.5 8.32 13.93 6.75 12 6.75Z");
        if (icon == MessageBoxImage.Exclamation) pathData = Geometry.Parse("M10 3H14V14H10V3M10 21V17H14V21H10Z");
        if (icon == MessageBoxImage.Exclamation) pathData = Geometry.Parse("M12 2C6.5 2 2 6.5 2 12C2 17.5 6.5 22 12 22C17.5 22 22 17.5 22 12C22 6.5 17.5 2 12 2M12 20C7.61 20 4 16.39 4 12C4 7.61 7.61 4 12 4C16.39 4 20 7.61 20 12C20 16.39 16.39 20 12 20M13 10.27L15.83 8.63L16.83 10.37L14 12L16.83 13.63L15.83 15.37L13 13.73V17H11V13.73L8.17 15.37L7.17 13.63L10 12L7.17 10.37L8.17 8.63L11 10.27V7H13V10.27Z");
        if (icon == MessageBoxImage.Stop) pathData = Geometry.Parse("M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4M9,9V15H15V9");
        if (icon == MessageBoxImage.Error) pathData = Geometry.Parse("M11,15H13V17H11V15M11,7H13V13H11V7M12,2C6.47,2 2,6.5 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20Z");
        if (icon == MessageBoxImage.Warning) pathData = Geometry.Parse("M5,3H19A2,2 0 0,1 21,5V19A2,2 0 0,1 19,21H5A2,2 0 0,1 3,19V5A2,2 0 0,1 5,3M13,13V7H11V13H13M13,17V15H11V17H13Z");
        if (icon == MessageBoxImage.Warning) pathData = Geometry.Parse("M11,15H13V17H11V15M11,7H13V13H11V7M12,2C6.47,2 2,6.5 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20Z");
        if ((byte)icon == 0x80)
        {
            pathData = Geometry.Parse("M7,19H15V22H7V19M16.94,1C16.4,0.91 15.87,1.25 15.76,1.8L14.75,7.57C14.53,7.54 14.29,7.5 14,7.47L13.57,7.5L12.41,1.8C12.31,1.26 11.78,0.91 11.24,1C10.7,1.13 10.35,1.66 10.45,2.2L11.65,8.11L7.85,9.8C7.35,10 7,10.53 7,11.14V15.5C7,16.3 7.73,17 8.5,17H15C15.39,17 15.74,16.84 16,16.57L16.5,16.16C16.5,16.16 17,15.78 17,15.36V13C17,13 17,11.86 16.13,11.3L17.71,2.2C17.83,1.66 17.5,1.13 16.94,1Z");
        }
        if ((byte)icon == 0x81)
        {
            pathData = Geometry.Parse("M20,12A8,8 0 0,1 12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4C12.76,4 13.5,4.11 14.2,4.31L15.77,2.74C14.61,2.26 13.34,2 12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12M7.91,10.08L6.5,11.5L11,16L21,6L19.59,4.58L11,13.17L7.91,10.08Z");
        }

        Path titleIconImg = new Path
        {
            Stretch = Stretch.Uniform,
            Width = 15,
            Margin = new Thickness(10, 0, 0, 0),
            Height = 15,
            Opacity = 0.5,
            Data = pathData,
            VerticalAlignment = VerticalAlignment.Center,
        };
        titleIconImg.SetResourceReference(Path.FillProperty, "Default_Major_Foreground");
        titlePanel.Children.Add(titleIconImg);
        var titleTextBlock = new TextBlock
        {
            Text = title,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 15,
            Margin = new Thickness(15, 0, 15, 0),
        };
        titlePanel.Children.Add(titleTextBlock);
        titleTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "Default_Major_Foreground");
        titleTextBlock.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Microsoft YaHei Bold"));

        Grid contentGrid = new Grid();
        TextBlock messageTextBlock = new TextBlock
        {
            Text = message,
            FontSize = 11,
            Opacity = 0.8,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(25, 5, 15, 5),
        };
        messageTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "Default_Major_Foreground");

        TextBlock captionTextBlock = new TextBlock
        {
            Text = caption,
            FontSize = 11,
            Opacity = 1,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(25, 0, 15, 0),
        };
        captionTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "Default_Major_Foreground");

        StackPanel btnStackPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 10, 0)
        };

        var okBtnIcon = new Path
        {
            Data = Geometry.Parse("M21,7L9,19L3.5,13.5L4.91,12.09L9,16.17L19.59,5.59L21,7Z"),
        };
        okBtn.SetValue(ButtonStyle.ButtonIconProperty, Geometry.Parse("M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M10 17L5 12L6.41 10.59L10 14.17L17.59 6.58L19 8L10 17Z"));
        //okBtn.SetValue(ButtonStyle.ButtonIconMarginProperty, new Thickness(0, 4, 5, 4));
        yesBtn.SetValue(ButtonStyle.ButtonIconProperty, Geometry.Parse("M21,7L9,19L3.5,13.5L4.91,12.09L9,16.17L19.59,5.59L21,7Z"));
        //yesBtn.SetValue(ButtonStyle.ButtonIconMarginProperty, new Thickness(0, 5, 5, 5));

        noBtn.SetValue(ButtonStyle.ButtonIconProperty, Geometry.Parse("M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z"));
        //noBtn.SetValue(ButtonStyle.ButtonIconMarginProperty, new Thickness(0, 5, 5, 5));
        cancelBtn.SetValue(ButtonStyle.ButtonIconProperty, Geometry.Parse("M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z"));
        //cancelBtn.SetValue(ButtonStyle.ButtonIconMarginProperty, new Thickness(0, 4, 5, 4));

        if (buttons == MessageBoxButton.OK || buttons == MessageBoxButton.OKCancel)
        {
            btnStackPanel.Children.Add(okBtn);
            okBtn.IsDefault = true;
            okBtn.Focus();
        }
        if (buttons == MessageBoxButton.YesNo || buttons == MessageBoxButton.YesNoCancel)
        {
            btnStackPanel.Children.Add(yesBtn);
            yesBtn.IsDefault = true;
            yesBtn.Focus();
        }
        if (buttons == MessageBoxButton.YesNo || buttons == MessageBoxButton.YesNoCancel)
        {
            btnStackPanel.Children.Add(noBtn);
            yesBtn.IsDefault = true;
            yesBtn.Focus();
        }
        if (buttons == MessageBoxButton.OKCancel || buttons == MessageBoxButton.YesNoCancel)
        {
            btnStackPanel.Children.Add(cancelBtn);
            okBtn.IsDefault = true;
            okBtn.Focus();
        }

        okBtn.Click += (_, _) =>
        {
            result = MessageBoxResult.OK;
            popup.IsOpen = false;
            EnableThreadWindows(state: true);

            runningDispatcher?.Invoke(() =>
            {
                Dispatcher.ExitAllFrames();
            });
        };
        yesBtn.Click += (_, _) =>
        {
            result = MessageBoxResult.Yes;
            popup.IsOpen = false;
            EnableThreadWindows(state: true);

            runningDispatcher?.Invoke(() =>
            {
                Dispatcher.ExitAllFrames();
            });
        };
        noBtn.Click += (_, _) =>
        {
            result = MessageBoxResult.No;
            popup.IsOpen = false;
            EnableThreadWindows(state: true);
            runningDispatcher?.Invoke(() =>
            {
                Dispatcher.ExitAllFrames();
            });
        };
        cancelBtn.Click += (_, _) =>
        {
            result = MessageBoxResult.Cancel;
            popup.IsOpen = false;
            EnableThreadWindows(state: true);
            runningDispatcher?.Invoke(() =>
            {
                Dispatcher.ExitAllFrames();
            });
        };

        Grid.SetRow(titleBorder, 0);
        Grid.SetRow(captionTextBlock, 1);
        Grid.SetRow(contentGrid, 2);
        Grid.SetRow(btnStackPanel, 3);

        Path msgBoxImg = new Path
        {
            Stretch = Stretch.Uniform,
            Width = 250,
            Height = 250,
            Opacity = 0.2,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(300, -50, 0, 0),
        };
        msgBoxImg.SetResourceReference(Path.FillProperty, "Default_Major_Foreground");
        if (icon == MessageBoxImage.Hand) msgBoxImg.Data = Geometry.Parse("M7,19H15V22H7V19M16.94,1C16.4,0.91 15.87,1.25 15.76,1.8L14.75,7.57C14.53,7.54 14.29,7.5 14,7.47L13.57,7.5L12.41,1.8C12.31,1.26 11.78,0.91 11.24,1C10.7,1.13 10.35,1.66 10.45,2.2L11.65,8.11L7.85,9.8C7.35,10 7,10.53 7,11.14V15.5C7,16.3 7.73,17 8.5,17H15C15.39,17 15.74,16.84 16,16.57L16.5,16.16C16.5,16.16 17,15.78 17,15.36V13C17,13 17,11.86 16.13,11.3L17.71,2.2C17.83,1.66 17.5,1.13 16.94,1Z");
        if (icon == MessageBoxImage.Question) msgBoxImg.Data = Geometry.Parse("M3.05 13H1V11H3.05C3.5 6.83 6.83 3.5 11 3.05V1H13V3.05C17.17 3.5 20.5 6.83 20.95 11H23V13H20.95C20.5 17.17 17.17 20.5 13 20.95V23H11V20.95C6.83 20.5 3.5 17.17 3.05 13M12 5C8.13 5 5 8.13 5 12S8.13 19 12 19 19 15.87 19 12 15.87 5 12 5M11.13 17.25H12.88V15.5H11.13V17.25M12 6.75C10.07 6.75 8.5 8.32 8.5 10.25H10.25C10.25 9.28 11.03 8.5 12 8.5S13.75 9.28 13.75 10.25C13.75 12 11.13 11.78 11.13 14.63H12.88C12.88 12.66 15.5 12.44 15.5 10.25C15.5 8.32 13.93 6.75 12 6.75Z");
        if (icon == MessageBoxImage.Exclamation) msgBoxImg.Data = Geometry.Parse("M10 3H14V14H10V3M10 21V17H14V21H10Z");
        if (icon == MessageBoxImage.Exclamation) msgBoxImg.Data = Geometry.Parse("M12 2C6.5 2 2 6.5 2 12C2 17.5 6.5 22 12 22C17.5 22 22 17.5 22 12C22 6.5 17.5 2 12 2M12 20C7.61 20 4 16.39 4 12C4 7.61 7.61 4 12 4C16.39 4 20 7.61 20 12C20 16.39 16.39 20 12 20M13 10.27L15.83 8.63L16.83 10.37L14 12L16.83 13.63L15.83 15.37L13 13.73V17H11V13.73L8.17 15.37L7.17 13.63L10 12L7.17 10.37L8.17 8.63L11 10.27V7H13V10.27Z");
        if (icon == MessageBoxImage.Stop) msgBoxImg.Data = Geometry.Parse("M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4M9,9V15H15V9");
        if (icon == MessageBoxImage.Error) msgBoxImg.Data = Geometry.Parse("M11,15H13V17H11V15M11,7H13V13H11V7M12,2C6.47,2 2,6.5 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20Z");
        if (icon == MessageBoxImage.Warning) msgBoxImg.Data = Geometry.Parse("M5,3H19A2,2 0 0,1 21,5V19A2,2 0 0,1 19,21H5A2,2 0 0,1 3,19V5A2,2 0 0,1 5,3M13,13V7H11V13H13M13,17V15H11V17H13Z");
        if (icon == MessageBoxImage.Warning) msgBoxImg.Data = Geometry.Parse("M11,15H13V17H11V15M11,7H13V13H11V7M12,2C6.47,2 2,6.5 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20Z");
        if ((byte)icon == 0x80)
        {
            msgBoxImg.Data = Geometry.Parse("M7,19H15V22H7V19M16.94,1C16.4,0.91 15.87,1.25 15.76,1.8L14.75,7.57C14.53,7.54 14.29,7.5 14,7.47L13.57,7.5L12.41,1.8C12.31,1.26 11.78,0.91 11.24,1C10.7,1.13 10.35,1.66 10.45,2.2L11.65,8.11L7.85,9.8C7.35,10 7,10.53 7,11.14V15.5C7,16.3 7.73,17 8.5,17H15C15.39,17 15.74,16.84 16,16.57L16.5,16.16C16.5,16.16 17,15.78 17,15.36V13C17,13 17,11.86 16.13,11.3L17.71,2.2C17.83,1.66 17.5,1.13 16.94,1Z");
            msgBoxImg.Margin = new Thickness(300, -20, 0, 0);
        }
        if ((byte)icon == 0x81)
        {
            msgBoxImg.Fill = Brushes.GreenYellow;
            msgBoxImg.Opacity = 0.2;
            msgBoxImg.Data = Geometry.Parse("M20,12A8,8 0 0,1 12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4C12.76,4 13.5,4.11 14.2,4.31L15.77,2.74C14.61,2.26 13.34,2 12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12M7.91,10.08L6.5,11.5L11,16L21,6L19.59,4.58L11,13.17L7.91,10.08Z");
            msgBoxImg.Margin = new Thickness(300, -20, 0, 0);
        }

        Grid.SetRowSpan(msgBoxImg, 10);

        contentGrid.Children.Add(messageTextBlock);
        hostGrid.Children.Add(msgBoxImg);
        hostGrid.Children.Add(captionTextBlock);
        hostGrid.Children.Add(contentGrid);
        hostGrid.Children.Add(btnStackPanel);
        hostGrid.Children.Add(titleBorder);
        var borderTh = new Border
        {
            BorderThickness = new Thickness(1, 5, 1, 1),
        };
        borderTh.SetResourceReference(Border.BorderBrushProperty, "Default_Bottom_Background");
        Grid.SetRowSpan(borderTh, 10);
        Grid.SetColumnSpan(borderTh, 10);
        hostGrid.Children.Add(borderTh);
    }

    internal string Title { get; set; }
    internal string Caption { get; set; }
    internal object Content { get; set; }

    internal void CreateHostVirtual(string message, string caption, string title, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defaultResult)
    {
        result = defaultResult;
        _threadWindowHandles = new ArrayList();
        _dialogOwnerHandle = GetActiveWindow();
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
                _dialogOwnerHandle = GetParent(new HandleRef(null, _dialogOwnerHandle));
            }
        }
        EnumThreadWindows(GetCurrentThreadId(), ThreadWindowsCallback, new HandleRef(null, IntPtr.Zero));
        EnableThreadWindows(false);
        var capture = GetCapture();
        if (capture != IntPtr.Zero)
        {
            IntReleaseCapture();
        }
        var thread = new Thread(() =>
        {
            runningDispatcher = Dispatcher.CurrentDispatcher;
            invokeDispatcher?.Invoke(() =>
            {
                GetPopup(message, caption, title, buttons, icon, defaultResult);
            });
            Dispatcher.Run();
            _dispatcherFrame.Continue = false;
        })
        { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        ComponentDispatcher.PushModal();
        _dispatcherFrame = new DispatcherFrame();
        Dispatcher.PushFrame(_dispatcherFrame);
        ComponentDispatcher.PopModal();
        if (_dialogOwnerHandle != IntPtr.Zero)
        {
            SetFocus(_dialogOwnerHandle);
            SetForegroundWindow(_dialogOwnerHandle);
        }
    }

    internal static int WGetWindowLong(HandleRef hWnd, int nIndex)
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

    internal JWMessageBox()
    {
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

    internal static IntPtr WGetParent(HandleRef hWnd)
    {
        IntPtr parent = GetParent(hWnd);
        int lastWin32Error = Marshal.GetLastWin32Error();
        if (parent == IntPtr.Zero && lastWin32Error != 0)
        {
            throw new Win32Exception(lastWin32Error);
        }
        return parent;
    }

    internal bool ThreadWindowsCallback(IntPtr hWnd, IntPtr lparam)
    {
        if (IsWindowVisible(new HandleRef(null, hWnd)) && IsWindowEnabled(new HandleRef(null, hWnd)))
        {
            _threadWindowHandles.Add(hWnd);
        }
        return true;
    }
}
